namespace NSUWatcher.Interfaces.ExtCommands
{
    public interface IConsoleCommands
    {
        public IExternalCommand Start();
        public IExternalCommand Stop();
        public IExternalCommand ExecCommandLine(string cmdLine);
    }
}
