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
            switch (systemStatus.CurrentState)
            {
                case JKeys.Syscmd.SystemBooting: 
                    Status = MCUStatus.Booting;
                    break;
                case JKeys.Syscmd.ReadyPauseBoot:
                    Status = MCUStatus.BootPauseReady;
                    break;
                case JKeys.Syscmd.SystemBootPaused: 
                    Status = MCUStatus.BootPaused;
                    break;
                case JKeys.Syscmd.SystemRunning:
                    Status = MCUStatus.Running;
                    break;
                default:
                    Status = MCUStatus.Off;
                    break;
            };
            FreeMem = systemStatus.FreeMem;
            UpTime = systemStatus.UpTime ?? 0;
            RebootRequired = systemStatus.RebootRequired;
        }
    }
}
