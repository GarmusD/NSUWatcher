using NSUWatcher.Interfaces;

namespace NSUWatcher.CommandCenter
{
    public class ExternalCommand : IExternalCommand
    {
        public string Target { get; set; } = string.Empty;
        public string Action { get; set;} = string.Empty;
        public string Content { get; set;} = string.Empty;
    }
}
