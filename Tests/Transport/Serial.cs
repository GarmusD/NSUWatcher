using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSUWatcher.Transport.Serial;
using NSUWatcher.Transport.Serial.Config;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Tests.Transport
{
    [TestClass]
    public class Serial
    {
        private const string AppSettingsFile = "/etc/nsuwatcher/appsettings.json";

        private readonly ILogger _loggerNull = null;
        private ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;

        private readonly IConfiguration _configNull = null;
        private IConfiguration _config;

        public Serial()
        {
            _loggerFactory = NullLoggerFactory.Instance;

            Console.WriteLine(Environment.CurrentDirectory);
            Console.WriteLine(Environment.OSVersion.Platform);
            Console.WriteLine(Environment.OSVersion.VersionString);
            Console.WriteLine(Environment.OSVersion.ToString());
        }


        [TestMethod]
        public void ConfigTest()
        {
            _config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.Test.json", false, false)
                .Build();
            SerialConfig serialConfig = _config.GetSection("transport:serial").Get<SerialConfig>();
            Assert.IsNotNull(serialConfig);
        }

        [TestMethod]
        public void CreateSerialTransport()
        {
            LoadConfig(AppSettingsFile);
            SerialTransport transport = new SerialTransport(_config, _loggerFactory);
            Assert.IsNotNull(transport);
        }

        private void LoadConfig(string fileName)
        {
            _config = new ConfigurationBuilder()
                .AddJsonFile(fileName, false, false)
                .Build();
        }
    }
}
