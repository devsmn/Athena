using Athena.Resources.Localization;

namespace Athena.UI
{
    public class DefaultGreetingService : IGreetingService
    {
        public string Get()
        {
            int hour = DateTime.Now.Hour;

            return hour switch
            {
                >= 6 and < 12 => Localization.GoodMorning,
                >= 12 and < 18 => Localization.GoodAfternoon,
                >= 18 and < 23 => Localization.GoodEvening,
                _ => Localization.GoodNight,
            };
        }
    }
}
