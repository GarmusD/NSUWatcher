namespace NSUWatcher.Interfaces
{
    public interface IMessageData
    {
        DataDestination Destination { get; set; }
        string Action { get; set; }
        string Content { get; set; }
    }

    public enum DataDestination
    {
        Unknown,
        Mcu,
        System
    }
}
