using NSU.Shared;
using NSU.Shared.DTO.ExtCommandContent;
using NSU.Shared.Serializer;
using NSUWatcher.Interfaces;
using NSUWatcher.Interfaces.ExtCommands;

namespace NSUWatcher.CommandCenter.ExtCommands
{
    public class CircPumpCommands : ICircPumpCommands
    {
        private readonly INsuSerializer _serializer;

        public CircPumpCommands(INsuSerializer serializer)
        {
            _serializer = serializer;
        }

        public IExternalCommand Setup(byte configPos, bool enabled, string name, string tempTriggerName, byte maxSpeed, byte speed1Ch, byte speed2Ch, byte speed3Ch)
        {
            return new ExternalCommand() 
            {
                Target = JKeys.CircPump.TargetName,
                Action = JKeys.Generic.Setup,
                Content = _serializer.Serialize(
                    new CircPumpUpdateContent(
                        configPos,
                        enabled,
                        name,
                        tempTriggerName,
                        maxSpeed,
                        speed1Ch,
                        speed2Ch,
                        speed3Ch
                    ))
            };
        }

        public IExternalCommand Clicked(string name)
        {
            return new ExternalCommand() 
            {
                Target = JKeys.CircPump.TargetName,
                Action = JKeys.Action.Click,
                Content = name
            };
        }

        
    }
}
