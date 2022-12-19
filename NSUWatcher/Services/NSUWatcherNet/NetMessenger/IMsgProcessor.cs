using Newtonsoft.Json.Linq;
using NSU.Shared.NSUNet;

namespace NSUWatcher.NSUWatcherNet.NetMessenger
{
#nullable enable
    public interface IMsgProcessor
    {
        bool ProcessMessage(JObject message, NetClientData clientData, out INetMessage? response);
    }
}
