using System.Collections.ObjectModel;
using Athena.DataModel;
using Athena.Resources.Localization;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Athena.UI
{
    public partial class TagsOverviewViewModel : ContextViewModel
    {
        [ObservableProperty]
        private ObservableCollection<TagViewModel> _tags;

        [ObservableProperty]
        private TagViewModel _selectedTag;

        public TagsOverviewViewModel()
        {
            Tags = new ObservableCollection<TagViewModel>(Tag.ReadAll(this.RetrieveContext()).Select(x => new TagViewModel(x)));
        }


        protected override void OnDataPublished(DataPublishedEventArgs e)
        {
            if (!e.Tags.Any())
                return;

            foreach (var tag in e.Tags)
            {
                switch (tag.Type)
                {
                    case UpdateType.Remove:
                        {
                            var deletedTag = this.Tags.FirstOrDefault(x => x.Id == tag.Entity.Id);

                            if (deletedTag != null)
                            {
                                Tags.Remove(deletedTag);
                            }

                            break;
                        }

                    case UpdateType.Add:
                        Tags.Add(tag.Entity);
                        break;

                }
            }
        }

        [RelayCommand]
        private async Task TagSelected(TagViewModel selectedTag)
        {
            if (SelectedTag == null)
                return;

            string choice = await DisplayActionSheet(SelectedTag.Name, Localization.Close, string.Empty, Localization.ChangeName, Localization.Delete);

            if (string.IsNullOrEmpty(choice) || choice.Equals(Localization.Close, StringComparison.OrdinalIgnoreCase))
            {
                SelectedTag = null;
                return;
            }

            var context = this.RetrieveContext();

            if (choice.Equals(Localization.Delete, StringComparison.OrdinalIgnoreCase))
            {
                bool delete = await DisplayAlert(
                    Localization.DeleteTag,
                    string.Format(Localization.DeleteTagConfirm, SelectedTag.Name),
                    Localization.Yes,
                    Localization.No);


                if (delete)
                {
                    SelectedTag.Delete(context);
                    ServiceProvider.GetService<IDataBrokerService>().Publish<Tag>(context, SelectedTag, UpdateType.Remove);

                    await Toast.Make(string.Format(Localization.TagDeleted, SelectedTag.Name), ToastDuration.Long).Show();
                }
            }
            else
            {
                string name = await DisplayPrompt(
                    Localization.EditTag,
                    string.Format(Localization.TagNewName, SelectedTag.Name),
                    Localization.Save,
                    Localization.Close);

                if (string.IsNullOrEmpty(name))
                {
                    SelectedTag = null;
                    return;
                }

                SelectedTag.Name = name;
                SelectedTag.Save(context);

                ServiceProvider.GetService<IDataBrokerService>().Publish<Tag>(context, SelectedTag, UpdateType.Edit);
            }

            SelectedTag = null;
        }


        [RelayCommand]
        internal async Task AddNewTag()
        {
            var name = await DisplayPrompt(
                Localization.AddNewTag,
                Localization.AddNewTagName,
                Localization.Add,
                Localization.Close);

            if (string.IsNullOrEmpty(name))
                return;

            var tag = new Tag
            {
                Name = name.Substring(0, Math.Min(20, name.Length))
            };

            var context = this.RetrieveContext();

            tag.Save(context);

            ServiceProvider.GetService<IDataBrokerService>().Publish<Tag>(context, tag, UpdateType.Add);
        }
    }
}
