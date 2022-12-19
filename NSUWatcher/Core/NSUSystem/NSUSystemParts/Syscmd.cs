using System;
using NSUWatcher.Interfaces.MCUCommands;
using NSUWatcher.Interfaces.MCUCommands.From;
using NSU.Shared.NSUSystemPart;
using NSUWatcher.NSUSystem.Data;
using System.Threading.Tasks;
using NSU.Shared.DataContracts;
using NSUWatcher.Interfaces;
using NSU.Shared;
using NSU.Shared.Serializer;
using Microsoft.Extensions.Logging;
using System.Collections;
using NSUWatcher.Interfaces.NsuUsers;

namespace NSUWatcher.NSUSystem.NSUSystemParts
{
    public partial class Syscmd : NSUSysPartBase
    {
        public MCU SystemStatus => _mcu;

        public override string[] SupportedTargets => new string[] { JKeys.Syscmd.TargetName, "SYSCMD:" };

        private readonly MCU _mcu;
        private readonly DaylightSavingTimeHelper _dstHelper;
        private bool _executingSnapshotDone = false;

        public Syscmd(NsuSystem sys, ILoggerFactory loggerFactory, INsuSerializer serializer) : base(sys, loggerFactory, serializer, PartType.System)
        {
            sys.CmdCenter.SystemMessageReceived += CmdCenter_SystemMessageReceived;
            _mcu = new MCU();
            _mcu.PropertyChanged += Mcu_PropertyChanged;

            _dstHelper = new DaylightSavingTimeHelper(loggerFactory.CreateLogger<DaylightSavingTimeHelper>());
            _dstHelper.Start();
            _dstHelper.DaylightTimeChanged += (s, e) =>
            {
                _logger.LogDebug("Syscmd: DaylightTimeChange detected.");
                if (_mcu.Status == MCUStatus.Running)
                    SetMcuClock();
            };
        }

        private void CmdCenter_SystemMessageReceived(object sender, SystemMessageEventArgs e)
        {
            _logger.LogDebug($"Syscmd: CmdCenter_SystemMessageReceived: {e.Message}");
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
                    ProcessMcuCrashed();
                    break;
                default:
                    break;
            }
        }

        public override bool ProcessCommandFromMcu(IMessageFromMcu command)
        {
            switch (command)
            {
                case ISystemStatus systemStatus:
                    ProcessSystemStatusCommand(systemStatus);
                    return true;

                case ISystemSnapshotDone systemSnapshotDone:
                    ProcessSnapshotDone(systemSnapshotDone);
                    return true;

                case ISystemSetTimeResult systemSetTimeResult:
                    return true;

                default:
                    return false;
            }
        }

        private void ProcessSystemStatusCommand(ISystemStatus systemStatus)
        {
            _logger.LogDebug($"Syscmd: ProcessSystemStatusCommand: {systemStatus.CurrentState}");
            McuStatusData data = new McuStatusData(systemStatus);
            _mcu.SetData(data);
        }

        private void ProcessSnapshotDone(ISystemSnapshotDone systemSnapshotDone)
        {
            if (_executingSnapshotDone) return;
            _executingSnapshotDone = true;
            _logger.LogDebug($"Syscmd: ProcessSnapshotDone: Notify parts... Count: {_nsuSys.NSUParts.Count}");
            foreach (var nsuPart in _nsuSys.NSUParts)
            {
                TrySendToNsuPart(nsuPart, systemSnapshotDone);
            }
            _logger.LogDebug("Syscmd: ProcessSnapshotDone: _nsuSys.SetReady(true);");
            _nsuSys.SetReady(true);
            _executingSnapshotDone = false;
        }

        private void TrySendToNsuPart(NSUSysPartBase item, ISystemSnapshotDone systemSnapshotDone)
        {
            try
            {
                item.ProcessCommandFromMcu(systemSnapshotDone);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{item.PartType} Exception on ShapshotDone:");
            }
        }

        private void ProcessMcuCrashed()
        {
            _logger.LogDebug("Syscmd: ProcessMcuCrashed()");
            _nsuSys.SetReady(false);
            _mcu.Status = MCUStatus.Off;
        }

        private void ProcessMsgTransportStarted()
        {
            _logger.LogDebug("Syscmd: ProcessMsgTransportStarted()");
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
            _logger.LogDebug($"Syscmd: Mcu_StatusPropertyChanged: {_mcu.Status}");
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
                    _logger.LogError($"Mcu_StatusPropertyChanged(): Status '{_mcu.Status}' not implemented.");
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

        public override IExternalCommandResult ProccessExternalCommand(IExternalCommand command, INsuUser nsuUser, object context)
        {
            _logger.LogWarning($"ProccessExternalCommand() not implemented for 'Target:{command.Target}' and 'Action:{command.Action}'.");
            return null;
        }

        public void SetMcuClock()
        {
            _logger.LogInformation("Setting time for MCU...");
            DateTime dt = DateTime.Now;
            _nsuSys.CmdCenter.MCUCommands.ToMcu.SystemCommands
                .SetTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, null)
                .Send();
        }

        private void ExecuteUserCommands()
        {
            if (_nsuSys.Config.CmdsToExec == null) return;
            foreach (string item in _nsuSys.Config.CmdsToExec)
            {
                _logger.LogDebug("UserCmd: " + item);
                _nsuSys.CmdCenter.ExecManualCommand(item);
            }
        }

        private void RequestNsuSnapshot()
        {
            _logger.LogDebug("Syscmd: RequestNsuSnapshot(): Clearing parts...");
            //Clear before new snapshot
            foreach (var item in _nsuSys.NSUParts)
            {
                item.Clear();
            }
            _nsuSys.XMLConfig.Clear();

            _logger.LogDebug("Syscmd: RequestNsuSnapshot(): Requesting snapshot...");
            _nsuSys.CmdCenter.MCUCommands.ToMcu.SystemCommands.RequestSnapshot().Send();
        }

        private void PauseBoot()
        {
            _nsuSys.CmdCenter.MCUCommands.ToMcu.SystemCommands.PauseBoot().Send();
        }

        public override void Clear()
        {
        }

        public override IEnumerable GetEnumerator<T>()
        {
            return null;
        }
    }
}
