using System;
using System.Globalization;
using System.Threading;
using System.CommandLine;
using System.IO;
using Serilog;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using NSUWatcher.NSUSystem;
using Microsoft.AspNetCore.Hosting;
using NSUWatcher.Logger;
using NSUWatcher.Interfaces;
using NSUWatcher.CommandCenter;
using NSUWatcher.NSUUserManagement;
using NSUWatcher.NSUWatcherNet;

namespace NSUWatcher
{
    class Program
    {
        public const string APP_VERSION = "0.5.1";

        static void Main(string[] args)
        {
            SetupDefaultLang();
            AppDomain.CurrentDomain.ProcessExit += ProcessExitRequested;

            // Not in all cases the NSUWatcher needed to run in Hosted environment.
            // So, loading config separatelly for System.CommandLine.
            // config will be injected to a host.
            var config = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddJsonFile("/etc/nsuwatcher/appsettings.json", false, false)
                .AddJsonFile("/etc/nsuwatcher/appsettings.Development.json", true, false)
                .Build();

            var rootCmd = SetupCommandLine(config);
            rootCmd.Invoke(args);

            //For Serilog
            Log.CloseAndFlush();
        }

        private static void ProcessExitRequested(object? sender, EventArgs e)
        {
            //host shutdown
        }

        private static void SetupDefaultLang()
        {
            //en-150 = English (Europe)
            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-150");

            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }

        /*
         * 
         * Setup System.CommandLine
         * 
         */
        private static RootCommand SetupCommandLine(IConfigurationRoot config)
        {
            Command serviceCmd = SetupRunAsServiceCommand(config);
            Command userCmd = SetupUserAddRemoveCommand(config);

            var rootCommand = new RootCommand()
            {
                serviceCmd,
                userCmd,
            };
            rootCommand.SetHandler(
                (loggerValue) =>
                {
                    ExecCommandRunAsCLI(config, loggerValue);
                },
                LoggerBinder.ConfigureWithConsole(config)
            );
            return rootCommand;
        }

        private static Command SetupRunAsServiceCommand(IConfigurationRoot config)
        {
            var serviceCmd = new Command("service", "Run as service");
            var lockFileOption = new Option<FileInfo>(new string[] { "--lockfile", "-l" }) { IsRequired = true };
            serviceCmd.AddOption(lockFileOption);
            serviceCmd.SetHandler(
                (lockFileValue, loggerValue) =>
                {
                    ExecCommandRunAsService(lockFileValue, config, loggerValue);
                },
                lockFileOption, LoggerBinder.Configure(config)
            );
            return serviceCmd;
        }

        private static Command SetupUserAddRemoveCommand(IConfigurationRoot config)
        {
            Command userCmdAdd = SetupUserAddCommand(config);
            Command userCmdRemove = SetupUserRemoveCommand(config);

            // User command
            var userCmd = new Command("user", "Add or remove user")
            {
                userCmdAdd,
                userCmdRemove
            };
            return userCmd;
        }

        private static Command SetupUserRemoveCommand(IConfigurationRoot config)
        {
            // Remove user command
            var optionRemoveUser = new Option<string>(new string[] { "--user", "-u" }, "User to remove") { IsRequired = true };
            var userCmdRemove = new Command("remove", "Remove user")
            {
                optionRemoveUser
            };
            userCmdRemove.SetHandler(
                (userValue, loggerValue) =>
                {
                    ExecCommandRemoveUser(userValue, loggerValue);
                },
                optionRemoveUser, LoggerBinder.ConfigureWithConsole(config));
            return userCmdRemove;
        }

        private static Command SetupUserAddCommand(IConfigurationRoot config)
        {
            // Add user command
            var optionAddUserName = new Option<string>(new string[] { "--user", "-u" }, "User name") { IsRequired = true };
            var optionAddPassword = new Option<string>(new String[] { "--password", "-p" }, "User password") { IsRequired = true };
            var argumentIsAdmin = new Argument<bool>(name: "admin", description: "Set user as admin", getDefaultValue: () => false);
            var userCmdAdd = new Command("add", "Add user")
            {
                optionAddUserName,
                optionAddPassword,
                argumentIsAdmin
            };
            userCmdAdd.SetHandler(
                (userValue, passwordValue, isAdminValue, loggerValue) =>
                {
                    ExecCommandAddUser(userValue, passwordValue, isAdminValue, loggerValue);
                },
                optionAddUserName, optionAddPassword, argumentIsAdmin, LoggerBinder.ConfigureWithConsole(config)
            );
            return userCmdAdd;
        }

        private static void ExecCommandRunAsService(FileInfo lockFileInfo, IConfigurationRoot config, ILogger logger)
        {
            // Workaround for Linux 14.04 Upstart system. Starting 15 Ubuntu uses systemd
            // and Host can be configured to use Systemd (using Systemd extension).
            // So, a [lock] file with PID inside is needed.

            int pid = System.Diagnostics.Process.GetCurrentProcess().Id;
            File.WriteAllText(lockFileInfo.FullName, pid.ToString());
            // Blocking call
            RunAsService(config, logger);
            // Delete lock file
            if (File.Exists(lockFileInfo.FullName)) File.Delete(lockFileInfo.FullName);
        }

        private static void ExecCommandRunAsCLI(IConfigurationRoot config, ILogger logger)
        {
            RunAsCLI(config, logger);
        }

        private static void ExecCommandAddUser(string user, string password, bool isAdmin, ILogger logger)
        {
            CreateUser(user, password, isAdmin, logger);
        }

        private static void ExecCommandRemoveUser(string user, ILogger logger)
        {
            DeleteUser(user, logger);
        }

        /*
         * 
         * Execute cli commands
         * 
         */
        private static void CreateUser(string userName, string password, bool isAdmin, ILogger logger)
        {
            logger.Information($"Creating user '{userName}'. IsAdmin: {isAdmin}");
            throw new NotImplementedException();
        }

        private static void DeleteUser(string userName, ILogger logger)
        {
            logger.Information($"Deleting user '{userName}':");
            throw new NotImplementedException();
        }

        /*
         * 
         * Execute app in hosted environment - cli or service
         * 
         */
        private static IHostBuilder CreateHostBuilder(IConfigurationRoot configDI, ILogger logger)
        {
            return Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    hostingContext.Configuration = configDI;
                })
                .ConfigureServices((services) =>
                {
                    services.AddSingleton<INsuUsers, NSUUsers>();
                    services.AddHostedService<CmdCenter>();
                    services.AddHostedService<NsuSystem>();
                    services.AddHostedService<WatcherNet>();
                    //services.AddSingleton<MyDbProviders>
                })
                .UseSerilog(logger)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
        }

        private static void RunAsService(IConfigurationRoot config, ILogger logger)
        {
            IHost host = CreateHostBuilder(config, logger).Build();
            host.Run();
        }

        private static void RunAsCLI(IConfigurationRoot config, ILogger logger)
        {
            try
            {
                logger.Information("NSUWatcher is running in console mode.");
                logger.Information("Enter 'q' or Ctrl+C to quit.");

                var builder = CreateHostBuilder(config, logger).UseConsoleLifetime();
                var host = builder.Build();
                logger.Information("IHost builded. Calling StartAsync().");
                host.Start();

                bool done = false;

                var cmdCenter = (ICmdCenter)host.Services.GetRequiredService(typeof(ICmdCenter));
                if(cmdCenter is null)
                {
                    logger.Error("Cannot get an required service \"ICmdCenter\" for manual command execution.");
                    done = true;
                }

                while (!done)
                {
                    string str = Console.ReadLine();
                    if (str.Equals("q", StringComparison.OrdinalIgnoreCase))
                    {
                        done = true;
                    }
                    else
                    {
                        cmdCenter!.ExecManualCommand(str);
                    }
                }
                logger.Information("Quit requested. Terminating...");
                host.Dispose();
            }
            catch (Exception ex)
            {
                logger.Fatal("NSUWatcher CRASH!!!! {ex}", ex);
            }
        }
    }
}
