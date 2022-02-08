using NSU.Shared;
using System;
using System.Globalization;
using System.ServiceProcess;
using System.Threading;

namespace NSUWatcher
{
    class Program
    {
        static readonly string LogTag = "NSUWatcherMain";

        static void Main (string [] args)
        {
            CultureInfo culture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;            

            if (args.Length > 0 && (args [0].Equals ("noservice") || args [0].Equals ("--noservice")))
            {
                Console.WriteLine("Current Culture:    {0}",
                        Thread.CurrentThread.CurrentCulture);
                Console.WriteLine("Current UI Culture: {0}",
                                  Thread.CurrentThread.CurrentUICulture);

                var cfg = Config.Instance ();

                NSULog.Debug (LogTag, "Main(). Starting worker thread.");
                try
                {
                    var cmdCenter = new CmdCenter();
                    if (cmdCenter.Start())
                    {
                        bool done = false;
                        while (!done)
                        {
                            string str = Console.ReadLine();
                            if (str.Equals("q", StringComparison.OrdinalIgnoreCase))
                            {
                                done = true;
                            }
                            else
                            {
                                cmdCenter.ManualCommand(str);
                            }
                        }
                        NSULog.Debug(LogTag, "Main(). Quit requested.");
                        cmdCenter.Stop();
                    }
                    else
                    {
                        Console.WriteLine("CmdCenter failed to start. Press any key to quit.");
                        Console.Read();
                    }
                }
                catch(Exception ex)
                {
                    NSULog.Exception("NSUWatcher CRASH!!!!", ex.Message);
                }
                NSULog.Close();
            }
            else
            {
                NSULog.Debug (LogTag, "Main(). Starting as service.");
                ServiceBase [] ServicesToRun;
                ServicesToRun = new ServiceBase []
                    {
                        new NSUWatcherService ()
                    };
                ServiceBase.Run (ServicesToRun);
            }
        }
    }
}
