using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Athena.UI
{
    internal class DataPublishedMessage : ValueChangedMessage<DataPublishedArgs>
    {
        public DataPublishedMessage(DataPublishedArgs data)
            : base(data)
        {
        }
    }
}
