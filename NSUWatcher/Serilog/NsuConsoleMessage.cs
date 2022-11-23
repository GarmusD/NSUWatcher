using System;
using System.Runtime.CompilerServices;

namespace NSUWatcher.Serilog
{
    public class NsuConsoleEventArgs : EventArgs
    {
        public string Output { get; }

        public NsuConsoleEventArgs(string output)
        {
            Output = output;
        }
    }

    public static class NsuConsoleMessage
    {
        public static event EventHandler<NsuConsoleEventArgs>? OutputReceived;

        public static void SetOutoutLine(string line)
        {
            OutputReceived?.Invoke(null, new NsuConsoleEventArgs(line));
        }
    }
}
