using System;

namespace NSUWatcher.CommandCenter
{
    public class McuMessageReceivedArgs : EventArgs
    {
        public string McuMessage { get; }

        public McuMessageReceivedArgs(string data)
        {
            McuMessage = data;
        }
    }
}
