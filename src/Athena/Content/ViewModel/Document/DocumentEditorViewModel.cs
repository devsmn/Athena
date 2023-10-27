using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Athena.UI
{
    using Athena.DataModel;
    using CommunityToolkit.Mvvm.Input;

    public partial class DocumentEditorViewModel : ContextViewModel
    {
        [ObservableProperty]
        private int _documentStep;

        [ObservableProperty]
        private bool _isNew;

        private DocumentViewModel _document;

        private Page _page;
        private readonly Folder _folder;

        public DocumentViewModel Document
        {
            get { return _document; }

            set
            {
                _document = value;
                OnPropertyChanged();
            }
        }

        public DocumentEditorViewModel(Folder folder, Page page, Document document)
        {
            this.IsNew = document == null;

            _folder = folder;
            _page = page;
            Document = document;
        }

        [RelayCommand]
        private async Task NextStep()
        {
            DocumentStep++;

            if (DocumentStep > 2)
            {
                var context = this.RetrieveContext();
                
                DocumentStep = 0;
                
                _page.AddDocument(Document);
                _page.Save(context);

                ServiceProvider.GetService<IDataBrokerService>().Publish(context, Document.Document, UpdateType.Add, _page.Key);
                ServiceProvider.GetService<IDataBrokerService>().Publish(context, _page, UpdateType.Edit, _folder.Key);

                await App.Current.MainPage.Navigation.PopAsync();
            }
        }


        [RelayCommand]
        private async Task TakeImage()
        {
            if (MediaPicker.Default.IsCaptureSupported)
            {
                var context = this.RetrieveContext();

                context.Log("Taking picture");

                FileResult image = await MediaPicker.Default.CapturePhotoAsync();

                if (image != null)
                {
                    using Stream sourceStream = await image.OpenReadAsync();

                    using MemoryStream byteStream = new();
                    await sourceStream.CopyToAsync(byteStream);
                    byte[] bytes = byteStream.ToArray();

                    string content = Convert.ToBase64String(bytes);

                    Document.Image = bytes;
                }
            }
        }

        [RelayCommand]
        private async Task PickImage()
        {
            var image = await MediaPicker.PickPhotoAsync();

            if (image != null)
            {
                using Stream sourceStream = await image.OpenReadAsync();

                using MemoryStream byteStream = new();
                await sourceStream.CopyToAsync(byteStream);
                byte[] bytes = byteStream.ToArray();

                string content = Convert.ToBase64String(bytes);

                Document.Image = bytes;
            }
        }

    }
}
