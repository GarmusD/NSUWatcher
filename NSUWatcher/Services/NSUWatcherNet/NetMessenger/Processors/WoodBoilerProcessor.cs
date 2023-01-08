using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json.Linq;
using NSU.Shared;
using NSU.Shared.NSUNet;
using NSUWatcher.Interfaces;

namespace NSUWatcher.Services.NSUWatcherNet.NetMessenger.Processors
{
#nullable enable
    public class WoodBoilerProcessor : IMsgProcessor
    {
        private readonly ICmdCenter _cmdCenter;
        private readonly ILogger _logger;

        public WoodBoilerProcessor(ICmdCenter cmdCenter, ILoggerFactory loggerFactory)
        {
            _cmdCenter = cmdCenter;
            _logger = loggerFactory?.CreateLoggerShort<WoodBoilerProcessor>() ?? NullLoggerFactory.Instance.CreateLoggerShort<WoodBoilerProcessor>();
        }

        public bool ProcessMessage(JObject message, NetClientData clientData, out INetMessage? response)
        {
            string target = (string)message[JKeys.Generic.Target]!;
            string action = (string)message[JKeys.Generic.Action]!;
            string? commandId = (string?)message[JKeys.Generic.Action];

            if(target == JKeys.WoodBoiler.TargetName) 
            {
                switch (action)
                {
                    case JKeys.WoodBoiler.ActionSwitch:
                        response = ProcessActionSwitch(message, clientData);
                        return true;
                    case JKeys.WoodBoiler.ActionIkurimas:
                        response = ProcessActionIkurimas(message, clientData);
                        return true;
                    default:
                        break;
                }
            }
            response = null;
            return false;
        }

        private INetMessage? ProcessActionSwitch(JObject message, NetClientData clientData)
        {
            string? value = (string?)message[JKeys.Generic.Value];
            string? name = (string?)message[JKeys.Generic.Name];
            if (value == null)
            {
                _logger.LogWarning($"ProcessActionSwitch: An required parameter 'value' not exists. Message: {message}");
                return null;
            }
            IExternalCommand? command = value switch 
            { 
                JKeys.WoodBoiler.TargetLadomat => _cmdCenter.ExtCommandFactory.WoodBoilerCommands.SwitchLadomatManual(name),
                JKeys.WoodBoiler.TargetExhaustFan => _cmdCenter.ExtCommandFactory.WoodBoilerCommands.SwitchExhaustFanManual(name),
                _ => null
            };
            _logger.LogDebug($"ProcessActionSwitch: calling '_cmdCenter.ExecExternalCommand(...)'");
            var response = _cmdCenter.ExecExternalCommand(command, clientData.NsuUser, null);            
            return response == null ? null : new NetMessage(response);
        }

        private INetMessage? ProcessActionIkurimas(JObject message, NetClientData clientData)
        {
            string? name = (string?)message[JKeys.Generic.Name];
            var command = _cmdCenter.ExtCommandFactory.WoodBoilerCommands.StartUp(name);
            var response = _cmdCenter.ExecExternalCommand(command, clientData.NsuUser, null);
            return response == null ? null : new NetMessage(response);
        }
    }
}
