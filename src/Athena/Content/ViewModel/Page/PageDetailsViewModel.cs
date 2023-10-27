using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Athena.UI;

namespace Athena.UI
{
    using Athena.DataModel;
    using CommunityToolkit.Mvvm.Input;

    public partial class PageDetailsViewModel : ContextViewModel
    {
        private PageViewModel _page;
        private readonly Folder _folder;

        public PageViewModel Page
        {
            get { return _page; }
            set
            {
                _page = value;
                OnPropertyChanged();
            }
        }

        public PageDetailsViewModel(Folder folder, Page page)
        {
            this.Page = page;
            this._folder = folder;

            ServiceProvider.GetService<IDataBrokerService>().Published += OnDataBrokerPublished;
        }

        private void OnDataBrokerPublished(object sender, DataPublishedEventArgs e)
        {
            var pageUpdate = e.Pages.FirstOrDefault(x => x.Entity == this.Page.Page);

            if (pageUpdate != null)
            {
                if (pageUpdate.Type == UpdateType.Edit)
                {
                    Page.Title = pageUpdate.Entity.Title;
                    Page.Comment = pageUpdate.Entity.Comment;
                }
            }

            var documentUpdate = e.Documents.Where(x => x.ParentReference == this.Page.Page.Key);

            foreach (var docUpdate in documentUpdate)
            {
                if (docUpdate.Type == UpdateType.Add)
                {
                    Page.AddDocument(docUpdate);
                }
                else if (docUpdate.Type == UpdateType.Remove)
                {

                }
            }
        }

        [RelayCommand]
        private async Task AddDocument()
        {
            await App.Current.MainPage.Navigation.PushAsync(new DocumentEditorView(this._folder, _page, new Document()));
        }

        [RelayCommand]
        private async Task DeleteDocument(DocumentViewModel document)
        {
            bool result = await App.Current.MainPage.DisplayAlert(
                "Confirm deletion",
                $"Do you really want to delete the document{Environment.NewLine}{document.Name}?" +
                $"{Environment.NewLine}" +
                $"{Environment.NewLine}" +
                $"This action cannot be undone!",
                "Yes",
                "No");

            if (!result)
                return;

            var context = this.RetrieveContext();

            document.Document.Delete(context);
            Page.RemoveDocument(document);

            ServiceProvider.GetService<IDataBrokerService>().Publish(context, document.Document, UpdateType.Remove, Page.Page.Key);
            ServiceProvider.GetService<IDataBrokerService>().Publish(context, Page.Page, UpdateType.Edit, _folder.Key);
        }

        [RelayCommand]
        private async Task DocumentSelected(DocumentViewModel document)
        {
            await App.Current.MainPage.Navigation.PushAsync(new DocumentDetailsView(_page, document));
        }
    }
}
