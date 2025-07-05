namespace Athena.UI
{
    public interface INetworkService
    {
        /// <summary>
        /// Determines whether the device is connected to a WLAN network.
        /// </summary>
        /// <returns></returns>
        bool IsWifi();

        /// <summary>
        /// Determines whether internet access is possible.
        /// </summary>
        /// <returns></returns>
        bool IsInternetAccessPossible();
    }
}
