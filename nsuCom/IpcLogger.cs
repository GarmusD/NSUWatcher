using Newtonsoft.Json;
using System;
using TransportDataContracts;

namespace serialTransport
{
    internal class IpcLogger : ILogger
    {
        public IpcLogger()
        {
        }

        private void Send(string action, string message)
        {
            Console.WriteLine(
                JsonConvert.SerializeObject(
                    new TransportData()
                    {
                        Destination = Constants.DestinationLogger,
                        Action = action,
                        Content = message
                    }
                )
            );
        }

        public void Debug(string message)
        {
            Send(Constants.ActionLogDebug, message);
        }

        public void Error(string message)
        {
            Send(Constants.ActionLogError, message);
        }

        public void Fatal(string message)
        {
            Send(Constants.ActionLogFatal, message);
        }

        public void Info(string message)
        {
            Send(Constants.ActionLogInfo, message);
        }

        public void Warn(string message)
        {
            Send(Constants.ActionLogWarning, message);
        }
    }
}
