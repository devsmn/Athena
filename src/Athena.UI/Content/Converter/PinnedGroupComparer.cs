using Syncfusion.Maui.DataSource.Extensions;

namespace Athena.UI
{
    public class PinnedGroupComparer : IComparer<GroupResult>
    {
        public int Compare(GroupResult x, GroupResult y)
        {
            bool firstKey = Convert.ToBoolean(x.Key);
            bool secondKey = Convert.ToBoolean(y.Key);

            if (firstKey == secondKey)
                return 0;

            if (firstKey)
                return -1;

            return 1;
        }
    }
}
