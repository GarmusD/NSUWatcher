using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NSUWatcher.Transport.TestTransport
{
    internal class CommandSender
    {
        private enum ContentType { Command, ScopeBegin, ScopeEnd}
        private struct Content
        {
            public ContentType Type;
            public string Value;
        }

        private readonly Action<string> _sender;
        private readonly object _sendLock = new object();
        private readonly Queue<Content> _commands = new Queue<Content>();
        private int _taskDelay = 100;

        public CommandSender(Action<string> sender)
        {
            _sender = sender;
        }

        public async Task RunAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (_commands.Any())
                {
                    Content content = _commands.Dequeue();
                    switch (content.Type)
                    {
                        case ContentType.Command:
                            string cmd = content.Value;
                            DoSend(cmd);
                            break;
                        case ContentType.ScopeBegin:
                            _taskDelay = 10;
                            break;
                        case ContentType.ScopeEnd:
                            _taskDelay = 100;
                            break;
                        default:
                            break;
                    }
                }
                await Task.Delay(_taskDelay);
            }
        }

        public void SendNow(string command, string prefix = "JSON: ")
        {
            DoSend(prefix + command);
        }

        public void Send(string command, string prefix = "JSON: ")
        {
            _commands.Enqueue(new Content() { Type = ContentType.Command, Value = prefix + command });
        }

        public void BeginSnapshot()
        {
            _commands.Enqueue(new Content() { Type = ContentType.ScopeBegin, Value = string.Empty });
        }

        public void EndSnapshot()
        {
            _commands.Enqueue(new Content() { Type = ContentType.ScopeEnd, Value = string.Empty });
        }

        private void DoSend(string command)
        {
            lock (_sendLock)
            {
                _sender(command);
            }
        }
    }
}
