namespace NSUWatcher.Interfaces
{
    public interface IExternalCommand
    {
        string Target { get; }
        string Action { get; }
        string Content { get; }
    }
}
