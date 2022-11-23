using NSU.Shared;
using NSU.Shared.DTO.ExtCommandContent;
using NSU.Shared.Serializer;
using NSUWatcher.Interfaces;
using NSUWatcher.Interfaces.ExtCommands;

namespace NSUWatcher.CommandCenter.ExtCommands
{
    public class ConsoleCommands : IConsoleCommands
    {
        private readonly INsuSerializer _serializer;

        public ConsoleCommands(INsuSerializer serializer)
        {
            _serializer = serializer;
        }

        public IExternalCommand Start()
        {
            return new ExternalCommand()
            {
                Target = JKeys.Console.TargetName,
                Action = JKeys.Console.Start
            };
        }

        public IExternalCommand Stop()
        {
            return new ExternalCommand()
            {
                Target = JKeys.Console.TargetName,
                Action = JKeys.Console.Stop
            };
        }
        
        public IExternalCommand ExecCommandLine(string cmdLine)
        {
            return new ExternalCommand()
            {
                Target = JKeys.Console.TargetName,
                Action = JKeys.Console.ManualCommand,
                Content = _serializer.Serialize( new ConsoleExecCmdLineContent(cmdLine) )
            };
        }
    }
}
