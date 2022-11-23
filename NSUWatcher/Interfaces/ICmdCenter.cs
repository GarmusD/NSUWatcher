using Newtonsoft.Json.Linq;
using System;
using NSUWatcher.Interfaces.MCUCommands;
using System.Threading.Tasks;
using System.Threading;

namespace NSUWatcher.Interfaces
{
    public interface ICmdCenter
    {
        public event EventHandler<McuMessageReceivedEventArgs>? McuMessageReceived;
        public event EventHandler<SystemMessageEventArgs>? SystemMessageReceived;
        public event EventHandler<ExternalCommandEventArgs>? ExternalCommandReceived;
        public event EventHandler<ManualCommandEventArgs>? ManualCommandReceived;

        public IMessageTransport MessageTransport { get; set; }
        public IMcuCommands MCUCommands { get; }
        public IExternalCommands ExternalCommands { get; }

        public Task StartAsync(CancellationToken cancellationToken);
        public Task StopAsync(CancellationToken cancellationToken);
        //public void ExecExternalCommand(IExternalCommand command, INsuUser nsuUser, Action<IExternalCommandResult, object> onCommandResult, object context);
        public IExternalCommandResult? ExecExternalCommand(IExternalCommand command, INsuUser nsuUser, object? context);
        public void ExecManualCommand(string command);
    }
    
    public enum SysMessage
    { 
        TransportConnected,
        TransportConnectFailed,
        TransportDisconnected,
        McuCrashed,
        TransportAppCrashed
    }

    public class McuMessageReceivedEventArgs
    {
        public IMessageFromMcu Message { get; }

        public McuMessageReceivedEventArgs(IMessageFromMcu command)
        {
            Message = command ?? throw new ArgumentNullException(nameof(command), "Message from MCU cannot be null.");
        }
    }

    public class SystemMessageEventArgs : EventArgs
    {
        public SysMessage Message { get; }

        public SystemMessageEventArgs(SysMessage message)
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
        public IExternalCommandResult? Result { get; set; } = null;
        public object? Context { get; set; }
        public ExternalCommandEventArgs(IExternalCommand command, INsuUser nsuUser, object? context = null)
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
