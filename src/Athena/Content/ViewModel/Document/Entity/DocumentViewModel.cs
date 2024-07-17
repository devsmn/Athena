using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using Athena.DataModel.Core;
using Athena.Resources.Localization;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Graphics.Platform;

namespace Athena.UI
{
    using Athena.DataModel;

    public class DocumentViewModel : ObservableObject, IVisualModel<Document>
    {
        private readonly Document _document;
        private string _imageLocation;

        public DocumentViewModel()
        {
            Tags = new();
        }

        public DocumentViewModel(Document document)
        {
            _document = document;
            Tags = new ObservableCollection<Tag>(document.Tags);
        }


        [Display(AutoGenerateField = false)]
        public IntegerEntityKey Key
        {
            get { return _document.Key; }
        }

        [Display(AutoGenerateField = false)]
        public bool IsPinned
        {
            get { return _document.IsPinned; }
            set
            {
                _document.IsPinned = value;
                OnPropertyChanged();
            }
        }

        [Display(AutoGenerateField = false)]
        public string ImageLocation
        {
            get { return _imageLocation; }
            set { _imageLocation = value; }
        }

        [Display(AutoGenerateField = false)]
        public Document Document
        {
            get { return _document; }
        }

        [Display(AutoGenerateField = false)]
        public int Id
        {
            get { return _document.Key.Id; }
        }

        public void Edit(Document entity)
        {
            Name = entity.Name;
            Comment = entity.Comment;
            IsPinned = entity.IsPinned;

            Tags.Clear();
            foreach (var tag in entity.Tags)
            {
                Tags.Add(tag);
            }
        }

        [Required(
            AllowEmptyStrings = false,
            ErrorMessageResourceType = typeof(Localization),
            ErrorMessageResourceName = nameof(Localization.DocumentVMNameRequired))]
        [StringLength(
            45,
            ErrorMessageResourceType = typeof(Localization),
            ErrorMessageResourceName = nameof(Localization.DocumentVMNameCharsExceedLimit))]
        [Display(
            ResourceType = typeof(Localization),
            Name = nameof(Localization.DocumentVMName))]
        [DataType(DataType.Text)]
        public string Name
        {
            get { return _document.Name; }
            set
            {
                _document.Name = value;
                OnPropertyChanged();
            }
        }

        [Display(
            ResourceType = typeof(Localization),
            Name = nameof(Localization.DocumentVMComment))]
        [StringLength(
            80,
            ErrorMessageResourceType = typeof(Localization),
            ErrorMessageResourceName = nameof(Localization.DocumentVMCommentExceedsCharLimit))]
        [DataType(DataType.MultilineText)]
        public string Comment
        {
            get { return _document.Comment; }
            set
            {
                _document.Comment = value;
                OnPropertyChanged();
            }
        }

        [Display(AutoGenerateField = false)]
        public ObservableCollection<Tag> Tags
        {
            get;
            set;
        }

        public void AddTag(IContext context, TagViewModel tag)
        {
            Document.AddTag(context, tag);
            _document.Tags.Add(tag);
            Tags.Add(tag);
        }

        public void DeleteTag(IContext context, TagViewModel tag)
        {
            Document.DeleteTag(context, tag);
            _document.Tags.Remove(tag);
            Tags.Remove(tag);
        }

        [Display(AutoGenerateField = false)]
        public DateTime ModDate
        {
            get { return _document.ModDate; }
        }

        [Display(AutoGenerateField = false)]
        public DateTime CreationDate
        {
            get { return _document.CreationDate; }
        }

        [Display(AutoGenerateField = false)]
        public byte[] Pdf
        {
            get { return _document.Pdf; }
            set
            {
                _document.Pdf = value;
                OnPropertyChanged();
            }
        }

        [Display(AutoGenerateField = false)]
        public byte[] Thumbnail
        {
            get { return _document.Thumbnail; }
            set
            {
                using (MemoryStream ms = new MemoryStream(value))
                {
                    var img = PlatformImage.FromStream(ms);
                    var newImage = img.Resize(512, 512);
                    _document.Thumbnail = newImage.AsBytes();
                }

                OnPropertyChanged();
            }
        }

        public static implicit operator DocumentViewModel(Document document)
        {
            return new DocumentViewModel(document);
        }

        public static implicit operator Document(DocumentViewModel document)
        {
            return document._document;
        }

    }
}
