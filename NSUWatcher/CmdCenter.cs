using NSUWatcher.NSUSystem;
using System;
using System.Timers;
using System.Collections.Generic;
using System.Threading;
using NSU.Shared;
using NSUWatcher.NSUSystem.NSUSystemParts;
using CommandMessenger.TransportLayer;
using CommandMessenger;
using System.Diagnostics;
using NSU.Shared.NSUTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Globalization;

namespace NSUWatcher
{

	public class CmdCenter
	{
        public delegate void CmdCenterStartedHandler();
        public delegate void ArduinoCrashedHandler();
        //public delegate void ArduinoDataReceivedHandler(JObject data);
        public delegate void ArduinoDataReceivedHandler(JObject data);

        public event CmdCenterStartedHandler OnCmdCenterStarted;
        public event ArduinoCrashedHandler OnArduinoCrashed;
        public event ArduinoDataReceivedHandler OnArduinoDataReceived;

		readonly string LogTag = "CmdCenter";

        SerialTransport transport;
        CmdMessenger messenger;
        System.Timers.Timer guard_timer;
        JsonSerializerSettings jsonsettings;
        private readonly Config _config;

        public bool Running { get; set; } = false;

        public CmdCenter(Config config)
		{
            _config = config ?? throw new ArgumentNullException(nameof(config), "Config object cannot be null.");
            transport = new SerialTransport
            {
                CurrentSerialSettings = { PortName = _config.PortName, BaudRate = _config.BaudRate, DtrEnable = false, RtsEnable = false } // object initializer
            };
            messenger = new CmdMessenger(transport, ' ', '\n')
            {
                BoardType = BoardType.Bit32 // Set if it is communicating with a 16- or 32-bit Arduino board
            };
            messenger.PrintLfCr = false;
            AttachCommandCallBacks();


            guard_timer = new System.Timers.Timer
            {
                Interval = 120000
            };
            guard_timer.Elapsed += OnGuardTimer;

            jsonsettings = new JsonSerializerSettings() { Culture = CultureInfo.CreateSpecificCulture("en-US") };
        }

        public void SendDTRSignal()
        {
            NSULog.Debug(LogTag, "Sending DTR signal...");
            transport.SerialPort.DtrEnable = true;
            Thread.Sleep(200);
            transport.SerialPort.DtrEnable = false;
        }

		public bool Start(bool dtrEnable = false)
		{
			NSULog.Debug(LogTag, $"Start(dtrEnable = {dtrEnable})");
            //With DTR enabled MCU will restart on connection
            if (dtrEnable)
            {
                //transport.CurrentSerialSettings.BaudRate = 1200;
                transport.CurrentSerialSettings.DtrEnable = true;

                if(messenger.StartListening())
                {
                    Thread.Sleep(200);
                    messenger.StopListening();
                    transport.CurrentSerialSettings.BaudRate = _config.BaudRate;
                    transport.CurrentSerialSettings.DtrEnable = false;

                    Thread.Sleep(200);
                }
                else
                {
                    NSULog.Error(LogTag, "Messenger not started listening with DTR enabled.");
                    return false;
                }
            }
            if (messenger.StartListening())
            {                
                NSULog.Debug(LogTag, "Started. OK.");
                Running = true;
                OnCmdCenterStarted?.Invoke();
                return true;
            }
            else
            {
                NSULog.Error(LogTag, "DoWork() - Messenger is Not Listening");
                Running = false;
                return false;
            }            
		}

        public void Stop()
        {
            if (Running)
            {
                NSULog.Debug(LogTag, "Stop()");

                if (messenger.StopListening())
                {
                    NSULog.Debug(LogTag, "Stopped. OK.");
                    guard_timer.Enabled = false;
                    Running = false;
                }
                else
                {
                    NSULog.Error(LogTag, "Stop() FAILED.");
                }
            }
            else
            {
                NSULog.Debug(LogTag, "messenger is not running.");
            }
        }

        public void Dispose()
        {
            Stop();
            messenger.Dispose();
            transport.Dispose();
        }

        /// Attach command call backs. 
		private void AttachCommandCallBacks()
        {
            messenger.Attach(OnUnknownCommand);
        }

        void OnUnknownCommand(ReceivedCommand arguments)
        {
            string str = arguments.CommandString();
            str = str.Replace('\n'.ToString(), "");
            str = str.Replace('\r'.ToString(), "");
            if (str.Length > 2)//
            {
                ArduinoSerialLine(str.Remove(0, str.IndexOf(' ') + 1));//Remove cmdID and space
            }
        }

        private void ExtPrc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            NSULog.Debug("ExternalProccess", e.Data);
        }

        //TODO Sutvarkyti Arduino perkrovima
        void OnGuardTimer(object sender, ElapsedEventArgs e)
        {
            NSULog.Debug(LogTag, "OnGuardTimer(). ARDUINO CRASHED - raising OnArduinoCrashed!");
            try
            {
                guard_timer.Enabled = false;
                OnArduinoCrashed?.Invoke();
            }
            catch(Exception ex)
            {
                NSULog.Exception(LogTag, "OnGuardTimer(): "+ex);
            }
            return;
        }


        public void SendToArduino(string cmd, bool waitAck = false)
        {
            NSULog.Debug(LogTag, string.Format("SendSerial: '{0}'", cmd));
            var scmd = new SendCommand(0, cmd);
            scmd.ReqAc = waitAck;
            messenger.SendCommand(scmd);
        }

        private void ArduinoSerialLine(string receivedLine)
        {
            if (receivedLine.StartsWith("VMD", StringComparison.Ordinal)) //Disable VisualMicro debug messages				
                return;

            if (receivedLine.Equals("GUARD"))
            {
                guard_timer.Enabled = false;
                guard_timer.Enabled = true;
            }
            else
            {   //LogLine
                if (receivedLine.StartsWith("DBG:", StringComparison.Ordinal))
                {
                    NSULog.Debug("Arduino", receivedLine.Remove(0, receivedLine.IndexOf(' ') + 1));
                }
                else if (receivedLine.StartsWith("ERR:", StringComparison.Ordinal))
                {
                    NSULog.Error("Arduino", receivedLine.Remove(0, receivedLine.IndexOf(' ') + 1));
                }
                else
                {
                    NSULog.Info("Arduino", receivedLine.Remove(0, receivedLine.IndexOf(' ') + 1));
                }
            }

            if(receivedLine.StartsWith("JSON:", StringComparison.Ordinal))
            {
                string strtojson = receivedLine.Remove(0, receivedLine.IndexOf(' ') + 1);
                try
                {                     
                    var jo = (JObject)JsonConvert.DeserializeObject(strtojson, jsonsettings);
                    //NSULog.Debug(LogTag, "Calling OnArduinoDataReceived?.Invoke(jo);");
                    OnArduinoDataReceived?.Invoke(jo);
                }
                catch(Exception ex)
                {
                    NSULog.Exception(LogTag+" JsonConvert.DeserializeObject("+strtojson+")", ex.Message);
                }
            }
        }
	}
}

