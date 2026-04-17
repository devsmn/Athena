using Athena.Content.Views;
using Athena.Data.Core;
using Athena.Data.SQLite.Proxy;
using Athena.DataModel;
using Athena.DataModel.Core;
using Athena.Resources.Localization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Athena.UI
{
    public partial class HomeViewModel : ContextViewModel
    {
        private const int MAX_RECENT_DOCUMENTS = 5;

        [ObservableProperty]
        private string _dataOverviewText;

        [ObservableProperty]
        private string _lastUpdatedText;

        [ObservableProperty]
        private int _folderCount;

        [ObservableProperty]
        private int _documentCount;

        [ObservableProperty]
        private string _greeting;

        [ObservableProperty]
        private string _userName;

        [ObservableProperty]
        private DocumentViewModel _selectedItem;

        [ObservableProperty]
        private VisualCollection<DocumentViewModel, Document> _recentDocuments;

        public HomeViewModel()
        {
            UpdateGreeting();
            RecentDocuments = new();
        }

        private void UpdateGreeting()
        {
            IGreetingService greeting = Services.GetService<IGreetingService>();
            Greeting = greeting.Get();

            string name = Services.GetService<IPreferencesService>().GetName();

            if (!string.IsNullOrEmpty(name))
            {
                Greeting += ",";
                UserName = name;
            }
        }

        [RelayCommand]
        public async Task ItemClicked()
        {
            await PushAsync(new DocumentDetailsView(null, SelectedItem.Document));
            SelectedItem = null;
        }

        private bool UpdateCounterStats()
        {
            IContext context = RetrieveContext();

            int folderCount = Folder.CountAll(context);
            int docCount = Document.CountAll(context);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                string folder = folderCount == 1 ? Localization.Folder : Localization.Folders;
                string doc = docCount == 1 ? Localization.Document : Localization.Documents;

                DataOverviewText = string.Format(Localization.DocFolderOverviewText, docCount, doc, folderCount, folder);
                LastUpdatedText = string.Format(Localization.DocFolderOverviewTextLastUpdate, DateTime.Now.ToShortTimeString());
                FolderCount = folderCount;
                DocumentCount = docCount;

                UpdateGreeting();
            });

            return true;
        }

        [RelayCommand]
        public async Task OpenFolderOverview()
        {
            FolderViewModel rootFolder = Services.GetService<IRootFolderService>().GetRootFolder();
            await PushAsync(new FolderOverview(rootFolder));
        }

        public async Task CheckFirstUsage()
        {
            IPreferencesService prefService = Services.GetService<IPreferencesService>();

            bool isFirstUsage = prefService.IsFirstUsage();

            if (isFirstUsage)
            {
                DefaultContentPage view = new WelcomeView();
                await PushModalAsync(view);
                await view.DoneTcs.Task;
                await PopModalAsync();
            }
            else
            {
                await CheckToSChange();
            }
        }

        public async Task CheckToSChange()
        {
            IPreferencesService prefService = Services.GetService<IPreferencesService>();

            int lastVersion = prefService.GetLastTermsOfUseVersion();

            if (lastVersion != IPreferencesService.ToSVersion)
            {
                prefService.SetLastTermsOfUseVersion(IPreferencesService.ToSVersion);
                await PushModalAsync(new ToSChangedView());
            }
        }

        private void LogMetaHeaderInfo(IContext context)
        {
            var deviceInfo = DeviceInfo.Current;
            var displayInfo = DeviceDisplay.Current.MainDisplayInfo;

            IPreferencesService prefService = Services.GetService<IPreferencesService>();

            context.Log(
                $"appVersion=[{AppInfo.Current.VersionString}-{AppInfo.Current.BuildString}], " +
                $"model=[{deviceInfo.Model}], " +
                $"idiom=[{deviceInfo.Idiom}], " +
                $"manufacturer=[{deviceInfo.Manufacturer}], " +
                $"name=[{deviceInfo.Name}], " +
                $"version=[{deviceInfo.Version}], " +
                $"versionString=[{deviceInfo.VersionString}]");

            context.Log(Environment.NewLine);

            context.Log(
                $"density=[{displayInfo.Density}], " +
                $"height=[{displayInfo.Height}], " +
                $"r=[{displayInfo.Width}], " +
                $"orientation=[{displayInfo.Orientation}], " +
                $"refreshRate=[{displayInfo.RefreshRate}], " +
                $"rotation=[{displayInfo.Rotation}]");

            context.Log(Environment.NewLine);

            context.Log(
                $"IsFirstUsage=[{prefService.IsFirstUsage()}], " +
                $"lan=[{prefService.GetLanguage()}], " +
                $"lastTosVersion=[{prefService.GetLastTermsOfUseVersion()}], " +
                $"lastVersion=[{prefService.GetLastUsedVersion()}], " +
                $"name=[{prefService.GetName()}], " +
                $"isFirstScannerUsage=[{prefService.IsFirstScannerUsage()}], " +
                $"useAdvancedScanner=[{prefService.GetUseAdvancedScanner()}]");

            context.Log(Environment.NewLine);
        }

        public new async Task<bool> InitializeAsync()
        {
            ICompatibilityService compatService = Services.GetService<ICompatibilityService>();

            IContext context = RetrieveReportContext();
            LogMetaHeaderInfo(context);

            IsBusy = true;
            await Task.Run(async () =>
            {
                await Task.Delay(200);

                SqliteProxy sqlProxy = new();
                SqLiteProxyParameter parameter = new SqLiteProxyParameter { MinimumVersion = new Version(0, 1) };

                await DataStore.CloseAllAsync(context);
                DataStore.Clear();

                IDataProviderPatcher sqlPatcher = sqlProxy.RequestPatcher();
                IDataProviderAuthenticator sqlAuth = sqlProxy.RequestAuthenticator();

                // First, register available patches.
                sqlPatcher.RegisterPatches(compatService);

                // Execute the patches before initializing the repositories.
                await sqlPatcher.ExecutePatchesAsync(context, compatService);

                context.Log("Requesting biometric access to database");

                // Unlock access to the database. Needs to be done before initializing the repositories.
                IDataEncryptionService encryptionService = Services.GetService<IDataEncryptionService>();
                IHardwareKeyStoreService keyService = Services.GetService<IHardwareKeyStoreService>();
                IPreferencesService prefService = Services.GetService<IPreferencesService>();

                EncryptionMethod method = prefService.GetEncryptionMethod();
                if (method == EncryptionMethod.Undefined)
                {
                    // Method has not been set yet.
                    prefService.SetEncryptionMethod(keyService.BiometricsAvailable()
                        ? EncryptionMethod.Biometrics
                        : EncryptionMethod.Password);

                    method = prefService.GetEncryptionMethod();
                }

                bool primarySucceeded = false;
                string alias = await encryptionService.GetActiveAliasAsync();

                if (method == EncryptionMethod.Biometrics)
                {
                    bool retryPrimary = true;

                    do
                    {
                        primarySucceeded = await encryptionService.ReadPrimaryAsync(
                            context,
                            alias,
                            key => parameter.Cipher = key,
                            context.Log,
                            () => retryPrimary = false);

                        if (primarySucceeded)
                            retryPrimary = false;

                    } while (retryPrimary);
                }

                if (!primarySucceeded)
                {
                    IPasswordService passwordService = Services.GetService<IPasswordService>();
                    bool firstTry = true;

                    do
                    {
                        context.Log("Requesting fallback access to database");
                        string pin = string.Empty;
                        await passwordService.Prompt(context, !firstTry, (str) => pin = str);

                        await encryptionService.ReadFallbackAsync(
                            context,
                            alias,
                            pin, key => parameter.Cipher = key,
                            error => context.Log(error));

                        firstTry = false;

                    } while (!await sqlAuth.AuthenticateAsync(parameter.Cipher));
                }

                // Register the repositories.
                DataStore.Register(sqlProxy.Request<IFolderRepository>(parameter));
                DataStore.Register(sqlProxy.Request<IDocumentRepository>(parameter));
                DataStore.Register(sqlProxy.Request<IChapterRepository>(parameter));
                DataStore.Register(sqlProxy.Request<ITagRepository>(parameter));

                context.Log("Initializing repositories");
                await DataStore.InitializeAsync(context, () => context.Log("Invalid cipher"));

                IRootFolderService service = Services.GetService<IRootFolderService>();
                Folder rootFolder = GetRootFolder(context);

                if (prefService.GetLastUsedVersion() != compatService.GetCurrentVersion())
                {
                    await PushModalAsync(new WebViewPage("https://athena.devsmn.de/app_latest_release/"));
                }

                service.SetRootFolder(rootFolder);
                UpdateCounterStats();

                RecentDocuments = new VisualCollection<DocumentViewModel, Document>(
                    Document.ReadRecent(context, MAX_RECENT_DOCUMENTS).Select(x => new DocumentViewModel(x)));

                Application.Current.Dispatcher.StartTimer(TimeSpan.FromMinutes(5), UpdateCounterStats);

                compatService.UpdateLastUsedVersion(context);
                MainThread.BeginInvokeOnMainThread(() => IsBusy = false);
            });

            return true;
        }

        private static Folder GetRootFolder(IContext context)
        {
            Folder rootFolder = Folder.Read(context, IntegerEntityKey.Root);

            if (rootFolder != null)
                return rootFolder;

            Folder.CreateRoot(context);
            return Folder.Read(context, IntegerEntityKey.Root);
        }
    }
}
