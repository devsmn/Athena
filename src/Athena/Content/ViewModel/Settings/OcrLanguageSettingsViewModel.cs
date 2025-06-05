using System.Collections.ObjectModel;
using Athena.DataModel.Core;
using Athena.Resources.Localization;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Athena.UI
{
    public partial class OcrLanguageSettingsViewModel : ContextViewModel
    {
        [ObservableProperty]
        private List<OcrLanguage> _allSupportedLanguages;

        [ObservableProperty]
        private ObservableCollection<OcrLanguage> _selectedSupportedLanguages;

        [ObservableProperty]
        private double _estimatedDownloadSize;

        [ObservableProperty]
        private double _estimatedLocalSize;

        private bool _pendingChanges;

        private bool PendingChanges
        {
            get => _pendingChanges;
            set
            {
                _pendingChanges = value;
                UpdateLanguagesCommand.NotifyCanExecuteChanged();
            }
        }

        public OcrLanguageSettingsViewModel()
        {
        }

        public override async Task InitializeAsync()
        {
            EstimatedDownloadSize = 0d;
            EstimatedLocalSize = 0d;
            PendingChanges = false;

            await ExecuteAsyncBackgroundAction(async context =>
            {
                context.Log(Localization.OcrRetrievingInstalledLanguages);

                Dictionary<string, OcrLanguage> tmpList = new();

                await Task.Run(() =>
                {
                    tmpList.Add("afr", new("afr", "Afrikaans", 253));
                    tmpList.Add("amh", new("amh", "Amharic", 522));
                    tmpList.Add("ara", new("ara", "Arabic", 137));
                    tmpList.Add("asm", new("asm", "Assamese", 195));
                    tmpList.Add("aze", new("aze", "Azerbaijani", 336));
                    tmpList.Add("aze_cyrl", new("aze_cyrl", "Azerbaijani (Cyrillic)", 185));
                    tmpList.Add("bel", new("bel", "Belarusian", 352));
                    tmpList.Add("ben", new("ben", "Bengali", 100));
                    tmpList.Add("bod", new("bod", "Tibetan", 188));
                    tmpList.Add("bos", new("bos", "Bosnian", 238));
                    tmpList.Add("bre", new("bre", "Breton", 604));
                    tmpList.Add("bul", new("bul", "Bulgarian", 160));
                    tmpList.Add("cat", new("cat", "Catalan; Valencian", 109));
                    tmpList.Add("ceb", new("ceb", "Cebuano", 100));
                    tmpList.Add("ces", new("ces", "Czech", 362));
                    tmpList.Add("chi_sim", new("chi_sim", "Chinese (Simplified)", 235));
                    tmpList.Add("chi_sim_vert", new("chi_sim_vert", "Chinese (Simplified, vertical)", 184));
                    tmpList.Add("chi_tra", new("chi_tra", "Chinese (Traditional)", 226));
                    tmpList.Add("chi_tra_vert", new("chi_tra_vert", "Chinese (Traditional, vertical)", 174));
                    tmpList.Add("chr", new("chr", "Cherokee", 100));
                    tmpList.Add("cos", new("cos", "Corsican", 219));
                    tmpList.Add("cym", new("cym", "Welsh", 211));
                    tmpList.Add("dan", new("dan", "Danish", 246));
                    tmpList.Add("deu", new("deu", "German", 145));
                    tmpList.Add("deu_latf", new("deu_latf", "German Fraktur", 613));
                    tmpList.Add("div", new("div", "Dhivehi", 169));
                    tmpList.Add("dzo", new("dzo", "Dzongkha", 100));
                    tmpList.Add("ell", new("ell", "Greek, Modern (1453-)", 135));
                    tmpList.Add("eng", new("eng", "English", 392));
                    tmpList.Add("enm", new("enm", "English, Middle (1100-1500)", 296));
                    tmpList.Add("epo", new("epo", "Esperanto", 451));
                    tmpList.Add("equ", new("equ", "Math / equation detection module", 215));
                    tmpList.Add("est", new("est", "Estonian", 425));
                    tmpList.Add("eus", new("eus", "Basque", 494));
                    tmpList.Add("fao", new("fao", "Faroese", 328));
                    tmpList.Add("fas", new("fas", "Persian", 100));
                    tmpList.Add("fil", new("fil", "Filipino", 176));
                    tmpList.Add("fin", new("fin", "Finnish", 75));
                    tmpList.Add("fra", new("fra", "French", 108));
                    tmpList.Add("frm", new("frm", "French, Middle (ca. 1400-1600)", 193));
                    tmpList.Add("fry", new("fry", "Western Frisian", 182));
                    tmpList.Add("gla", new("gla", "Gaelic; Scottish Gaelic", 293));
                    tmpList.Add("gle", new("gle", "Irish", 113));
                    tmpList.Add("glg", new("glg", "Galician", 244));
                    tmpList.Add("grc", new("grc", "Greek, Ancient (-1453)", 214));
                    tmpList.Add("guj", new("guj", "Gujarati", 135));
                    tmpList.Add("hat", new("hat", "Haitian; Haitian Creole", 189));
                    tmpList.Add("heb", new("heb", "Hebrew", 100));
                    tmpList.Add("hin", new("hin", "Hindi", 107));
                    tmpList.Add("hrv", new("hrv", "Croatian", 391));
                    tmpList.Add("hun", new("hun", "Hungarian", 505));
                    tmpList.Add("hye", new("hye", "Armenian", 330));
                    tmpList.Add("iku", new("iku", "Inuktitut", 267));
                    tmpList.Add("ind", new("ind", "Indonesian", 107));
                    tmpList.Add("isl", new("isl", "Icelandic", 217));
                    tmpList.Add("ita", new("ita", "Italian", 258));
                    tmpList.Add("ita_old", new("ita_old", "Italian - Old", 313));
                    tmpList.Add("jav", new("jav", "Javanese", 284));
                    tmpList.Add("jpn", new("jpn", "Japanese", 236));
                    tmpList.Add("jpn_vert", new("jpn_vert", "Japanese (vertical)", 290));
                    tmpList.Add("kan", new("kan", "Kannada", 344));
                    tmpList.Add("kat", new("kat", "Georgian", 241));
                    tmpList.Add("kat_old", new("kat_old", "Georgian - Old", 100));
                    tmpList.Add("kaz", new("kaz", "Kazakh", 452));
                    tmpList.Add("khm", new("khm", "Central Khmer", 138));
                    tmpList.Add("kir", new("kir", "Kirghiz; Kyrgyz", 947));
                    tmpList.Add("kmr", new("kmr", "Kurdish, Northern", 340));
                    tmpList.Add("kor", new("kor", "Korean", 160));
                    tmpList.Add("kor_vert", new("kor_vert", "Korean (vertical)", 106));
                    tmpList.Add("lao", new("lao", "Lao", 609));
                    tmpList.Add("lat", new("lat", "Latin", 304));
                    tmpList.Add("lav", new("lav", "Latvian", 259));
                    tmpList.Add("lit", new("lit", "Lithuanian", 301));
                    tmpList.Add("ltz", new("ltz", "Luxembourgish; Letzeburgesch", 249));
                    tmpList.Add("mal", new("mal", "Malayalam", 503));
                    tmpList.Add("mar", new("mar", "Marathi", 202));
                    tmpList.Add("mkd", new("mkd", "Macedonian", 153));
                    tmpList.Add("mlt", new("mlt", "Maltese", 220));
                    tmpList.Add("mon", new("mon", "Mongolian", 204));
                    tmpList.Add("mri", new("mri", "Maori", 100));
                    tmpList.Add("msa", new("msa", "Malay", 167));
                    tmpList.Add("mya", new("mya", "Burmese", 443));
                    tmpList.Add("nep", new("nep", "Nepali", 100));
                    tmpList.Add("nld", new("nld", "Dutch; Flemish", 577));
                    tmpList.Add("nor", new("nor", "Norwegian", 344));
                    tmpList.Add("oci", new("oci", "Occitan (post 1500)", 603));
                    tmpList.Add("ori", new("ori", "Oriya", 141));
                    tmpList.Add("osd", new("osd", "Orientation and script detection module", 1001));
                    tmpList.Add("pan", new("pan", "Panjabi; Punjabi", 100));
                    tmpList.Add("pol", new("pol", "Polish", 454));
                    tmpList.Add("por", new("por", "Portuguese", 189));
                    tmpList.Add("pus", new("pus", "Pushto; Pashto", 169));
                    tmpList.Add("que", new("que", "Quechua", 479));
                    tmpList.Add("ron", new("ron", "Romanian; Moldavian; Moldovan", 227));
                    tmpList.Add("rus", new("rus", "Russian", 368));
                    tmpList.Add("san", new("san", "Sanskrit", 1180));
                    tmpList.Add("sin", new("sin", "Sinhala; Sinhalese", 165));
                    tmpList.Add("slk", new("slk", "Slovak", 422));
                    tmpList.Add("slv", new("slv", "Slovenian", 286));
                    tmpList.Add("snd", new("snd", "Sindhi", 162));
                    tmpList.Add("spa", new("spa", "Spanish; Castilian", 219));
                    tmpList.Add("spa_old", new("spa_old", "Spanish; Castilian - Old", 276));
                    tmpList.Add("sqi", new("sqi", "Albanian", 179));
                    tmpList.Add("srp", new("srp", "Serbian", 205));
                    tmpList.Add("srp_latn", new("srp_latn", "Serbian (Latin)", 313));
                    tmpList.Add("sun", new("sun", "Sundanese", 131));
                    tmpList.Add("swa", new("swa", "Swahili", 207));
                    tmpList.Add("swe", new("swe", "Swedish", 397));
                    tmpList.Add("syr", new("syr", "Syriac", 210));
                    tmpList.Add("tam", new("tam", "Tamil", 309));
                    tmpList.Add("tat", new("tat", "Tatar", 102));
                    tmpList.Add("tel", new("tel", "Telugu", 264));
                    tmpList.Add("tgk", new("tgk", "Tajik", 248));
                    tmpList.Add("tha", new("tha", "Thai", 102));
                    tmpList.Add("tir", new("tir", "Tigrinya", 100));
                    tmpList.Add("ton", new("ton", "Tonga (Tonga Islands)", 100));
                    tmpList.Add("tur", new("tur", "Turkish", 434));
                    tmpList.Add("uig", new("uig", "Uighur; Uyghur", 266));
                    tmpList.Add("ukr", new("ukr", "Ukrainian", 365));
                    tmpList.Add("urd", new("urd", "Urdu", 133));
                    tmpList.Add("uzb", new("uzb", "Uzbek", 617));
                    tmpList.Add("uzb_cyrl", new("uzb_cyrl", "Uzbek (Cyrillic)", 149));
                    tmpList.Add("vie", new("vie", "Vietnamese", 100));
                    tmpList.Add("yid", new("yid", "Yiddish", 100));
                    tmpList.Add("yor", new("yor", "Yoruba", 100));

                    IOcrService service = Services.GetService<IOcrService>();
                    var languages = service.GetInstalledLanguages(context);

                    if (languages != null)
                    {
                        foreach (string lan in languages)
                        {
                            if (tmpList.TryGetValue(lan, out var item))
                            {
                                EstimatedLocalSize += item.Size;
                                item.IsInstalled = true;
                            }
                        }
                    }
                });

                AllSupportedLanguages = new(tmpList.Values.OrderBy(x => x.DisplayName));
                SelectedSupportedLanguages = new(AllSupportedLanguages.Where(x => x.IsInstalled));
                DeleteLocalFilesCommand.NotifyCanExecuteChanged();
            });
        }

        [RelayCommand]
        private void IsCheckedChanged()
        {
            if (IsBusy)
                return;

            PendingChanges = true;

            EstimatedDownloadSize = SelectedSupportedLanguages
                .Where(x => !x.IsInstalled)
                .Sum(x => x.Size);

            EstimatedLocalSize = SelectedSupportedLanguages
                .Where(x => x.IsInstalled)
                .Sum(x => x.Size);
        }

        private bool CanDeleteLocalFiles()
        {
            return AllSupportedLanguages?.Any(x => x.IsInstalled) == true;
        }

        [RelayCommand(CanExecute = nameof(CanDeleteLocalFiles))]
        private async Task DeleteLocalFiles()
        {
            IOcrService ocrService = Services.GetService<IOcrService>();
            IContext context = RetrieveReportContext();

            var toDelete = AllSupportedLanguages.Where(x => x.IsInstalled).ToList();

            bool yes = await DisplayAlert(
                Localization.OcrDeleteInstalledLanguagesConfirmTitle,
                string.Format(Localization.OcrDeleteInstalledLanguagesConfirmText, toDelete.Count, toDelete.Sum(x => x.Size)),
                Localization.Yes,
                Localization.No);

            if (!yes)
                return;

            await DeleteLocalFilesCore(context, ocrService);
        }

        private async Task DeleteLocalFilesCore(IContext passedContext, IOcrService ocrService)
        {
            await ExecuteAsyncBackgroundAction(async context =>
            {
                ocrService.DeleteInstalledLanguages(passedContext);
                ocrService.Reset();
                await InitializeAsync();

            });

            await Toast.Make(Localization.OcrInstalledLanguagesDeletedSuccessfully).Show();

        }

        public bool CanExecuteSave()
        {
            return _pendingChanges;
        }

        [RelayCommand(CanExecute = nameof(CanExecuteSave))]
        private async Task UpdateLanguages()
        {
            var filesToDownload = SelectedSupportedLanguages
                .Where(x => !x.IsInstalled)
                .ToList();

            var filesToDelete = AllSupportedLanguages
                .Where(x => x.IsInstalled)
                .Except(SelectedSupportedLanguages)
                .ToList();

            INetworkService networkService = Services.GetService<INetworkService>();
            bool yes;

            if (filesToDownload.Count > 0)
            {
                if (!networkService.IsInternetAccessPossible())
                {
                    await DisplayAlert(
                        Localization.OcrNotInternetTitle,
                        Localization.OcrNoInternetText,
                        "Ok",
                        string.Empty);

                    filesToDownload.Clear();
                }

                if (filesToDownload.Count > 0 && !networkService.IsWifi())
                {
                    yes = await DisplayAlert(
                        Localization.OcrNoWlanConnectionTitle,
                        string.Format(Localization.OcrNoWlanConnectionText, Math.Round(EstimatedDownloadSize / 100.0, 2)),
                        Localization.Yes,
                        Localization.No);

                    if (!yes)
                        filesToDownload.Clear();
                }
            }

            if (filesToDownload.Count == 0 && filesToDelete.Count == 0)
                return;

            string downloadInfo = string.Format(Localization.OcrDownloadLanguagesInfo, filesToDownload.Count, Math.Round(EstimatedDownloadSize / 100.0, 2));
            string deleteInfo = string.Format(Localization.OcrDeleteLanguageInfo, filesToDelete.Count, Math.Round(filesToDelete.Sum(x => x.Size / 100.0), 2));

            string info = Localization.OcrSaveLanguagesPrefix;

            if (filesToDelete.Count > 0 && filesToDownload.Count > 0)
            {
                info += $" {downloadInfo} {Localization.OcrSaveAnd} {deleteInfo}";
            }
            else if (filesToDelete.Count > 0 && filesToDownload.Count == 0)
            {
                info += $" {deleteInfo}";
            }
            else
            {
                info += $" {downloadInfo}";
            }

            yes = await DisplayAlert(
                Localization.OcrSaveLanguagesTitle,
                string.Format(Localization.OcrSaveLanguagesText, info),
                Localization.Yes,
                Localization.No);

            if (!yes)
                return;

            await Save(filesToDownload, filesToDelete);
        }

        private async Task Save(List<OcrLanguage> filesToDownload, List<OcrLanguage> filesToDelete)
        {
            await ExecuteAsyncBackgroundAction(async context =>
            {
                IOcrService ocrService = Services.GetService<IOcrService>();

                await Task.Run(async () =>
                {
                    if (filesToDownload.Count > 0)
                    {
                        context.Log(Localization.OcrDownloadingLanguage);

                        await ocrService.DownloadLanguagesAsync(
                            context,
                            filesToDownload.Select(x => x.ShortName));
                    }

                    if (filesToDelete.Count > 0)
                    {
                        context.Log(Localization.OcrDeletingLanguages);

                        ocrService.DeleteInstalledLanguages(
                            context,
                            filesToDelete.Select(x => x.ShortName));
                    }

                    ocrService.Reset();
                    await InitializeAsync();
                });
            });

            await Toast.Make(Localization.OcrLanguagesSaved).Show();
        }


        public async Task<bool> CanClose()
        {
            if (PendingChanges)
            {
                bool yes = await DisplayAlert(
                    Localization.OcrLanguagesPendingChangesTitle,
                    Localization.OcrLanguagesPendingChangesText,
                    Localization.Yes,
                    Localization.No);

                if (!yes)
                    return false; // Handled, prevent leaving page.
            }

            // Not handled, leaving page is allowed.
            return true;
        }
    }
}
