using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena.UI
{
    public static class UiExtensions
    {
        public static TParent GetParent<TParent>(this Element? element)
        {
            Element? parent = element?.Parent;

            while (parent != null)
            {
                if (parent is TParent parentType)
                    return parentType;

                parent = parent.Parent;
            }

            return default;
        }
    }
}
