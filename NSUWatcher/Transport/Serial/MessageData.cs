using NSUWatcher.Interfaces;

namespace NSUWatcher.Transport.Serial
{
    public class MessageData : IMessageData
    {
        public DataDestination Destination { get; set; } = DataDestination.Unknown;
        public string Action { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}
