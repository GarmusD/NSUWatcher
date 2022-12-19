using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NSU.Shared;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NSUWatcher.Transport.TestTransport.Commands
{
    internal partial class SysCommands : TestCommandsBase
    {
        public SysCommands(CommandSender commandSender, ILoggerFactory loggerFactory) : base(commandSender, loggerFactory.CreateLoggerShort<SysCommands>())
        {
        }

        public override bool ExecCommand(JObject command)
        {
            if ((string)command[JKeys.Generic.Target] != JKeys.Syscmd.TargetName)
                return false;
            string action = (string)command[JKeys.Generic.Action];
            string response = action switch 
            {
                JKeys.Syscmd.SystemStatus => "{\"target\":\"system\",\"action\":\"systemstatus\",\"value\":\"running\",\"freemem\":42224,\"uptime\":0,\"rebootreq\":false}",
                JKeys.Syscmd.SetTime => "{\"target\":\"system\",\"action\":\"settime\",\"result\":\"ok\"}",
                JKeys.Syscmd.Snapshot => SendSnapshot(),
                _ => string.Empty
            };
            if(!string.IsNullOrEmpty(response))
                _sender.SendNow(response);
            return true;
        }

        public override Task RunAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
        
        private string SendSnapshot()
        {
            _sender.BeginSnapshot();
            foreach (var item in SysSnapshotLines.SnapshotLines)
            {
                _sender.Send(item);
            }
            _sender.EndSnapshot();
            return string.Empty;
        }
    }
}
