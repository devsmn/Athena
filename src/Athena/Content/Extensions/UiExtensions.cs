using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena.UI
{
    public static class UiExtensions
    {
        /// <summary>
        /// Gets the first <typeparam name="TParent">parent</typeparam> for this <paramref name="element"/>.
        /// </summary>
        /// <typeparam name="TParent"></typeparam>
        /// <param name="element"></param>
        /// <returns></returns>
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
