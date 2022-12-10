using System.Collections.Generic;
using NSUWatcher.Transport.Serial.Config;

namespace NSUWatcher.NSUSystem.Config
{
    public class NsuSysConfig
    {
        public BossacConfig Bossac { get; set; }
        public List<string> CmdsToExec { get; set; }
        public bool McuPauseBoot { get; set; } = false;
    }
}
