using NSU.Shared;
using System;
using System.Threading;

namespace NSUWatcher
{
    public class NSUWatcherService : System.ServiceProcess.ServiceBase
	{
		CmdCenter cmdCenter;

		protected override void OnStart (string[] args)
		{
            try
            {
                var c = Config.Instance();
                cmdCenter = new CmdCenter();
                cmdCenter.Start();
            }
            catch(Exception ex)
            {
                NSULog.Exception("NSUWatcherService.OnStart()", ex.Message);
                throw;
            }
		}

		protected override void OnStop ()
		{
			cmdCenter.Stop();
            cmdCenter = null;
		}
	}
}

