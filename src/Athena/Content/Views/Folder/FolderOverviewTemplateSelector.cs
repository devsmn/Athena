using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena.UI
{
    public class FolderOverviewTemplateSelector : DataTemplateSelector
    {
        public DataTemplate FolderTemplate { get; set; }
        public DataTemplate DocumentTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item is not RootItemViewModel viewItem)
                return null;

            return viewItem.IsFolder ? FolderTemplate : DocumentTemplate;
        }
    }
}
