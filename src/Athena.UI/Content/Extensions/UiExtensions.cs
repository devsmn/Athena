namespace Athena.UI
{
    public static class UiExtensions
    {
        /// <summary>
        /// Gets the first <typeparam name="TParent">parent</typeparam> for this <paramref name="element"/>.
        /// </summary>
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
