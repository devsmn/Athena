
using Athena.Data.SQLite.Provider;
using Athena.Data.SQLite.Proxy;
using Athena.DataModel;

namespace Athena.UI
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            Task.Run(async () => {
                SQLiteProxyParameter parameter = new SQLiteProxyParameter {
                    MinimumVersion = new Version(0, 1)
                };

                DataStore.Register(SQLiteProxy.Request<IFolderRepository>(parameter));
                DataStore.Register(SQLiteProxy.Request<IDocumentRepository>(parameter));
                DataStore.Register(SQLiteProxy.Request<IPageRepository>(parameter));

                await DataStore.InitializeAsync();
            }).ContinueWith(
                _ => {
                    var context = new AthenaAppContext();
                    var folders = Folder.ReadAll(new AthenaAppContext());

                    ServiceProvider.GetService<IDataBrokerService>().Publish(context, folders, UpdateType.Initialize);
                });
                  
            MainPage = new NavigationPage(new FolderOverview());
            
        }
    }
}