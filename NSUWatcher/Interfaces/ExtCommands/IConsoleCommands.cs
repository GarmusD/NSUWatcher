namespace NSUWatcher.Interfaces.ExtCommands
{
    public interface IConsoleCommands
    {
        IExternalCommand Start();
        IExternalCommand Stop();
        IExternalCommand ExecCommandLine(string cmdLine);
    }
}
