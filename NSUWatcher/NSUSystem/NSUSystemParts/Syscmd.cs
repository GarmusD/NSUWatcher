using System;
using Serilog;
using NSUWatcher.Interfaces.MCUCommands;
using NSUWatcher.Interfaces.MCUCommands.From;
using NSU.Shared.NSUSystemPart;
using NSUWatcher.NSUSystem.Data;
using System.Threading.Tasks;
using NSU.Shared.DataContracts;
using NSUWatcher.Interfaces;
using NSU.Shared;
using NSU.Shared.Serializer;

namespace NSUWatcher.NSUSystem.NSUSystemParts
{
    public partial class Syscmd : NSUSysPartBase
    {
        public MCU SystemStatus => _mcu;

        public override string[] SupportedTargets => new string[] { JKeys.Syscmd.TargetName, "SYSCMD:" };

        private readonly MCU _mcu;
        private readonly DaylightSavingTimeHelper _dstHelper;

        public Syscmd(NsuSystem sys, ILogger logger, INsuSerializer serializer) : base(sys, logger, serializer, PartType.System)
        {
            sys.CmdCenter.SystemMessageReceived += CmdCenter_SystemMessageReceived;
            _mcu = new MCU();
            _mcu.PropertyChanged += Mcu_PropertyChanged;

            _dstHelper = new DaylightSavingTimeHelper(logger);
            _dstHelper.Start();
            _dstHelper.DaylightTimeChanged += (s, e) =>
            {
                if (_mcu.Status == MCUStatus.Running)
                    SetMcuClock();
            };
        }

        private void CmdCenter_SystemMessageReceived(object? sender, Interfaces.SystemMessageEventArgs e)
        {
            switch (e.Message)
            {
                case Interfaces.SysMessage.TransportConnected:
                    ProcessMsgTransportStarted();
                    break;
                case Interfaces.SysMessage.TransportConnectFailed:
                case Interfaces.SysMessage.TransportDisconnected:
                    // Hold last values or clear?
                    break;
                case Interfaces.SysMessage.McuCrashed:
                    
                    break;
                default:
                    break;
            }
        }

        public override void ProcessCommandFromMcu(IMessageFromMcu command)
        {
            switch (command)
            {
                case ISystemStatus systemStatus:
                    ProcessSystemStatusCommand(systemStatus);
                    break;

                case ISystemSnapshotDone systemSnapshotDone:
                    ProcessSnapshotDone(systemSnapshotDone);
                    break;

                default:
                    _logger.Warning($"Not implemented command from mcu: {command.GetType().FullName}");
                    break;
            }
        }

        private void ProcessSystemStatusCommand(ISystemStatus systemStatus)
        {
            McuStatusData data = new McuStatusData(systemStatus);
            _mcu.SetData(data);
        }

        private void ProcessSnapshotDone(ISystemSnapshotDone systemSnapshotDone)
        {
            foreach (var nsuPart in _nsuSys.NSUParts)
            {
                TrySendToNsuPart(nsuPart, systemSnapshotDone);
            }
            _nsuSys.SetReady(true);
        }

        private void TrySendToNsuPart(NSUSysPartBase item, ISystemSnapshotDone systemSnapshotDone)
        {
            try
            {
                item.ProcessCommandFromMcu(systemSnapshotDone);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"{item.PartType} Exception on ShapshotDone:");
            }
        }

        private void ProcessMcuCrashed()
        {
            _logger.Debug("ProcessMcuCrashed()");
            _nsuSys.SetReady(false);
            _mcu.Status = MCUStatus.Off;
        }

        private void ProcessMsgTransportStarted()
        {
            _logger.Debug("ProcessMsgTransportStarted()");
            // Workaround for the very first command - on mcu side it comes damaged
            _nsuSys.CmdCenter.MCUCommands.ToMcu.SystemCommands.EmptyCommand().Send();
            _nsuSys.CmdCenter.MCUCommands.ToMcu.SystemCommands.GetMcuStatus().Send();
        }

        private void Mcu_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_mcu.Status))
                Mcu_StatusPropertyChanged();
        }

        private void Mcu_StatusPropertyChanged()
        {
            switch (_mcu.Status)
            {
                case MCUStatus.Off:
                    break;

                case MCUStatus.Booting:
                    break;

                case MCUStatus.BootPauseReady:
                    OnMcuStatus_BootPauseReady();
                    break;

                case MCUStatus.BootPaused:
                    break;

                case MCUStatus.Running:
                    OnMcuStatus_Running();
                    break;

                default:
                    _logger.Error($"Mcu_StatusPropertyChanged(): Status '{_mcu.Status}' not implemented.");
                    return;
            }
        }

        private void OnMcuStatus_BootPauseReady()
        {
            if (_nsuSys.Config.McuPauseBoot)
            {
                //Clear pause boot flag for next boot
                _nsuSys.Config.McuPauseBoot = false;
                PauseBoot();
            }
        }

        private void OnMcuStatus_Running()
        {
            _ = Task.Run(async () =>
            {
                SetMcuClock();
                await Task.Delay(2000);
                ExecuteUserCommands();
                await Task.Delay(1500);
                RequestNsuSnapshot();
            });
        }

        public override IExternalCommandResult? ProccessExternalCommand(IExternalCommand command, INsuUser nsuUser, object context)
        {
            _logger.Warning($"ProccessExternalCommand() not implemented for 'Target:{command.Target}' and 'Action:{command.Action}'.");
            return null;
        }

        public void SetMcuClock()
        {
            _logger.Information("Setting time for MCU...");
            DateTime dt = DateTime.Now;
            _nsuSys.CmdCenter.MCUCommands.ToMcu.SystemCommands
                .SetTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second)
                .Send();
        }

        private void ExecuteUserCommands()
        {
            if (_nsuSys.Config.CmdsToExec == null) return;
            foreach (string item in _nsuSys.Config.CmdsToExec)
            {
                _logger.Debug("UserCmd: " + item);
                _nsuSys.CmdCenter.ExecManualCommand(item);
            }
        }

        private void RequestNsuSnapshot()
        {
            //Clear before new snapshot
            foreach (var item in _nsuSys.NSUParts)
            {
                item.Clear();
            }
            _nsuSys.XMLConfig.Clear();

            _nsuSys.CmdCenter.MCUCommands.ToMcu.SystemCommands.RequestSnapshot().Send();
        }

        private void PauseBoot()
        {
            _nsuSys.CmdCenter.MCUCommands.ToMcu.SystemCommands.PauseBoot().Send();
        }

        public override void Clear()
        {
            //
            _mcu.Status = MCUStatus.Off;
        }


    }
}
