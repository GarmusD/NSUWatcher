using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TransportDataContracts;

namespace serialTransport
{
    internal class Program
    {
        private static IpcLogger _logger;
        private static Messenger _messenger = null;
        private static ConfigContract _config;
        private static bool _terminate = false;
        private static bool _isRebooting = false;

        static void Main(string[] args)
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            try
            {
                ReadConfig(args);
                _logger = new IpcLogger();
                SetupMessenger();
                Run();
                Environment.Exit(0);
            }
            catch (Exception ex) when (ex is ExceptionWithExitCode exCode)
            {
                if (exCode.Message != null) Console.Error.WriteLine(exCode.Message);
                Environment.Exit(exCode.ErrorCode);
            }
            Environment.Exit(Constants.ExitCodeUnknown);
        }

        private static void ReadConfig(string[] args)
        {
            try
            {
                _config = new ConfigContract();
                _config.BaudRate = int.Parse(GetArgValue(args, Constants.ArgBaudRate));
                _config.ComPort = GetArgValue(args, Constants.ArgComPort);
                _config.RebootMcu.BaudRate = int.Parse(GetArgValue(args, Constants.ArgRebootBaudrate));
                _config.RebootMcu.DtrPulseOnly = bool.Parse(GetArgValue(args, Constants.ArgRebootDtrPulseOnly));
                _config.RebootMcu.Delay = int.Parse(GetArgValue(args, Constants.ArgRebootDelay));
                _config.RebootMcu.ReconnectDelay = int.Parse(GetArgValue(args, Constants.ArgRebootReconnectDelay));
            }
            catch (Exception ex)
            {
                throw new ExceptionWithExitCode(Constants.ExitCodeArgsError, ex.Message);
            }
        }

        private static string GetArgValue(string[] args, string arg)
        {
            string value = args.FirstOrDefault(x => x.StartsWith(arg, StringComparison.Ordinal))?.Remove(0, arg.Length);
            if (string.IsNullOrEmpty(value))
            {
                Environment.Exit(Constants.ExitCodeArgsError);
            }
            return value;
        }

        private static void SendData(ITransportData data)
        {
            Console.WriteLine(JsonConvert.SerializeObject(data));
        }

        static void Run()
        {
            while (!_terminate)
            {
                string dataStr = Console.ReadLine();
                TransportData data = null;
                if (dataStr != null)
                    data = JsonConvert.DeserializeObject<TransportData>(dataStr);

                if (data != null)
                {
                    switch (data.Destination)
                    {
                        case Constants.DestinationSystem:
                            ProcessSystemCommand(data);
                            break;

                        case Constants.DestinationMcu:
                            ProcessMcuData(data);
                            break;

                        default:
                            _logger.Error($"Target '{data.Destination}' not implemented.");
                            break;
                    }
                }
                else
                {
                    _logger.Error($"Deserialization to IpcData failed: {dataStr}");
                }
            }
            _messenger.Disconnect();
            _messenger.Dispose();
        }
        
        private static void SetupMessenger()
        {
            if (_messenger != null)
            {
                _messenger.Dispose();
            }
            _messenger = new Messenger(_config.ComPort, _config.BaudRate, _logger);

            _messenger.ArduinoCrashed += (s, e) =>
            {
                SendData(new TransportData()
                {
                    Destination = Constants.DestinationSystem,
                    Action = Constants.ActionMcuHalted
                });
                _ = ExecReboot();
            };

            _messenger.McuDataReceived += (s, e) =>
            {
                SendData(new TransportData()
                {
                    Destination = Constants.DestinationMcu,
                    Action = Constants.ActionData,
                    Content = e.DataString
                });
            };
        }

        private static void ProcessSystemCommand(ITransportData data)
        {
            switch (data.Action)
            {
                case Constants.ActionConnect:
                    SysCmdConnect(data.Content);
                    break;

                case Constants.ActionDisconnect:
                    SysCmdDisconnect(data.Content);
                    break;

                case Constants.ActionRebootMcu:
                    SysCmdRebootMcu(data.Content);
                    break;

                case Constants.ActionQuit:
                    _terminate = true;
                    break;

                default:
                    _logger.Error($"The Action '{data.Action}' for System command not implemented.");
                    break;
            }
        }

        

        private static void SysCmdConnect(string content)
        {
            _ = content;
            var result = _messenger.Connect();
            if (result)
            {
                SendMessageConnected();
                return;
            }

            SendConnectFailedAndTerminate();
        }

        private static void SendConnectFailedAndTerminate()
        {
            SendMessageConnectFailed();
            Environment.Exit(Constants.ExitCodeCommError);
        }

        private static void SendMessageConnectFailed()
        {
            SendData(new TransportData()
            {
                Destination = Constants.DestinationSystem,
                Action = Constants.ActionConnectFailed
            });
        }

        private static void SendMessageConnected()
        {
            SendData(new TransportData()
            {
                Destination = Constants.DestinationSystem,
                Action = Constants.ActionConnected
            });
        }

        private static void SysCmdDisconnect(string content)
        {
            _ = content;
            if(_messenger.IsConnected)
            {
                _messenger.Disconnect();
                SendData(new TransportData() 
                { 
                    Destination = Constants.DestinationSystem,
                    Action = Constants.ActionDisconnect
                });
            }
        }

        private static void SysCmdRebootMcu(string content)
        {
            _ = content;
            _ = ExecReboot();
        }

        private static async Task ExecReboot()
        {
            await _messenger.RebootMcu(_config.RebootMcu.DtrPulseOnly, _config.RebootMcu.BaudRate, _config.RebootMcu.Delay);
            int count = 0;
            while(true)
            {
                await Task.Delay(_config.RebootMcu.ReconnectDelay);
                var result = _messenger.Connect();
                if(result)
                {
                    SendMessageConnected();
                    return;
                }

                if(++count > 3)
                {
                    SendConnectFailedAndTerminate();
                }
            }
        }

        private static void ProcessMcuData(ITransportData data)
        {
            if (_messenger != null)
                _messenger.SendToArduino(data.Content);
        }
    }
}
