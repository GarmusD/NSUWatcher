using System;
using System.Timers;
//using System.Linq;
using System.Net.NetworkInformation;
using Serilog;
using System.Threading;
using Timer = System.Timers.Timer;

namespace NSUWatcher.NSUWatcherNet
{
    public partial class NetServer
    {
        /// <summary>
        /// Helps to find out when network adapter is ready and connected
        /// </summary>
        private class AdapterAvailableHelper : IDisposable
        {
            private readonly ILogger _logger;
            private readonly Timer _timer;
            private ManualResetEvent? _mre = null;
            private int _counter = 0;

            public AdapterAvailableHelper(ILogger logger)
            {
                _logger = logger.ForContext<AdapterAvailableHelper>();
                _timer = new Timer(5000);
                _timer.Elapsed += HelperTimer_Elapsed;
                _timer.AutoReset = true;
            }

            public void Dispose()
            {
                _logger.Debug("Dispose() called.");
                _timer.Dispose();
                _mre?.Dispose();
            }

            public void Stop()
            {
                _timer.Enabled = false;
                _mre?.Set();
            }

            /// <summary>
            /// Blocking call until network adapter is ready or cancellation requested
            /// </summary>
            /// <param name="token">Cancellation token</param>
            /// <returns></returns>
            public bool WaitForAdapterReady(CancellationToken token)
            {
                if (!IsNetworkAdapterReady())
                {
                    token.Register(Stop);
                    _logger.Warning("Network adapter is not ready yet.");
                    _timer.Enabled = true;
                    _mre = new ManualResetEvent(false);
                    _mre.WaitOne();
                }
                return IsNetworkAdapterReady();
            }

            /// <summary>
            /// Raise event if adapter is ready
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void HelperTimer_Elapsed(object sender, ElapsedEventArgs e)
            {
                if (IsNetworkAdapterReady() || ++_counter >= 10)
                {
                    _timer.Enabled = false;
                    _mre?.Set();
                }
                else _logger.Warning("Network adapter still not ready.");
            }

            /// <summary>
            /// Check network adapter is connected
            /// </summary>
            /// <returns>true if connected</returns>
            private bool IsNetworkAdapterReady()
            {
                return NetworkInterface.GetIsNetworkAvailable();
            }
        }
    }
}
