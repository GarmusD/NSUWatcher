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

        readonly NSUSys nsusystem;
        SerialTransport transport;
        CmdMessenger messenger;
        System.Timers.Timer guard_timer;

		public CmdCenter()
		{
			nsusystem = new NSUSys(this);
		}

		public void Stop()
		{
			NSULog.Debug(LogTag, "Stop()");
			//are.Set();
            messenger.StopListening();
            messenger.Dispose();
            transport.Dispose();
            //worker.Join();
			nsusystem.Stop();
            guard_timer.Enabled = false;            
        }

		public bool Start()
		{
			NSULog.Debug(LogTag, "Start()");

            transport = new SerialTransport
            {
                CurrentSerialSettings = { PortName = Config.Instance().PortName, BaudRate = Config.Instance().BaudRate, DtrEnable = false } // object initializer
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

            if (messenger.StartListening())
            {
                /*
                NSULog.Debug(LogTag, "DoWork() - Messenger is Listening;");
                worker = new Thread(new ThreadStart(CmdCenterTaskThread));
                worker.Name = "CmdCenterThread";
                worker.Start();
                */
                OnCmdCenterStarted?.Invoke();
                return true;
            }
            else
            {
                NSULog.Debug(LogTag, "DoWork() - Messenger is Not Listening");
                return false;
            }
            
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

        void StartExternalProccess(string fileName, string arguments)
        {
            var prc = new Process();
            ProcessStartInfo psi = new ProcessStartInfo(fileName);
            psi.Arguments = arguments;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;

            prc.StartInfo = psi;
            prc.EnableRaisingEvents = true;
            prc.OutputDataReceived += ExtPrc_OutputDataReceived; ;
            //prc.Exited += HandlePrcExited;            
            prc.Start();
            prc.BeginOutputReadLine();
            prc.WaitForExit(5000);
        }

        private void WriteToGPIO(byte value)
        {
            try
            {
                using (FileStream fs = new FileStream("/sys/class/gpio/gpio0/value", FileMode.Open, FileAccess.Write))
                {
                    fs.Seek(0, SeekOrigin.Begin);
                    fs.WriteByte(value);
                    fs.Flush();
                    fs.Dispose();
                }
            }
            catch (Exception ex)
            {
                NSULog.Exception(LogTag, "WriteToGPIO()" + ex.Message);
            }
        }

        private void ExtPrc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            NSULog.Debug("ExternalProccess", e.Data);
        }

        //TODO Sutvarkyti Arduino perkrovima
        void OnGuardTimer(object sender, ElapsedEventArgs e)
        {
            NSULog.Debug(LogTag, "OnGuardTimer(). ARDUINO CRASHED - rebooting!");
            try
            {
                OnArduinoCrashed?.Invoke();
            }
            finally
            {
                var cmd = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "reboot_arduino.sh");
                NSULog.Debug(LogTag, $"Calling '/bin/bash {cmd}'");
                Process.Start("/bin/bash", cmd);
            }
            return;

            try
            {
                guard_timer.Enabled = false;
                //StartExternalProccess("sudo echo 0 > /sys/class/gpio/gpio0/value", "");
                WriteToGPIO(0);
                System.Threading.Thread.Sleep(100);
                //StartExternalProccess("sudo echo 1 > /sys/class/gpio/gpio0/value", "");                
                WriteToGPIO(1);
            }
            catch (Exception ex)
            {
                NSULog.Exception(LogTag, "OnGuardTimer(): " + ex.Message);
            }
            //sudo echo 0 > /sys/class/gpio/gpio0/value
            //sudo echo 1 > /sys/class/gpio/gpio0/value
        }


        public void SendToArduino(string cmd, bool waitAck = false)
        {
            NSULog.Debug(LogTag, string.Format("SendSerial: '{0}'", cmd));
            var scmd = new SendCommand(0, cmd);
            scmd.ReqAc = waitAck;
            messenger.SendCommand(scmd);
        }

        public void ManualCommand(string cmd)
        {
            NSULog.Debug(LogTag, $"ManualCommand received: {cmd}");
            var vs = new JObject();
            vs.Add(JKeys.Generic.Target, JKeys.UserCmd.TargetName);
            vs.Add(JKeys.Generic.Value, cmd);
            OnArduinoDataReceived?.Invoke(vs);
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
                    var jo = (JObject)JsonConvert.DeserializeObject(strtojson, new JsonSerializerSettings(){ Culture=CultureInfo.CreateSpecificCulture("en-US") });
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

