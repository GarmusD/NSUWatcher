using Newtonsoft.Json;
using NSU.Shared;

namespace NSUWatcher.NSUWatcherNet.NetMessenger
{
#nullable enable
    public static class StandartResponses
    {
        public static NetMessage Error(string target, string action, string errCode, string? commandId)
        {
            return new NetMessage(new ErrorResponse() 
            { 
                Target = target,
                Action = action,
                Result = JKeys.Result.Error,
                ErrorCode = errCode,
                CommandID = commandId
            });
        }
    }

    public class ErrorResponse
    {
        [JsonProperty(JKeys.Generic.Target)]
        public string Target { get; set; } = string.Empty;

        [JsonProperty(JKeys.Generic.Target)]
        public string Action { get; set; } = string.Empty;

        [JsonProperty(JKeys.Generic.Action)]
        public string Result { get; set; } = string.Empty;

        [JsonProperty(JKeys.Generic.ErrCode)] 
        public string ErrorCode { get; set; } = string.Empty;

        [JsonProperty(JKeys.Generic.CommandID)]
        public string? CommandID { get; set; } = null;
    }
}
