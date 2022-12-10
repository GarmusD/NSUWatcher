using Newtonsoft.Json.Linq;

namespace NSUWatcher.Transport.TestTransport.Commands
{
    internal interface ITestCommand
    {
        string Exec(JObject command);
    }
}
