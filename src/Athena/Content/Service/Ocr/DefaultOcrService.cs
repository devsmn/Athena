using System.Text;
using Android.Net;
using TesseractOcrMaui;
using TesseractOcrMaui.Enums;
using TesseractOcrMaui.Exceptions;
using TesseractOcrMaui.Results;

namespace Athena.UI
{
    public class DefaultOcrService : IOcrService
    {
        private TessEngine _engine;
        private OcrError _error;
        private string _supportedLanguages;
        private string _dataDirectory;
        private bool _dataValidated;

        private TessEngine Engine => _engine ??= CreateNewEngine();

        public OcrError Error
        {
            get
            {
                if (!_dataValidated)
                    ValidateData();

                return _error;
            }
            private set => _error = value;
        }


        private void ValidateData()
        {
            _supportedLanguages = string.Empty;
            _dataDirectory = string.Empty;

            string dataFolder = Path.Combine(FileSystem.Current.AppDataDirectory, "trainedOcrData");

            if (!Directory.Exists(dataFolder))
            {
                Error = OcrError.TrainedDataDirectoryMissing;
                return;
            }

            string[] files = Directory.GetFiles(dataFolder);
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

            _supportedLanguages = languages.ToString();

            if (!string.IsNullOrEmpty(_supportedLanguages))
            {
                _supportedLanguages = _supportedLanguages.Substring(0, _supportedLanguages.Length - 2);
                return;
            }

            Error = OcrError.TrainedDataDirectoryEmpty;
        }

        private TessEngine CreateNewEngine()
        {
            ValidateData();

            if (Error == OcrError.None)
                return new TessEngine(_dataDirectory, _supportedLanguages);

            return null;
        }

        public void Reset()
        {
            _engine = null;
            _dataValidated = false;
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
            string? text = null;
            float confidence = -1f;

            if (Engine == null)
            {
                return new RecognizionResult
                {
                    Status = RecognizionStatus.Failed
                };
            }

            try
            {
                using (var page = Engine.ProcessImage(pix))
                {
                    // SegMode can't be OsdOnly in here.
                    confidence = page.GetConfidence();

                    // SegMode can't be OsdOnly in here.
                    text = page.GetText();
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

