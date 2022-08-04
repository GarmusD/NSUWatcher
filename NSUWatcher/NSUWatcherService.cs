using NSU.Shared;
using NSUWatcher.NSUSystem;
using System;
using System.Threading;

namespace NSUWatcher
{
    public class NSUWatcherService : System.ServiceProcess.ServiceBase
	{
		private NSUSys _nsuSys;

        public NSUWatcherService(string configFile = "")
        {
            if(!string.IsNullOrEmpty(configFile))
                Config.CfgFile = configFile;
        }

		protected override void OnStart (string[] args)
		{
            try
            {
                NSULog.Info("NSUWatcherService", "Starting NSUWatcherService...");
                ServiceName = "NSUWatcherService";
                _nsuSys = new NSUSys(new Config());
                if(!_nsuSys.Start())
                {
                    this.CanStop = true;
                    NSULog.Error("NSUWatcherService", "Service failed to start. Exiting...");
                }
            }
            catch(Exception ex)
            {
                NSULog.Exception("NSUWatcherService.OnStart()", ex.Message);
                this.CanStop = true;
                throw;
            }
		}

		protected override void OnStop ()
		{
            NSULog.Info("NSUWatcherService", "Stopping NSUWatcherService...");
            _nsuSys?.Stop();
            _nsuSys = null;
		}
	}
}

