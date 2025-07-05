using Athena.DataModel.Core;

namespace Athena.UI
{
    public interface ILanguageService
    {
        /// <summary>
        /// Sets the language to the given <paramref name="code"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="code"></param>
        /// <param name="reload"></param>
        void SetLanguage(IContext context, string code, bool reload);

        /// <summary>
        /// Gets all supported languages.
        /// </summary>
        /// <returns></returns>
        IEnumerable<LanguageViewModel> GetSupportedLanguages();

        /// <summary>
        /// Gets the language saved in the preferences.
        /// </summary>
        /// <returns></returns>
        string GetLanguage();
    }
}
