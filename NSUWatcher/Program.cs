using System;
using System.Globalization;
using System.Threading;
using System.CommandLine;
using System.IO;
using Serilog;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using NSUWatcher.NSUSystem;
using NSUWatcher.Interfaces;
using NSUWatcher.CommandCenter;
using NSUWatcher.NSUUserManagement;
using NSUWatcher.Transport.Serial;
using NSUWatcher.Worker;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using NSUWatcher.Transport.TestTransport;
using NSUWatcher.Services.NSUWatcherNet;
using NSUWatcher.Services.InfluxDB;
using NSUWatcher.Interfaces.NsuUsers;
using NSUWatcher.Db;

namespace NSUWatcher
{
    class Program
    {
        public const string APP_VERSION = "0.5.1";
        private const string AppSettingsFile = "/etc/nsuwatcher/appsettings.json";
        private const string AppSettingsDevFile = "/etc/nsuwatcher/appsettings.Development.json";

        static void Main(string[] args)
        {
            if(Environment.OSVersion.Platform != PlatformID.Unix)
                Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");

            SetupDefaultLang();
            AppDomain.CurrentDomain.ProcessExit += ProcessExitRequested;

            // Not in all cases the NSUWatcher is needed to run in Hosted environment.
            // So, loading config separatelly for System.CommandLine.
            // Config will be injected to a host when needed.
            if (!File.Exists(AppSettingsFile))
            {
                Console.WriteLine($"Required settings file '{AppSettingsFile}' could not be found.");
                return;
            }

            var config = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddJsonFile(AppSettingsFile, false, false)
                .AddJsonFile(AppSettingsDevFile, true, false)
                .Build();

            var rootCmd = SetupCommandLine(config);
            rootCmd.Invoke(args);

            //For Serilog
            Log.CloseAndFlush();
        }

        private static void ProcessExitRequested(object sender, EventArgs e)
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
            Command userCmd = SetupUserAddDeleteCommand(config);

            var rootCommand = new RootCommand()
            {
                serviceCmd,
                userCmd,
            };
            rootCommand.SetHandler(
                () =>
                {
                    ExecCommandRunAsCLI(config, NSUWatcher.Logger.LoggerFactory.CreateWithConsole(config));
                }
            );
            return rootCommand;
        }

        private static Command SetupRunAsServiceCommand(IConfigurationRoot config)
        {
            var serviceCmd = new Command("service", "Run as service");
            var lockFileOption = new Option<FileInfo>(new string[] { "--lockfile", "-l" }) { IsRequired = true };
            serviceCmd.AddOption(lockFileOption);
            serviceCmd.SetHandler(
                (lockFileValue) =>
                {
                    ExecCommandRunAsService(lockFileValue, config, NSUWatcher.Logger.LoggerFactory.Create(config));
                },
                lockFileOption
            );
            return serviceCmd;
        }

        private static Command SetupUserAddDeleteCommand(IConfigurationRoot config)
        {
            Command userCmdAdd = SetupUserAddCommand(config);
            Command userCmdRemove = SetupUserDeleteCommand(config);

            // User command
            var userCmd = new Command("user", "Add or remove user")
            {
                userCmdAdd,
                userCmdRemove
            };
            return userCmd;
        }

        private static Command SetupUserDeleteCommand(IConfigurationRoot config)
        {
            // Remove user command
            var argumentUserToDelete = new Argument<string>() { Description = "User to delete" };
            //var optionRemoveUser = new Option<string>(new string[] { "--user", "-u" }, "User to remove") { IsRequired = true };
            var userCmdRemove = new Command("delete", "Delete user")
            {
                argumentUserToDelete,
                //optionRemoveUser
            };
            userCmdRemove.SetHandler(
                (userValue) =>
                {
                    ExecCommandRemoveUser(userValue, config, Logger.LoggerFactory.CreateWithConsole(config, Serilog.Events.LogEventLevel.Information));
                },
                //optionRemoveUser
                argumentUserToDelete
                );
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
                (userValue, passwordValue, isAdminValue) =>
                {
                    ExecCommandAddUser(userValue, passwordValue, isAdminValue, config, Logger.LoggerFactory.CreateWithConsole(config, Serilog.Events.LogEventLevel.Information));
                },
                optionAddUserName, optionAddPassword, argumentIsAdmin
            );
            return userCmdAdd;
        }

        private static void ExecCommandRunAsService(FileInfo lockFileInfo, IConfigurationRoot config, Serilog.ILogger logger)
        {
            // Workaround for Linux 14.04 Upstart system. Starting 15 Ubuntu uses systemd
            // and Host can be configured to use Systemd (using Systemd extension).
            // So, a [lock] file with PID inside is needed.

            try
            {
                int pid = System.Diagnostics.Process.GetCurrentProcess().Id;
                File.WriteAllText(lockFileInfo.FullName, pid.ToString());
                // Blocking call
                RunAsService(config, logger);
            }
            finally
            {
                // Delete lock file
                if (File.Exists(lockFileInfo.FullName)) File.Delete(lockFileInfo.FullName);
            }
        }

        private static void ExecCommandRunAsCLI(IConfigurationRoot config, Serilog.ILogger logger)
        {
            RunAsCLI(config, logger);
        }

        private static void ExecCommandAddUser(string user, string password, bool isAdmin, IConfigurationRoot config, Serilog.ILogger logger)
        {
            CreateUser(user, password, isAdmin, logger);
        }

        private static void ExecCommandRemoveUser(string user, IConfigurationRoot config, Serilog.ILogger logger)
        {
            DeleteUser(user, logger);
        }

        /*
         * 
         * Execute cli commands
         * 
         */
        private static void CreateUser(string userName, string password, bool isAdmin, Serilog.ILogger logger)
        {
            logger.Information($"Creating user '{userName}'. IsAdmin: {isAdmin}");
            NsuWatcherDbContext dbContext = new NsuWatcherDbContext(null);
            var result = dbContext.NsuUsersDbContext.CreateUser(userName, password, isAdmin);
            logger.Information(result.ToString());
        }

        private static void DeleteUser(string userName, Serilog.ILogger logger)
        {
            logger.Information($"Deleting user '{userName}':");
            NsuWatcherDbContext dbContext = new NsuWatcherDbContext(null);
            var result = dbContext.NsuUsersDbContext.DeleteUser(userName);
            logger.Information(result.ToString());
        }

        /*
         * 
         * Execute app in hosted environment - cli or service
         * 
         */
        private static IHostBuilder CreateHostBuilder(IConfigurationRoot configDI, Serilog.ILogger logger)
        {
            return Host.CreateDefaultBuilder()
                .ConfigureLogging((builder) =>
                {
                    builder.Services.RemoveAll<ILoggerProvider>();
                    builder.AddSerilog(logger);
                    builder.SetMinimumLevel(LogLevel.Trace);
                })

                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    logger.Verbose("Injecting configuration...");
                    config.AddConfiguration(configDI);
                })
                .ConfigureServices((services) =>
                {
                    logger.Verbose("Adding singletons and serivces...");
                    services
                        .AddSingleton<NSUUsers>()
                        .AddSingleton<INsuUsers>(s => s.GetRequiredService<NSUUsers>());
                    if (Environment.OSVersion.Platform == PlatformID.Unix)
                    {
                        services
                        .AddSingleton<SerialTransport>()
                        .AddSingleton<IMcuMessageTransport>(s => s.GetRequiredService<SerialTransport>());
                    }
                    else
                    {
                        services
                        .AddSingleton<TestTransport>()
                        .AddSingleton<IMcuMessageTransport>(s => s.GetRequiredService<TestTransport>());
                    }
                    services
                        .AddSingleton<CmdCenter>()
                        .AddSingleton<ICmdCenter>(s => s.GetRequiredService<CmdCenter>())
                        .AddSingleton<NsuSystem>()
                        .AddSingleton<INsuSystem>(s => s.GetRequiredService<NsuSystem>());

                    services
                        .AddHostedService(s => s.GetRequiredService<CmdCenter>())
                        .AddHostedService(s => s.GetRequiredService<NsuSystem>());
                    if (Environment.OSVersion.Platform == PlatformID.Unix)
                    {
                        services
                        .AddHostedService(s => s.GetRequiredService<SerialTransport>());
                    }
                    else
                    {
                        services
                        .AddHostedService(s => s.GetRequiredService<TestTransport>());
                    }
                    services
                        .AddHostedService<InfluxDBService>()
                        .AddHostedService<NSUWorker>()
                        .AddHostedService<WatcherNet>();
                    logger.Verbose("Adding singletons and serivces... Done.");
                })
                //.ConfigureWebHostDefaults(webBuilder =>
                //{
                //    webBuilder.UseStartup<Startup>();
                //})
                ;
        }

        private static void RunAsService(IConfigurationRoot config, Serilog.ILogger logger)
        {
            using IHost host = CreateHostBuilder(config, logger).Build();
            host.Run();
        }

        private static void RunAsCLI(IConfigurationRoot config, Serilog.ILogger logger)
        {
            try
            {
                Logger.LoggerFactory.ConsoleLevelSwitch.MinimumLevel = Serilog.Events.LogEventLevel.Debug;
                logger.Information($"NSUWatcher is running on {Environment.OSVersion.VersionString}.");
                logger.Information("NSUWatcher is running in console mode.");
                logger.Information("Enter 'q' or Ctrl+C to quit.");

                var builder = CreateHostBuilder(config, logger).UseConsoleLifetime();
                using var host = builder.Build();
                host.Start();
                
                bool done = false;

                var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
                var cmdCenter = (ICmdCenter)host.Services.GetRequiredService(typeof(ICmdCenter));
                if (cmdCenter is null)
                {
                    logger.Error("Cannot get an required service \"ICmdCenter\" for manual command execution.");
                    done = true;
                }

                logger.Debug("Entering Console.ReadLine() loop.");
                while (!done)
                {
                    string str = Console.ReadLine();
                    if (str == null || str.Equals("q", StringComparison.OrdinalIgnoreCase))
                    {
                        done = true;
                        lifetime.StopApplication();
                    }
                    else
                    {
                        cmdCenter.ExecManualCommand(str);
                    }
                }
                logger.Information("Quit requested. Terminating...");
                lifetime.StopApplication();
            }
            catch (Exception ex)
            {
                logger.Fatal("NSUWatcher CRASH!!!! {ExMessage}", ex);
            }

            if (System.Diagnostics.Debugger.IsAttached)
            {
                Console.WriteLine("Press enter to exit...");
                Console.ReadLine();
            }
        }
    }
}
