using Newtonsoft.Json.Linq;
using NSU.Shared.NSUNet;
using NSUWatcher.Interfaces;

namespace NSUWatcher.NSUWatcherNet.NetMessenger
{
    public class NsuSysMsgProcessor : IMsgProcessor
    {
        private readonly ICmdCenter _cmdCenter;

        public NsuSysMsgProcessor(ICmdCenter cmdCenter)
        {
            _cmdCenter = cmdCenter;
        }

        public bool ProcessMessage(JObject message, out INetMessage response)
        {
            throw new System.NotImplementedException();
        }

    }
}
