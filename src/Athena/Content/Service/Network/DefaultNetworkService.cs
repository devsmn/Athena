namespace Athena.UI
{
    internal class DefaultNetworkService : INetworkService
    {
        public bool IsWifi()
        {
            return Connectivity.Current.ConnectionProfiles.Contains(ConnectionProfile.WiFi);
        }

        public bool IsInternetAccessPossible()
        {
            return Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
        }
    }
}
