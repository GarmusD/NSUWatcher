using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NSU.Shared;
using NSUWatcher.Transport.TestTransport.Commands;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NSUWatcher.Transport.TestTransport
{
    internal class TestCommands
    {
        private readonly ILogger _logger;
        private readonly List<TestCommandsBase> _testCommands;

        public TestCommands(CommandSender commandSender, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLoggerShort<TestCommands>();
            _testCommands = new List<TestCommandsBase>()
            { 
                new SysCommands(commandSender, loggerFactory),
                new TSensorCommands(commandSender, loggerFactory)
            };
        }

        public void Execute(string command)
        {
            try
            {
                JObject jo = JObject.Parse(command);
                if(jo.ContainsKey(JKeys.Generic.Target) && jo.ContainsKey(JKeys.Generic.Action))
                {
                    foreach (var item in _testCommands)
                    {
                        if (item.ExecCommand(jo))
                            return;
                    }
                    _logger.LogWarning($"Executor is not implemented [{command}]");
                }
                else
                {
                    _logger.LogWarning($"Command is mallformed: {command}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"{command} - {ex.Message}");
            }
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Starting TestCommands tasks...");
            CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            List<Task> tasks = new List<Task>();
            foreach (var item in _testCommands)
            {
                tasks.Add(item.RunAsync(cts.Token));
            }
            await Task.WhenAll(tasks);
            _logger.LogDebug("All TestCommands tasks is finished.");
        }
    }
}
