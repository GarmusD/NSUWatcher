using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json.Linq;
using NSU.Shared;
using NSU.Shared.NSUNet;
using NSUWatcher.Interfaces;

namespace NSUWatcher.Services.NSUWatcherNet.NetMessenger.Processors
{
#nullable enable
    public class SwitchProcessor : IMsgProcessor
    {
        private readonly ICmdCenter _cmdCenter;
        private readonly ILogger _logger;

        public SwitchProcessor(ICmdCenter cmdCenter, ILoggerFactory loggerFactory)
        {
            _cmdCenter = cmdCenter;
            _logger = loggerFactory?.CreateLoggerShort<SwitchProcessor>() ?? NullLoggerFactory.Instance.CreateLoggerShort<SwitchProcessor>();
        }

        public bool ProcessMessage(JObject message, NetClientData clientData, out INetMessage? response)
        {
            string target = (string)message[JKeys.Generic.Target]!;
            string action = (string)message[JKeys.Generic.Action]!;

            if (target == JKeys.Switch.TargetName)
            {
                switch (action)
                {
                    case JKeys.Action.Click:
                        response = ProcessActionClick(message, clientData);
                        return true;
                }
            }
            response = null;
            return false;
        }

        public INetMessage? ProcessActionClick(JObject message, NetClientData clientData)
        {
            string? name = (string?)message[JKeys.Generic.Name];
            if (!string.IsNullOrEmpty(name))
            {
                var command = _cmdCenter.ExtCommandFactory.SwitchCommands.Click(name);
                var result = _cmdCenter.ExecExternalCommand(command, clientData.NsuUser, clientData.ClientID);
                if (result != null)
                    return new NetMessage(result);
            }
            else
            {
                _logger.LogWarning($"ProcessActionClick: An required parameter 'name' not exists. Message: {message}");
            }
            // No response to this command
            return null;
        }
    }
}
