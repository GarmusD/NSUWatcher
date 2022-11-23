using NSU.Shared;
using NSU.Shared.DataContracts;
using NSUWatcher.Interfaces.MCUCommands.From;

namespace NSUWatcher.NSUSystem.Data
{
    public struct McuStatusData : IMCUStatusDataContract
    {
        public MCUStatus Status { get; set; }
        public int FreeMem { get; set; }
        public int UpTime { get; set; }
        public bool RebootRequired { get; set; }

        public McuStatusData(ISystemStatus systemStatus)
        {
            Status = systemStatus.CurrentState switch
            {
                JKeys.Syscmd.SystemBooting => MCUStatus.Booting,
                JKeys.Syscmd.ReadyPauseBoot => MCUStatus.BootPauseReady,
                JKeys.Syscmd.SystemBootPaused => MCUStatus.BootPaused,
                JKeys.Syscmd.SystemRunning => MCUStatus.Running,
                _ => MCUStatus.Off
            };
            FreeMem = systemStatus.FreeMem;
            UpTime = systemStatus.UpTime ?? 0;
            RebootRequired = systemStatus.RebootRequired;
        }
    }
}
