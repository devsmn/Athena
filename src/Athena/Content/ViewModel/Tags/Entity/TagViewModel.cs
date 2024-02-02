using System.ComponentModel;
using System.Runtime.CompilerServices;
using Athena.DataModel;
using Athena.DataModel.Core;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Athena.UI
{
    public class TagViewModel : ObservableObject
    {
        private readonly Tag _tag;

        public Tag Tag
        {
            get { return _tag; }
        }

        public int Id
        {
            get { return _tag.Id; }
        }

        public string Name
        {
            get { return _tag.Name; }
            set
            {
                _tag.Name = value;
                OnPropertyChanged();
            }
        }

        public TagViewModel(Tag tag)
        {
            _tag = tag;
        }

        public void Delete(IContext context)
        {
            _tag.Delete(context);
        }

        public void Save(IContext context)
        {
            _tag.Save(context);
        }

        public static implicit operator TagViewModel(Tag tag)
        {
            return new TagViewModel(tag);
        }

        public static implicit operator Tag(TagViewModel tagVm)
        {
            return tagVm._tag;
        }
        
        

    }
}
