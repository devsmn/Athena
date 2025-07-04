using System.Diagnostics;
using Android.Runtime;
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

        protected override void OnAppInitialized()
        {
            IContext context = RetrieveContext();

            UpdateCounterStats();

            RecentDocuments = new VisualCollection<DocumentViewModel, Document>(
                Document.ReadRecent(context, MAX_RECENT_DOCUMENTS).Select(x => new DocumentViewModel(x)));

            Application.Current.Dispatcher.StartTimer(TimeSpan.FromMinutes(5), UpdateCounterStats);
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

        protected override void OnDataPublished(DataPublishedEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (e.Documents.Count > 0)
                {
                    foreach (RequestUpdate<Document> update in e.Documents)
                    {
                        if (update.Type == UpdateType.Add)
                        {
                            if (RecentDocuments.Count >= MAX_RECENT_DOCUMENTS)
                            {
                                RecentDocuments.RemoveAt(0);
                                RecentDocuments.Insert(0, new DocumentViewModel(update.Entity));
                            }
                            else
                            {
                                RecentDocuments.Process(update);
                            }
                        }
                        else if (update.Type == UpdateType.Edit)
                        {
                            RecentDocuments.Process(update);
                        }
                        else if (update.Type == UpdateType.Delete)
                        {
                            FolderViewModel rootFolder = Services.GetService<IDataBrokerService>().GetRootFolder();
                            Stack<Folder> folders = new Stack<Folder>();

                            bool stop = false;

                            foreach (Folder folder in rootFolder.LoadedFolders)
                            {
                                folders.Push(folder);
                            }

                            folders.Push(rootFolder);

                            while (folders.Count > 0)
                            {
                                Folder currentFolder = folders.Pop();

                                foreach (Document folderDoc in currentFolder.LoadedDocuments)
                                {
                                    if (folderDoc.Key.Id == update.Entity.Id)
                                    {
                                        currentFolder.ResetDocumentsLoaded();
                                        stop = true;
                                        break;
                                    }
                                }

                                if (stop)
                                    break;

                                foreach (Folder folder in currentFolder.LoadedFolders)
                                {
                                    folders.Push(folder);
                                }
                            }

                            RecentDocuments.Process(update);
                        }
                    }
                }

                if (e.Tags.Count > 0)
                {
                    HashSet<int> deletedTagIds = e.Tags
                        .Where(x => x.Type == UpdateType.Delete)
                        .Select(x => x.Entity.Id)
                        .ToHashSet();

                    foreach (DocumentViewModel document in RecentDocuments)
                    {
                        List<Tag> validTags = document.Tags.Where(x => !deletedTagIds.Contains(x.Id)).ToList();

                        document.Tags.Clear();

                        foreach (Tag tag in validTags)
                        {
                            document.Tags.Add(tag);
                        }
                    }
                }
            });
        }

        [RelayCommand]
        public async Task OpenFolderOverview()
        {
            FolderViewModel rootFolder = Services.GetService<IDataBrokerService>().GetRootFolder();
            await PushAsync(new FolderOverview(rootFolder));
        }

        public async Task CheckFirstUsage()
        {
            IPreferencesService prefService = Services.GetService<IPreferencesService>();

            bool isFirstUsage = prefService.IsFirstUsage();

            if (isFirstUsage)
            {
                await PushModalAsync(new WelcomeView());
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

        public async Task InitializeAsync()
        {
            ICompatibilityService compatService = Services.GetService<ICompatibilityService>();

            // Data will be initialized via the welcome view because it creates the home view again.
            if (Services.GetService<IPreferencesService>().IsFirstUsage())
                return;

            IsBusy = true;
            IContext context = RetrieveReportContext();

            await Task.Run(async () =>
            {
                await Task.Delay(200);

                SqliteProxy sqlProxy = new();
                SqLiteProxyParameter parameter = new SqLiteProxyParameter { MinimumVersion = new Version(0, 1) };

                Services.GetService<IDataBrokerService>().PrepareForLoading();

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
                bool primarySucceeded = await encryptionService.ReadPrimaryAsync(context, IDataEncryptionService.DatabaseAlias, key => parameter.Cipher = key, _ => { });

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
                            IDataEncryptionService.DatabaseAlias,
                            pin, key => parameter.Cipher = key,
                            error =>
                            {
                                //INavigationService navService = Services.GetService<INavigationService>();
                                //MainThread.BeginInvokeOnMainThread(async () =>
                                //    await navService.DisplayAlert("Error", $"The data could not be decrypted: {error}",
                                //        "Ok", "Close"));
                            });

                        firstTry = false;

                    } while (await sqlAuth.AuthenticateAsync(parameter.Cipher) == false);
                }

                // Register the repositories.
                DataStore.Register(sqlProxy.Request<IFolderRepository>(parameter));
                DataStore.Register(sqlProxy.Request<IDocumentRepository>(parameter));
                DataStore.Register(sqlProxy.Request<IChapterRepository>(parameter));
                DataStore.Register(sqlProxy.Request<ITagRepository>(parameter));

                context.Log("Initializing repositories");
                await DataStore.InitializeAsync(context, () => Debug.WriteLine("Invalid cipher"));

                IDataBrokerService service = Services.GetService<IDataBrokerService>();
                Folder rootFolder = GetRootFolder(context);

                service.SetRootFolder(rootFolder);
                service.Publish(context, rootFolder.Folders, UpdateType.Initialize);
                service.Publish(context, rootFolder.Documents, UpdateType.Initialize);
                service.RaiseAppInitialized();

                // Last used version is updated here and not in HomeViewModel in case it's the first usage.
                // Otherwise, the patches (e.g. encrypting the database) would not be executed.
                compatService.UpdateLastUsedVersion();
            });

            IsBusy = false;
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
