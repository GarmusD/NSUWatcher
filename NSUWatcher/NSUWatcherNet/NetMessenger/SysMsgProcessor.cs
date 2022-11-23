using Newtonsoft.Json.Linq;
using NSU.Shared;
using NSU.Shared.DTO.NsuNet;
using NSU.Shared.NSUNet;

namespace NSUWatcher.NSUWatcherNet.NetMessenger
{
    public class SysMsgProcessor : IMsgProcessor
    {
        public bool ProcessMessage(JObject message, out INetMessage? response)
        {
            string target = (string)message[JKeys.Generic.Target]!;
            string action = (string)message[JKeys.Generic.Action]!;

            if(target == JKeys.Syscmd.TargetName)
            {
                if(action == JKeys.SystemAction.Handshake)
                {
                    response = ProcessActionHandshake();
                    return true;
                }
            }
            response = null;
            return false;
        }

        private INetMessage ProcessActionHandshake()
        {
            HandshakeResponse response = new HandshakeResponse()
            { 
                Target = JKeys.Syscmd.TargetName,
                Action = JKeys.SystemAction.Handshake,
                Name = "NSUServer",
                Version = $"{Messenger.VERSION_MAJOR}.{Messenger.VERSION_MINOR}",
                Protocol = Messenger.PROTOCOL_VERSION
            };
            return new NetMessage(response);
        }
    }
}
