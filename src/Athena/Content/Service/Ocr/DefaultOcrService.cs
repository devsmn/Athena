using System.Text;
using Athena.DataModel.Core;
using Athena.Resources.Localization;
using TesseractOcrMaui;
using TesseractOcrMaui.Enums;
using TesseractOcrMaui.Exceptions;
using TesseractOcrMaui.Results;

namespace Athena.UI
{
    public class DefaultOcrService : IOcrService
    {
        private OcrError _error;
        private string _installedLanguages;
        private string _dataDirectory;
        private bool _dataValidated;

        public OcrError Error
        {
            get
            {
                EnsureDataValidated();
                return _error;
            }

            private set => _error = value;
        }


        private void ValidateData(IContext context = null)
        {
            context?.Log(Localization.ValidatingInstalledLanguages);
            _installedLanguages = string.Empty;
            _dataDirectory = Path.Combine(FileSystem.Current.AppDataDirectory, "trainedOcrData");
            _error = OcrError.None;

            if (!Directory.Exists(_dataDirectory))
            {
                Directory.CreateDirectory(_dataDirectory);
                Error = OcrError.TrainedDataDirectoryEmpty;
                return;
            }

            string[] files = Directory.GetFiles(_dataDirectory);
            StringBuilder languages = new();

            if (files.Length == 0)
            {
                Error = OcrError.TrainedDataDirectoryEmpty;
                return;
            }

            for (int i = 0; i < files.Length; i++)
            {
                string lan = Path.GetFileNameWithoutExtension(files[i]);
                languages.Append($"{lan}+");
            }

            _installedLanguages = languages.ToString();

            if (!string.IsNullOrEmpty(_installedLanguages))
            {
                _installedLanguages = _installedLanguages.Substring(0, _installedLanguages.Length - 1);
                return;
            }

            Error = OcrError.TrainedDataDirectoryEmpty;
        }

        private TessEngine CreateNewEngine()
        {
            ValidateData();

            if (Error == OcrError.None)
                return new TessEngine(_installedLanguages, _dataDirectory);

            return null;
        }

        public void Reset()
        {
            _dataValidated = false;
        }

        private void EnsureDataValidated()
        {
            if (!_dataValidated)
                ValidateData();
        }

        public string[] GetInstalledLanguages(IContext context)
        {
            context.Log(Localization.OcrRetrievingInstalledLanguages);

            EnsureDataValidated();

            if (string.IsNullOrEmpty(_installedLanguages))
                return null;

            return _installedLanguages.Split('+', StringSplitOptions.RemoveEmptyEntries);
        }

        public void DeleteInstalledLanguages(IContext context)
        {
            EnsureDataValidated();

            string[] files = Directory.GetFiles(_dataDirectory);

            foreach (string file in files)
            {
                DeleteLanguageCore(context, file);
            }
        }

        private void DeleteLanguageCore(IContext context, string path)
        {
            try
            {
                context.Log(string.Format(Localization.DeletingLanguage, path));
                File.Delete(path);
            }
            catch (Exception ex)
            {
                context.Log(ex);
            }
        }

        public void DeleteInstalledLanguages(IContext context, IEnumerable<string> languages)
        {
            EnsureDataValidated();

            string[] files = Directory.GetFiles(_dataDirectory);
            Dictionary<string, string> localFiles = files.ToDictionary(Path.GetFileNameWithoutExtension);

            foreach (string lan in languages)
            {
                if (!localFiles.TryGetValue(lan, out string fullPath))
                    continue;

                DeleteLanguageCore(context, fullPath);
            }
        }

        public async Task DownloadLanguagesAsync(IContext context, IEnumerable<string> languages)
        {
            EnsureDataValidated();

            IDownloadService downloadService = Services.GetService<IDownloadService>();

            string url = @"https://github.com/tesseract-ocr/tessdata_fast/raw/main/{0}.traineddata";

            foreach (string lan in languages)
            {
                context?.Log(string.Format(Localization.OcrDownloadingLanguage, lan));

                await downloadService.DownloadAsync(
                    context,
                    string.Format(url, lan),
                    Path.Combine(_dataDirectory, $"{lan}.traineddata"));
            }
        }

        public async Task<RecognizionResult> RecognizeTextAsync(string path)
        {
            try
            {
                using (Pix pix = Pix.LoadFromFile(path))
                {
                    return await Task.Run(() => Recognize(pix));
                }
            }
            catch (IOException)
            {
                return new()
                {
                    Status = RecognizionStatus.InvalidImage,
                    Message = "Invalid image, cannot be loaded"
                };
            }
        }

        private RecognizionResult Recognize(Pix pix)
        {
            string? text;
            float confidence;


            try
            {
                using (TessEngine engine = CreateNewEngine())
                {
                    using (TessPage page = engine.ProcessImage(pix))
                    {
                        // SegMode can't be OsdOnly in here.
                        confidence = page.GetConfidence();

                        // SegMode can't be OsdOnly in here.
                        text = page.GetText();
                    }
                }
            }
            catch (DllNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                (RecognizionStatus status, string message) = ex switch
                {
                    PageNotDisposedException => (RecognizionStatus.ImageAlredyProcessed,
                        "Old image TessPage must be disposed after one (1) use"),
                    InvalidBytesException => (RecognizionStatus.CannotRecognizeText,
                        "Invalid bytes in recognized text, see inner exception"),
                    TesseractInitException => (RecognizionStatus.Failed,
                        "Invalid data to init Tesseract Engine, see exception"),
                    StringMarshallingException => (RecognizionStatus.InvalidResultString,
                        "Native library returned invalid string, please file bug report with input image as attachment"),
                    TesseractException => (RecognizionStatus.CannotRecognizeText,
                        "Library cannot get thresholded image when recognizing"),
                    ArgumentException => (RecognizionStatus.InvalidImage,
                        "Cannot process Pix image, height or width has invalid values."),
                    not null => (RecognizionStatus.UnknowError,
                        $"Failed to ocr for unknown reason '{ex.GetType().Name}': '{ex.Message}'.")
                };
                return new RecognizionResult
                {
                    Status = status,
                    Message = message,
                    Exception = ex
                };
            }

            return new RecognizionResult
            {
                Status = RecognizionStatus.Success,
                RecognisedText = text,
                Confidence = confidence,
            };
        }
    }
}

