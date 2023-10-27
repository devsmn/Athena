using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Athena.DataModel;
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
        private int _addPageStep;

        private Folder _folder;

        public PageEditorViewModel(Page page, Folder folder)
        {
            this.Page = page;
            this._folder = folder;
        }

        [RelayCommand]
        private async Task NextAddPageStep()
        {
            AddPageStep++;

            if (AddPageStep > 1)
            {
                IContext context = this.RetrieveContext();

                _folder.AddPage(Page);
                _folder.Save(context);
                
                var service = ServiceProvider.GetService<IDataBrokerService>();

                service.Publish(context, this.Page.Page, UpdateType.Add, _folder.Key);
                service.Publish(context, _folder, UpdateType.Edit);

                await App.Current.MainPage.Navigation.PopAsync();
            }
        }
    }
}
