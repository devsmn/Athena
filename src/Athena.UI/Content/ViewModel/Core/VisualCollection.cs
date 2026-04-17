using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Athena.DataModel.Core;

namespace Athena.UI
{
    /// <summary>
    /// Provides a common base for binding operations for logically similar entities (e.g. folders and documents).
    /// </summary>
    /// <typeparam name="TViewModel">The viewmodel of the entity.</typeparam>
    /// <typeparam name="TEntity">The entity.</typeparam>
    public class VisualCollection<TViewModel, TEntity> : ObservableCollection<TViewModel>
        where TViewModel : class, IVisualModel
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

        /// <summary>
        /// Adds the given range of <paramref name="items"/> to this collection.
        /// The notifications are suspended until all items were added.
        /// </summary>
        /// <param name="items"></param>
        public void AddRange(IEnumerable<TViewModel> items)
        {
            foreach (TViewModel item in items)
            {
                Items.Add(item);
            }

            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
