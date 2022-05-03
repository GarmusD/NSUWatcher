using NSU.Shared;
using NSUWatcher.NSUSystem;
using System;
using System.Threading;

namespace NSUWatcher
{
    public class NSUWatcherService : System.ServiceProcess.ServiceBase
	{
		NSUSys nsuSys;

		protected override void OnStart (string[] args)
		{
            try
            {
                NSULog.Info("NSUWatcherService", "Starting NSUWatcherService...");
                ServiceName = "NSUWatcherService";
                var c = Config.Instance();
                nsuSys = new NSUSys();
                if(!nsuSys.Start())
                {
                    this.CanStop = true;
                    NSULog.Error("NSUWatcherService", "Service failed to start. Exiting...");
                }
            }
            catch(Exception ex)
            {
                NSULog.Exception("NSUWatcherService.OnStart()", ex.Message);
                throw;
            }
		}

		protected override void OnStop ()
		{
            NSULog.Info("NSUWatcherService", "Stopping NSUWatcherService...");
            nsuSys.Stop();
            nsuSys = null;
		}
	}
}

