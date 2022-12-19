using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json.Linq;
using NSU.Shared;
using NSUWatcher.Interfaces.MCUCommands;
using System;
using System.Collections.Generic;

namespace NSUWatcher.CommandCenter.MessagesFromMcu.Factory.ArduinoV1
{
    public class ArduinoV1Messages : IFromMcuMessages
    {
        private readonly ILogger _logger;
        private readonly List<IFromArduinoV1Base> _commands;

        public ArduinoV1Messages(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory?.CreateLogger<ArduinoV1Messages>() ?? NullLoggerFactory.Instance.CreateLogger<ArduinoV1Messages>();
            _commands = new List<IFromArduinoV1Base>() 
            { 
                new SystemMessages(),
                new CircPumpMessages(),
                new CollectorMessages(),
                new ComfortZoneMessages(),
                new KTypeMessages(),
                new RelayMessages(),
                new SystemFanCommands(),
                new SwitchMessages(),
                new TSensorsMessages(),
                new TempTriggerMessages(),
                new WaterBoilerMessages(),
                new WoodBoilerMessages(),
                new AllarmMessages(),
            };
        }

        public IMessageFromMcu Parse(string command)
        {
            var jo = JObject.Parse(command);
            if(!ValidateCommand(jo))
            {
                _logger.LogError("The message from arduino does not contain required 'target' and 'action' keys. Command: {command}", command);
                return null;
            }
            RenameTargetToSource(jo);
            return TryFindCommand(jo);
        }

        private bool ValidateCommand(JObject jo)
        {
            return (jo.ContainsKey(JKeys.Generic.Target) || jo.ContainsKey(JKeys.Generic.Source)) && jo.ContainsKey(JKeys.Generic.Action);
        }
        
        private void RenameTargetToSource(JObject jo)
        {
            if(jo.ContainsKey(JKeys.Generic.Target))
            {
                jo[JKeys.Generic.Source] = (string)jo[JKeys.Generic.Target];
                jo.Remove(JKeys.Generic.Target);
            }
        }

        private IMessageFromMcu TryFindCommand(JObject jo)
        {
            foreach (var item in _commands)
            {
                var cmd = SafeFindMessage(item, jo);
                if(cmd != null) return cmd;
            }
            return null;
        }

        private IMessageFromMcu SafeFindMessage(IFromArduinoV1Base commands, JObject jo)
        {
            try
            {
                return commands.TryFindMessage(jo);
            }
            catch(Exception ex)
            {
                _logger.LogError("TryFindMessage thrown exception: {ex}", ex);
                _logger.LogError("JObject: {jo}", jo.ToString());
                return null;
            }
        }
    }
}
