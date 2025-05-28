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
        private bool _isBusy;

        [ObservableProperty]
        private string _busyText;

        [ObservableProperty]
        private VisualCollection<DocumentViewModel, Document> _recentDocuments;

        public HomeViewModel()
        {
            UpdateGreeting();
            RecentDocuments = new();
        }

        private void UpdateGreeting()
        {
            var greeting = Services.GetService<IGreetingService>();
            Greeting = greeting.Get();

            string name = Services.GetService<IPreferencesService>().GetName();

            if (!string.IsNullOrEmpty(name))
            {
                Greeting += ",";
                UserName = name;
            }
        }

        protected override Task OnAppInitialized()
        {
            var context = RetrieveContext();

            UpdateCounterStats();

            RecentDocuments = new VisualCollection<DocumentViewModel, Document>(
                Document.ReadRecent(context, MAX_RECENT_DOCUMENTS).Select(x => new DocumentViewModel(x)));

            Application.Current.Dispatcher.StartTimer(TimeSpan.FromMinutes(5), UpdateCounterStats);

            return Task.CompletedTask;
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
                    foreach (var update in e.Documents)
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
                            var rootFolder = Services.GetService<IDataBrokerService>().GetRootFolder();
                            Stack<Folder> folders = new Stack<Folder>();

                            bool stop = false;

                            foreach (var folder in rootFolder.LoadedFolders)
                            {
                                folders.Push(folder);
                            }

                            folders.Push(rootFolder);

                            while (folders.Count > 0)
                            {
                                Folder currentFolder = folders.Pop();

                                foreach (var folderDoc in currentFolder.LoadedDocuments)
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
                    var deletedTagIds = e.Tags
                        .Where(x => x.Type == UpdateType.Delete)
                        .Select(x => x.Entity.Id)
                        .ToHashSet();

                    foreach (var document in RecentDocuments)
                    {
                        var validTags = document.Tags.Where(x => !deletedTagIds.Contains(x.Id)).ToList();

                        document.Tags.Clear();

                        foreach (var tag in validTags)
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
            var rootFolder = Services.GetService<IDataBrokerService>().GetRootFolder();
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
            IsBusy = true;
            IContext context = RetrieveReportContext();
            
            await Task.Run(async () =>
            {
                await Task.Delay(200);
                SqLiteProxyParameter parameter = new SqLiteProxyParameter { MinimumVersion = new Version(0, 1) };

                Services.GetService<IDataBrokerService>().PrepareForLoading();

                DataStore.Clear();
                DataStore.Register(SqLiteProxy.Request<IFolderRepository>(parameter));
                DataStore.Register(SqLiteProxy.Request<IDocumentRepository>(parameter));
                DataStore.Register(SqLiteProxy.Request<IChapterRepository>(parameter));
                DataStore.Register(SqLiteProxy.Request<ITagRepository>(parameter));
                await DataStore.InitializeAsync(context);

                // Data will be initialized via the welcome view.
                if (Services.GetService<IPreferencesService>().IsFirstUsage())
                    return;

                Services.GetService<ICompatibilityService>().UpdateLastUsedVersion();

                IDataBrokerService service = Services.GetService<IDataBrokerService>();

                Folder rootFolder = GetRootFolder(context);

                service.SetRootFolder(rootFolder);
                service.Publish(context, rootFolder.Folders, UpdateType.Initialize);
                service.Publish(context, rootFolder.Documents, UpdateType.Initialize);
                service.RaiseAppInitialized();
            });

            IsBusy = false;
        }

        private static Folder GetRootFolder(IContext context)
        {
            var rootFolder = Folder.Read(context, IntegerEntityKey.Root);

            if (rootFolder != null)
                return rootFolder;

            Folder.CreateRoot(context);
            return Folder.Read(context, IntegerEntityKey.Root);
        }


    }
}
