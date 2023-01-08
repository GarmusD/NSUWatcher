using NSUShared.DTO.NsuNet;

namespace NSUWatcher.Services.NSUWatcherNet.NetMessenger
{
#nullable enable
    public class Error
    {
        public static NetMessage Create(string target, string action, string errCode, string? commandId)
        {
            return new NetMessage(
                ErrorResponse.Create(target, action, errCode, commandId)
                );
        }
    }
}
