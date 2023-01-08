using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NSU.Shared;
using NSU.Shared.DataContracts;
using NSU.Shared.NSUNet;
using NSUWatcher.Interfaces;
using NSUWatcher.Interfaces.NsuUsers;
using NSUWatcher.Services.NSUWatcherNet.NetMessenger.Processors;
using System.Collections.Generic;

namespace NSUWatcher.Services.NSUWatcherNet.NetMessenger
{
    public class Messenger
    {
        public const int VERSION_MAJOR = 0;
        public const int VERSION_MINOR = 4;
        public const int PROTOCOL_VERSION = 3;


        private readonly NetServer _netServer;
        private readonly INsuSystem _nsuSystem;
        private readonly ILogger _logger;

        private readonly List<IMsgProcessor> _msgProcessors;

        public Messenger(NetServer netServer, INsuUsers nsuUsers, ICmdCenter cmdCenter, INsuSystem nsuSystem, IConfiguration config, ILoggerFactory loggerFactory)
        {
            _netServer = netServer;
            _nsuSystem = nsuSystem;
            _logger = loggerFactory?.CreateLoggerShort<Messenger>() ?? NullLoggerFactory.Instance.CreateLoggerShort<Messenger>();
            _msgProcessors = new List<IMsgProcessor>()
            {
                new SysMsgProcessor(nsuSystem, nsuUsers, config, loggerFactory),
                new NsuSysMsgProcessor(cmdCenter),
                new CircPumpProcessor(cmdCenter, loggerFactory),
                new WoodBoilerProcessor(cmdCenter, loggerFactory)
            };

            _nsuSystem.EntityStatusChanged += NsuSystem_StatusChanged;
        }

        public INetMessage ProcessNetMessage(INetMessage message, NetClientData clientData)
        {
            _logger.LogDebug("ProcessNetMessage()");
            if (message.DataType == DataType.String)
            {
                try
                {
                    string dataStr = message.AsString();

                    JObject jsonData = JObject.Parse(dataStr);
                    if (JsonDataIsValid(jsonData))
                    {
                        return ProcessJsonData(jsonData, clientData);
                    }
                }
                catch (JsonReaderException) { }
            }
            _logger.LogWarning($"ProcessNetMessage: NetDataType '{message.DataType}' is not implemented.");
            return null;
        }

        private bool JsonDataIsValid(JObject jo)
        {
            return jo.ContainsKey(JKeys.Generic.Target) && jo.ContainsKey(JKeys.Generic.Action);
        }

        private INetMessage ProcessJsonData(JObject jsonData, NetClientData clientData)
        {
            foreach (var processor in _msgProcessors)
            {
                if (processor.ProcessMessage(jsonData, clientData, out INetMessage netMessage))
                {
                    _logger.LogDebug($"ProcessJsonData() returning: {netMessage?.GetType().Name}.");
                    return netMessage;
                }
            }
            _logger.LogWarning($"Unsupported message received: {jsonData}");
            return null;
        }



        private void NsuSystem_StatusChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            NetMessage netMessage = null;
            NetClientRequirements requirements = NetClientRequirements.CreateStandartAcceptInfo();
            switch (sender)
            {
                case IAlarmDataContract alarmData:
                    netMessage = AlarmChanges.Message(alarmData, e.PropertyName);
                    break;

                case ICircPumpDataContract circPumpData:
                    netMessage = CircPumpChanges.Message(circPumpData, e.PropertyName);
                    break;

                case ICollectorDataContract collectorData:
                    //TODO Send ICollectorDataContract changes
                    break;

                case IComfortZoneDataContract comfortZoneData:
                    netMessage = ComfortZoneChanges.Message(comfortZoneData, e.PropertyName);
                    break;

                case IConsoleDataContract consoleData:
                    //TODO Send IConsoleDataContract changes
                    break;

                case IKTypeDataContract kTypeData:
                    netMessage = KTypeChanges.Message(kTypeData, e.PropertyName);
                    break;

                case IMCUStatusDataContract mcuStatusData:
                    //TODO Send IMCUStatusDataContract changes
                    break;

                case IRelayModuleDataContract relayModuleData:
                    netMessage = RelayModuleChanges.Message(relayModuleData, e.PropertyName);
                    break;

                case ISystemFanDataContract systemFanData:
                    netMessage = SystemFanChanges.Message(systemFanData, e.PropertyName);
                    break;

                case ISwitchDataContract switchData:
                    netMessage = SwitchChanges.Message(switchData, e.PropertyName);
                    break;

                case ITempSensorDataContract tempSensorData:
                    netMessage = TSensorChanges.Message(tempSensorData, e.PropertyName);
                    break;

                case ITempTriggerDataContract tempTriggerData:
                    netMessage = TempTriggerChanges.Message(tempTriggerData, e.PropertyName);
                    break;

                case IWaterBoilerDataContract waterBoilerData:
                    //TODO Send IWaterBoilerDataContract changes
                    break;

                case IWoodBoilerDataContract woodBoilerData:
                    netMessage = WoodBoilerChanges.Message(woodBoilerData, e.PropertyName);
                    break;

                default:
                    _logger.LogWarning($"NsuSystem StatusChanged handler for data contract [{sender?.GetType().FullName}] not implemented.");
                    break;
            }
            if (netMessage != null)
                _netServer.Send(netMessage, requirements);
        }
    }
}
