namespace NSUWatcher.Interfaces
{
    public interface IExternalCommandResult
    {
        string Target { get; }
        string Action { get; }
        string Result { get; }
        string ErrorMessage { get; }
        string Content { get; }
    }
}
