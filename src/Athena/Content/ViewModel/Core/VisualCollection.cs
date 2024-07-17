using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Athena.DataModel.Core;

namespace Athena.UI
{
    public class VisualCollection<TViewModel, TEntity> : ObservableCollection<TViewModel>
        where TViewModel : class, IVisualModel<TEntity>
        where TEntity : Entity
    {
        public VisualCollection()
            : base()
        {
        }

        public VisualCollection(IEnumerable<TViewModel> items)
            : base(items)
        {
        }

        public VisualCollection(List<TViewModel> items)
            : base(items)
        {
        }

        public void AddRange(IEnumerable<TViewModel> items)
        {
            foreach (var item in items)
            {
                Items.Add(item);
            }

            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public void Edit(TEntity entity)
        {
            var toEdit = Find(entity.Id);
            toEdit?.Edit(entity);
        }

        public void Delete(TEntity entity)
        {
            var toDelete = Find(entity.Id);

            if (toDelete != null)
            {
                Remove(toDelete);
            }
        }

        private TViewModel Find(int id)
        {
            return Items.FirstOrDefault(x => x.Id == id);
        }

        public void Process(RequestUpdate<TEntity> update)
        {
            switch (update.Type)
            {
                case UpdateType.Add:
                    Add(Activator.CreateInstance(typeof(TViewModel), update.Entity) as TViewModel);
                    break;

                case UpdateType.Delete:
                    Delete(update.Entity);
                    break;

                case UpdateType.Edit:
                    Edit(update.Entity);
                    break;
            }
        }

    }

}
