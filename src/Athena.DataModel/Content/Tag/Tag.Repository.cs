using Athena.DataModel.Core;

namespace Athena.DataModel
{
    public partial class Tag
    {
        public static IEnumerable<Tag> ReadAll(IContext context)
        {
            return DataStore.Resolve<ITagRepository>().ReadAll(context);
        }

        public void Save(IContext context)
        {
            DataStore.Resolve<ITagRepository>().Save(context, this);
        }

        public void Delete(IContext context)
        {
            DataStore.Resolve<ITagRepository>().Delete(context, this);
        }
    }
}
