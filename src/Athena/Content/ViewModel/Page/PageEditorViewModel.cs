using Athena.DataModel.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Athena.UI
{
    using Athena.DataModel;

    public partial class PageEditorViewModel : ContextViewModel
    {
        [ObservableProperty]
        private PageViewModel _page;

        [ObservableProperty]
        private bool _isNew;
        
        private Folder _folder;

        public PageEditorViewModel(Page page, Folder folder)
        {
            IsNew = page.Key == null || page.Key.Id == PageKey.TemporaryId;
            this.Page = page;
            this._folder = folder;
        }

        [RelayCommand]
        private async Task NextAddPageStep()
        {
            IContext context = this.RetrieveContext();

            var service = ServiceProvider.GetService<IDataBrokerService>();

            if (IsNew)
            {
                _folder.AddPage(Page);
                _folder.Save(context);
                
                service.Publish(context, this.Page.Page, UpdateType.Add, _folder.Key);
                service.Publish(context, _folder, UpdateType.Edit);
            }
            else
            {
                Page.Page.Save(context);
                service.Publish(context, this.Page.Page, UpdateType.Edit, _folder.Key);
            }
            
            await PopAsync();
        }
    }
}
