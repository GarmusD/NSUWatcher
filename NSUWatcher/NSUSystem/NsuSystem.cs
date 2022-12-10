using System;
using System.Collections.Generic;
using System.Linq;
using NSU.Shared.NSUXMLConfig;
using NSUWatcher.NSUSystem.NSUSystemParts;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;
using NSUWatcher.NSUSystem.Config;
using Microsoft.Extensions.Hosting;
using NSUWatcher.Interfaces;
using NSU.Shared.DataContracts;
using System.ComponentModel;
using NSU.Shared;
using NSU.Shared.Serializer;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace NSUWatcher.NSUSystem
{
    public class NsuSystem : INsuSystem, IHostedService
    {   
        public event EventHandler<PropertyChangedEventArgs> StatusChanged;

        public NsuSysConfig Config => _config;
        public bool IsReady => _isReady;
        public ICmdCenter CmdCenter { get => _iCmdCenter; set => _iCmdCenter = value; }
        public List<NSUSysPartBase> NSUParts => _nsuParts;
        
        public NSUXMLConfig XMLConfig { get; }


        private ICmdCenter _iCmdCenter;
        private readonly ILogger _logger;
        private readonly IHostApplicationLifetime _lifetime;
        private readonly List<NSUSysPartBase> _nsuParts;
        private readonly Syscmd _sysPart;
        private readonly NsuSysConfig _config;
        private readonly NsuSerializer _nsuSerializer;
        private bool _isReady = false;

        

        public NsuSystem(ICmdCenter cmdCenter, IHostApplicationLifetime lifetime, IConfiguration config, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory?.CreateLoggerShort<NsuSystem>() ?? NullLoggerFactory.Instance.CreateLoggerShort<NsuSystem>();
            _logger.LogDebug("Creating NSU System.");
            _nsuSerializer = new NsuSerializer();

            _lifetime = lifetime;

            _config = config.GetSection("NsuSys").Get<NsuSysConfig>();
            ValidateConfig();

            _logger.LogInformation("Creating XMLConfig.");
            XMLConfig = new NSUXMLConfig(loggerFactory);

            _iCmdCenter = cmdCenter;
            _iCmdCenter.SystemMessageReceived += CmdCenter_SystemMessageReceived;
            _iCmdCenter.McuMessageReceived += CmdCenter_McuMessageReceived;
            _iCmdCenter.ExternalCommandReceived += CmdCenter_ExternalCommandReceived;
            _iCmdCenter.ManualCommandReceived += CmdCenter_ManualCommandReceived;

            //SYSCMD
            _sysPart = new Syscmd(this, loggerFactory, _nsuSerializer);

            _nsuParts = new List<NSUSysPartBase>();
            CreateParts(loggerFactory);
            
            _logger.LogTrace("Creating NsuSystem. Done.");
        }

        private void ValidateConfig()
        {
            if (_config.Bossac is null ||
                _config.Bossac.Cmd == "" ||
                _config.Bossac.Port == "")
            throw new ArgumentNullException(nameof(_config), "Configuration is not valid. Required sections 'serial', 'bossac' and 'rebootMcu' must contain valid data.");
        }
        
        private void CmdCenter_SystemMessageReceived(object sender, SystemMessageEventArgs e)
        {
            switch (e.Message)
            {
                case SysMessage.TransportConnected:
                    break;
                case SysMessage.TransportConnectFailed:
                    break;
                case SysMessage.TransportDisconnected:
                    break;
                case SysMessage.McuCrashed:
                    break;
                default:
                    break;
            }
        }

        private void CmdCenter_McuMessageReceived(object sender, McuMessageReceivedEventArgs e)
        {
            try
            {
                _logger.LogTrace($"CmdCenter_McuMessageReceived(): {e.Message.GetType().Name}");
                foreach (var nsuPart in _nsuParts)
                {
                    if (nsuPart.ProcessCommandFromMcu(e.Message))
                        return;
                }
                _logger.LogError($"A processor for IMessageFromMcu '{e.Message.GetType().Name}' not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"CmdCenter_McuMessageReceived(): Exception:");
            }
        }

        private void CmdCenter_ExternalCommandReceived(object sender, ExternalCommandEventArgs e)
        {
            _logger.LogCritical("CmdCenter_ExternalCommandReceived(): NotImplementedException");
            throw new NotImplementedException();
        }
        
        private void CmdCenter_ManualCommandReceived(object sender, ManualCommandEventArgs e)
        {
            _logger.LogCritical("CmdCenter_ManualCommandReceived(): NotImplementedException");
            throw new NotImplementedException();
        }

        internal void OnStatusChanged(INSUSysPartDataContract source, string property)
        {
            _logger.LogDebug($"Emiting StatusChanged event for: {source.GetType().Name} - {property}");
            var evt = StatusChanged;
            evt?.Invoke(source, new PropertyChangedEventArgs(property));
        }

        internal void SetReady(bool isReady)
        {
            _isReady = isReady;
        }

        private void CreateParts(ILoggerFactory loggerFactory)
        {
            NSUParts.Clear();
            NSUParts.Add(_sysPart);
            NSUParts.Add(new TempSensors(this, loggerFactory, _nsuSerializer));
            NSUParts.Add(new Switches(this, loggerFactory, _nsuSerializer));
            NSUParts.Add(new RelayModules(this, loggerFactory, _nsuSerializer));
            NSUParts.Add(new TempTriggers(this, loggerFactory, _nsuSerializer));
            NSUParts.Add(new CircPumps(this, loggerFactory, _nsuSerializer));
            NSUParts.Add(new Collectors(this, loggerFactory, _nsuSerializer));
            NSUParts.Add(new ComfortZones(this, loggerFactory, _nsuSerializer));
            NSUParts.Add(new KTypes(this, loggerFactory, _nsuSerializer));
            NSUParts.Add(new WaterBoilers(this, loggerFactory, _nsuSerializer));
            NSUParts.Add(new WoodBoilers(this, loggerFactory, _nsuSerializer));
            NSUParts.Add(new SystemFans(this, loggerFactory, _nsuSerializer));
            NSUParts.Add(new Usercmd(this, loggerFactory, _nsuSerializer));
            NSUParts.Add(new BinUploader(this, loggerFactory, _nsuSerializer));
            NSUParts.Add(new NSUSystemParts.Console(this, loggerFactory, _nsuSerializer));
        }

        string ReadCmdID(JObject jo)
        {
            var lastCmdID = string.Empty;
            if (jo.Property(JKeys.Generic.CommandID) != null)
            {
                lastCmdID = (string)jo[JKeys.Generic.CommandID];
            }
            return lastCmdID;
        }

        private NSUSysPartBase FindPart(string target)
        {
            return _nsuParts.FirstOrDefault(x => x.SupportedTargets.Contains(target));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogTrace("StartAsync(). Do nothing.");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogTrace("StopAsync(). Do nothing.");
            return Task.CompletedTask;
        }
    }
}

