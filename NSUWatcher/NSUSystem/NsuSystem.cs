using System;
using System.Collections.Generic;
using System.Linq;
using NSU.Shared.NSUXMLConfig;
using NSUWatcher.NSUSystem.NSUSystemParts;
using Newtonsoft.Json.Linq;
using Serilog;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NSUWatcher.NSUSystem.Config;
using Microsoft.Extensions.Hosting;
using System.Threading;
using NSUWatcher.Interfaces;
using NSU.Shared.DataContracts;
using System.ComponentModel;
using NSU.Shared;
using NSU.Shared.Serializer;

namespace NSUWatcher.NSUSystem
{
    public class NsuSystem : INsuSystem, IHostedService
    {   
        public event EventHandler<PropertyChangedEventArgs>? StatusChanged;

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

        

        public NsuSystem(ICmdCenter cmdCenter, IHostApplicationLifetime lifetime, IConfiguration config, ILogger logger)
        {
            _logger = logger.ForContext<NsuSystem>() ?? throw new ArgumentNullException(nameof(logger), "Instance of logger cannot be null.");
            _logger.Information("Creating NSU System.");
            _nsuSerializer = new NsuSerializer();

            _lifetime = lifetime;

            _config = config.GetSection("NsuSys").Get<NsuSysConfig>();
            ValidateConfig();

            _logger.Information("NsuSys: Creating XMLConfig.");
            XMLConfig = new NSUXMLConfig(logger);

            _iCmdCenter = cmdCenter;
            _iCmdCenter.SystemMessageReceived += CmdCenter_SystemMessageReceived;
            _iCmdCenter.McuMessageReceived += CmdCenter_McuMessageReceived;
            _iCmdCenter.ExternalCommandReceived += CmdCenter_ExternalCommandReceived;
            _iCmdCenter.ManualCommandReceived += CmdCenter_ManualCommandReceived;

            //SYSCMD
            _logger.Information("NsuSys: Creating SysPart.");
            _sysPart = new Syscmd(this, logger, _nsuSerializer);

            _logger.Information("NsuSys: Creating NsuParts.");
            _nsuParts = new List<NSUSysPartBase>();
            CreateParts(logger);
            
            _logger.Information("NsuSys: Done.");
        }
        
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.Information("NsuSystem: StartAsync() called.");
            return Task.CompletedTask;
        }

        // IHostedService
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.Debug("NsuSystem: StopAsync() called.");
            _iCmdCenter.SystemMessageReceived -= CmdCenter_SystemMessageReceived;
            _iCmdCenter.McuMessageReceived -= CmdCenter_McuMessageReceived;
            _iCmdCenter.ExternalCommandReceived -= CmdCenter_ExternalCommandReceived;
            _iCmdCenter.ManualCommandReceived -= CmdCenter_ManualCommandReceived;
            return Task.CompletedTask;
        }

        private void ValidateConfig()
        {
            if (_config.Bossac is null ||
                _config.Bossac.Cmd == "" ||
                _config.Bossac.Port == "")
            throw new ArgumentNullException(nameof(_config), "Configuration is not valid. Required sections 'serial', 'bossac' and 'rebootMcu' must contain valid data.");
        }
        
        private void CmdCenter_SystemMessageReceived(object? sender, SystemMessageEventArgs e)
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
                case SysMessage.TransportAppCrashed:
                    break;
                default:
                    break;
            }
        }

        private void CmdCenter_McuMessageReceived(object? sender, McuMessageReceivedEventArgs e)
        {
            try
            {
                var nsuPart = FindPart(e.Message.Target);
                if (nsuPart == null)
                {
                    _logger.Error($"Target '{e.Message.Target}' not found.");
                    return;
                }
                nsuPart.ProcessCommandFromMcu(e.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"CmdCenter_McuMessageReceived(): Exception:");
            }
        }

        private void CmdCenter_ExternalCommandReceived(object? sender, ExternalCommandEventArgs e)
        {
            throw new NotImplementedException();
        }
        
        private void CmdCenter_ManualCommandReceived(object? sender, ManualCommandEventArgs e)
        {
            throw new NotImplementedException();
        }

        internal void OnStatusChanged(INSUSysPartDataContract source, string property)
        {
            var evt = StatusChanged;
            evt?.Invoke(source, new PropertyChangedEventArgs(property));
        }

        internal void SetReady(bool isReady)
        {
            _isReady = isReady;
        }

        private void CreateParts(ILogger logger)
        {
            NSUParts.Add(_sysPart);
            NSUParts.Add(new TempSensors(this, logger, _nsuSerializer));
            NSUParts.Add(new Switches(this, logger, _nsuSerializer));
            NSUParts.Add(new RelayModules(this, logger, _nsuSerializer));
            NSUParts.Add(new TempTriggers(this, logger, _nsuSerializer));
            NSUParts.Add(new CircPumps(this, logger, _nsuSerializer));
            NSUParts.Add(new Collectors(this, logger, _nsuSerializer));
            NSUParts.Add(new ComfortZones(this, logger, _nsuSerializer));
            NSUParts.Add(new KTypes(this, logger, _nsuSerializer));
            NSUParts.Add(new WaterBoilers(this, logger, _nsuSerializer));
            NSUParts.Add(new WoodBoilers(this, logger, _nsuSerializer));
            NSUParts.Add(new SystemFans(this, logger, _nsuSerializer));
            NSUParts.Add(new Usercmd(this, logger, _nsuSerializer));
            NSUParts.Add(new BinUploader(this, logger, _nsuSerializer));
            NSUParts.Add(new NSUSystemParts.Console(this, logger, _nsuSerializer));
        }

        string ReadCmdID(JObject jo)
        {
            var lastCmdID = string.Empty;
            if (jo.Property(JKeys.Generic.CommandID) != null)
            {
                lastCmdID = (string)jo[JKeys.Generic.CommandID]!;
            }
            return lastCmdID;
        }

        private NSUSysPartBase? FindPart(string target)
        {
            return _nsuParts.FirstOrDefault(x => x.SupportedTargets.Contains(target));
        }
        
    }
}

