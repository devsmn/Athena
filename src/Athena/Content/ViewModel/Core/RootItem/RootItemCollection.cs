using Athena.DataModel;

namespace Athena.UI
{
    public class RootItemCollection : VisualCollection<RootItemViewModel, RootItem>
    {
        public void Process(RequestUpdate<Document> update)
        {
            switch (update.Type)
            {
                case UpdateType.Delete:
                    Delete(update.Entity);
                    break;

                case UpdateType.Add:
                    Add(update.Entity);
                    break;

                case UpdateType.Edit:
                    Edit(update.Entity);
                    break;
            }

            update.Handled = true;
        }

        public void Process(RequestUpdate<Folder> update)
        {
            switch (update.Type)
            {
                case UpdateType.Delete:
                    Delete(update.Entity);
                    break;

                case UpdateType.Add:
                    Add(update.Entity);
                    break;

                case UpdateType.Edit:
                    Edit(update.Entity);
                    break;
            }
        }

        public void Add(Folder folder)
        {
            Add(new RootItemViewModel(folder));
        }

        public void Add(Document document)
        {
            Add(new RootItemViewModel(document));
        }

        public void Delete(Folder folder)
        {
            var toDelete = Items.FirstOrDefault(x => x.IsFolder && x.Id == folder.Id);

            if (toDelete != null)
            {
                Remove(toDelete);
            }
        }

        public void Delete(Document document)
        {
            var toDelete = Items.FirstOrDefault(x => !x.IsFolder && x.Id == document.Id);

            if (toDelete != null)
            {
                Remove(toDelete);
            }
        }

        public void Edit(Folder folder)
        {
            var toEdit = Items.FirstOrDefault(x => x.IsFolder && x.Id == folder.Id);

            if (toEdit == null)
                return;

            toEdit.Name = folder.Name;
            toEdit.Comment = folder.Comment;
            toEdit.IsPinned = folder.IsPinned;
        }

        public void Edit(Document document)
        {
            var toEdit = Items.FirstOrDefault(x => !x.IsFolder && x.Id == document.Id);

            if (toEdit == null)
                return;

            toEdit.Name = document.Name;
            toEdit.Comment = document.Comment;
            toEdit.Document.Tags.Clear();

            foreach (var tag in document.Tags)
            {
                toEdit.Document.Tags.Add(tag);
            }
        }
    }
}
