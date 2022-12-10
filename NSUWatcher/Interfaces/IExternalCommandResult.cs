namespace NSUWatcher.Interfaces
{
    public interface IExternalCommandResult
    {
        public string Target { get; }
        public string Action { get; }
        public string Result { get; }
        public string ErrorMessage { get; }
        public string Content { get; }
    }
}
