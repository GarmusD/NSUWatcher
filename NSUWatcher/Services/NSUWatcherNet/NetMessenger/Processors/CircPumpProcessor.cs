using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json.Linq;
using NSU.Shared;
using NSU.Shared.NSUNet;
using NSUWatcher.Interfaces;

namespace NSUWatcher.Services.NSUWatcherNet.NetMessenger.Processors
{
#nullable enable
    public class CircPumpProcessor : IMsgProcessor
    {
        private readonly ICmdCenter _cmdCenter;
        private readonly ILogger _logger;

        public CircPumpProcessor(ICmdCenter cmdCenter, ILoggerFactory loggerFactory)
        {
            _cmdCenter = cmdCenter;
            _logger = loggerFactory?.CreateLoggerShort<CircPumpProcessor>() ?? NullLoggerFactory.Instance.CreateLoggerShort<CircPumpProcessor>();
        }

        public bool ProcessMessage(JObject message, NetClientData clientData, out INetMessage? response)
        {
            string target = (string)message[JKeys.Generic.Target]!;
            string action = (string)message[JKeys.Generic.Action]!;

            if (target == JKeys.CircPump.TargetName)
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

        private INetMessage? ProcessActionClick(JObject message, NetClientData clientData)
        {
            string? name = (string?)message[JKeys.Generic.Name];
            if(!string.IsNullOrEmpty(name))
            {
                var command = _cmdCenter.ExtCommandFactory.CircPumpCommands.Clicked(name);
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
