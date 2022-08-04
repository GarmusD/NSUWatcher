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

        static void Main(string[] args)
        {
            //en-150 = English (Europe)
            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-150");

            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            if (args.Length > 0)
            {
                if((args[0].Equals("help") || args[0].Equals("-h")))
                {
                    PrintUsage();
                    return;
                }
                else if ((args[0].Equals("noservice") || args[0].Equals("-noservice") || args[0].Equals("--noservice")))
                {
                    RunNoService();
                }
                else if (args[0] == "user")
                {
                    if (args[1] == "add")
                    {
                        CreateUser(args);
                    }
                    else if (args[1] == "delete")
                    {
                        DeleteUser(args);
                    }
                }
                else
                {
                    PrintUsage();
                }
            }
            else
            {
                NSULog.Debug(LogTag, "Main(). Starting as service.");
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                    {
                        new NSUWatcherService ()
                    };
                ServiceBase.Run(ServicesToRun);
            }
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("-noservice - run as console app.");
            Console.WriteLine();
            Console.WriteLine("user [add|delete]");
            Console.WriteLine("     add username password [user|admin] (default - user)");
            Console.WriteLine("     delete username");
        }

        private static void CreateUser(string[] args)
        {
            string userName = args?[2];
            string password = args?[3];
            string userType = args?[4] ?? "user";
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            {
                PrintUsage();
                return;
            }

            using (var dbUtility = new DBUtils.MySqlDBUtility(DBUtils.MySqlDBUtility.MakeConnectionString(new Config())))
            {
                var Users = new NSUUserManagement.NSUUsers(dbUtility);
                if (Users.UsernameExist(userName))
                {
                    Console.WriteLine("Error - user exist.");
                    return;
                }

                if (Users.CreateNewUser(userName, password, userType == "admin" ? NSUUserManagement.NSUUserType.Admin : NSUUserManagement.NSUUserType.User))
                {
                    Console.WriteLine($"User '{userName}' created.");
                }
            }
        }

        private static void DeleteUser(string[] args)
        {
            string userName = args?[2];
            if (string.IsNullOrEmpty(userName))
            {
                PrintUsage();
                return;
            }

            using (var dbUtility = new DBUtils.MySqlDBUtility(DBUtils.MySqlDBUtility.MakeConnectionString(new Config())))
            {
                var Users = new NSUUserManagement.NSUUsers(dbUtility);
                if (Users.UsernameExist(userName))
                {
                    if (Users.DeleteUser(userName))
                    {
                        Console.WriteLine($"User '{userName}' deleted.");
                    }
                    else Console.WriteLine("Error: unknown error occured.");
                }
                else
                {
                    Console.WriteLine($"Error: User '{userName}' not exist.");
                }
            }
        }

        private static void RunNoService()
        {
            NSULog.Debug(LogTag, "Main(). Starting NSU System.");
            try
            {
                var nsuSys = new NSUSystem.NSUSys(new Config());
                //var cmdCenter = new CmdCenter();
                if (nsuSys.Start())
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
                            nsuSys.ManualCommand(str);
                        }
                    }
                    NSULog.Debug(LogTag, "Main(). Quit requested.");
                    nsuSys.Stop();
                }
                else
                {
                    Console.WriteLine("NSUSystem failed to start. Press any key to quit.");
                    Console.Read();
                }
            }
            catch (Exception ex)
            {
                NSULog.Exception("NSUWatcher CRASH!!!!", ex.Message);
            }
            NSULog.Close();
        }
    }
}
