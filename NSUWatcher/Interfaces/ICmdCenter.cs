using Newtonsoft.Json.Linq;
using System;
using NSUWatcher.Interfaces.MCUCommands;
using System.Threading.Tasks;
using System.Threading;

namespace NSUWatcher.Interfaces
{
    public interface ICmdCenter
    {
        event EventHandler<McuMessageReceivedEventArgs> McuMessageReceived;
        event EventHandler<SystemMessageEventArgs> SystemMessageReceived;
        event EventHandler<ExternalCommandEventArgs> ExternalCommandReceived;
        event EventHandler<ManualCommandEventArgs> ManualCommandReceived;

        IMcuMessageTransport MessageTransport { get; set; }
        IMcuCommands MCUCommands { get; }
        IExternalCommands ExternalCommands { get; }

        //Task StartAsync(CancellationToken cancellationToken);
        //Task StopAsync(CancellationToken cancellationToken);
        //public void ExecExternalCommand(IExternalCommand command, INsuUser nsuUser, Action<IExternalCommandResult, object> onCommandResult, object context);
        IExternalCommandResult ExecExternalCommand(IExternalCommand command, INsuUser nsuUser, object context);
        void ExecManualCommand(string command);
    }
    
    public enum SysMessage
    { 
        TransportConnected,
        TransportConnectFailed,
        TransportDisconnected,
        McuCrashed,
    }

    public class SystemMessageEventArgs : EventArgs
    {
        public SysMessage Message { get; }

        public SystemMessageEventArgs(SysMessage message)
        {
            Message = message;
        }
    }

    public class McuMessageReceivedEventArgs : EventArgs
    {
        public IMessageFromMcu Message { get; private set; }

        public McuMessageReceivedEventArgs(IMessageFromMcu message)
        {
            Message = message;
        }
    }
    /*public class ExternalCommandEventArgs : EventArgs
    {
        public IExternalCommand Command { get; }
        public INsuUser NsuUser { get; }
        public Action<IExternalCommandResult, object> OnCommandResult { get; }
        public object Context { get; }
        public ExternalCommandEventArgs(IExternalCommand command, INsuUser nsuUser, Action<IExternalCommandResult, object> onCommandResult, object context)
        {
            Command = command;
            NsuUser = nsuUser;
            OnCommandResult = onCommandResult;
            Context = context;
        }
    }*/

    public class ExternalCommandEventArgs : EventArgs
    {
        public IExternalCommand Command { get; }
        public INsuUser NsuUser { get; }
        public IExternalCommandResult Result { get; set; } = null;
        public object Context { get; set; }
        public ExternalCommandEventArgs(IExternalCommand command, INsuUser nsuUser, object context = null)
        {
            Command = command;
            NsuUser = nsuUser;
            Context = context;
        }
    }

    public class ManualCommandEventArgs : EventArgs
    {
        public string Command { get; }

        public ManualCommandEventArgs(string command)
        {
            Command = command;
        }
    }
}
