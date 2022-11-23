using Newtonsoft.Json;
using NSU.Shared;
using NSUWatcher.Interfaces;

namespace NSUWatcher.NSUSystem.NSUSystemParts
{
    public class ExtCommandResult : IExternalCommandResult
    {
        public string Target { get; }

        public string Action { get; }

        public string Result { get; }

        public string ErrorMessage { get; } = string.Empty;
        public string Content { get; } = string.Empty;

        private ExtCommandResult(string target, string action, string result, string errorMessage = "", string content = "")
        {
            Target = target;
            Action = action;
            Result = result;
            ErrorMessage = errorMessage;
            Content = content;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static ExtCommandResult Success(string target, string action, string content = "")
        {
            return new ExtCommandResult(target, action, JKeys.Result.Ok, content:content);
        }

        public static ExtCommandResult Failure(string target, string action, string message)
        {
            return new ExtCommandResult(target, action, JKeys.Result.Error, message);
        }
    }
}
