using Newtonsoft.Json.Linq;
using NSU.Shared.NSUNet;

namespace NSUWatcher.NSUWatcherNet.NetMessenger
{
    public interface IMsgProcessor
    {
        bool ProcessMessage(JObject message, out INetMessage response);
    }
}
