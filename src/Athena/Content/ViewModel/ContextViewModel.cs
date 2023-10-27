using Athena.DataModel.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena.UI
{
    public class ContextViewModel : ObservableObject
    {
        public ContextViewModel()
            : base()
        {
        }

        [Obsolete("Not implemented", true)]
        protected TContext RetrieveContext<TContext>()
            where TContext : IContext
        {
            return default;
        }

        protected IContext RetrieveContext()
        {
            return new AthenaAppContext();
        }
        
    }
}
