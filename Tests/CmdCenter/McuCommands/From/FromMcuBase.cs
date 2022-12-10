using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSUWatcher.CommandCenter.MessagesFromMcu.Factories;
using NSUWatcher.Interfaces.MCUCommands;

namespace Tests.CmdCenter.McuCommands.From
{
    public class FromMcuBase
    {
        protected readonly IFromMcuMessages _messagesFromMcu;

        public FromMcuBase()
        {
            _messagesFromMcu = MessageFromMcuFactories.GetDefault(NullLoggerFactory.Instance);
        }

        protected void Check<T>(string jsonStr)
        {
            var result = _messagesFromMcu.Parse(jsonStr);
            Assert.IsTrue(result is T);
        }
    }
}
