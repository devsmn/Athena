using Athena.DataModel.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena.DataModel
{
    public partial class Folder
    {
        public void AddPage(Page page)
        {
            this.Pages.Add(page);
        }
        
        public IEnumerable<Page> ReadAllPages(IContext context)
        {
            var pages = DataStore.Resolve<IFolderRepository>().ReadAllPages(context, this);

            foreach (var page in pages)
            {
                page.ReadAllDocuments(context);
                this.Pages.Add(page);
            }

            return this.Pages;

        }

        public static IEnumerable<Folder> ReadAll(IContext context)
        {
            return DataStore.Resolve<IFolderRepository>().ReadAll(context);
        }
        
        public void Save(IContext context)
        {
            DataStore.Resolve<IFolderRepository>().Save(context, this);
        }
        
    }
}
