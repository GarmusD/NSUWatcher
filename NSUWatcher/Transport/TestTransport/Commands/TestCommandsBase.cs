using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NSUWatcher.Transport.TestTransport.Commands
{
    internal abstract class TestCommandsBase
    {
        protected readonly ILogger _logger;
        protected readonly CommandSender _sender;

        public TestCommandsBase(CommandSender commandSender, ILogger logger)
        {
            _logger = logger;
            _sender = commandSender;
        }

        public abstract bool ExecCommand(JObject command);
        public abstract Task RunAsync(CancellationToken cancellationToken);
    }
}
