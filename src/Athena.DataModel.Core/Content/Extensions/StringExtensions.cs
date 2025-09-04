namespace Athena.DataModel.Core
{
    public static class StringExtensions
    {
        public static string EmptyIfNull(this string value)
        {
            return value ?? string.Empty;
        }
    }
}
