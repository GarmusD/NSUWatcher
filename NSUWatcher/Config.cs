using System;
using System.IO;
using System.Collections.Generic;
using NSU.Shared;

namespace NSUWatcher
{
    public class Config
    {
        private const string LogTag = "Config";

        //Default values
        public string CfgFile { get; set; }
        public string PortName { get; set; }
        public int BaudRate { get; set; }
        public string Bossac { get; set; }
        public string BossacPort { get; set; }
        public bool ArduinoPauseBoot { get; set; }

        public string NSUWritablePath { get; set; }
        public string NSUXMLSnapshotFile { get;  set;}

        //private declarations
        
		private int baudrate;

		public List<string> CmdLines = new List<string>();
		
        static Config instance;
        public static Config Instance(string cfgfile = "/etc/NSU/NSUWatcher.conf")
        {
            instance = instance ?? new Config(cfgfile);
            return instance;
        }

        Config(string cfgfile)
        {
            CfgFile = cfgfile;
            PortName = "/dev/ttymxc3";
            BaudRate = 115200;
            Bossac = "/usr/bin/bossac-udoo";
            BossacPort = "ttymxc3";

            NSUWritablePath = "/var/lib/nsuwatcher";
            NSUXMLSnapshotFile = "snapshot.xml";

            NSULog.Debug(LogTag, $"Main(). Reading config file '{cfgfile}'.");
            if (File.Exists(cfgfile))
            {
                using (StreamReader sr = new StreamReader(cfgfile))
                {
                    string line;
                    while (!sr.EndOfStream)
                    {
                        line = sr.ReadLine().Trim();
                        if (!string.IsNullOrWhiteSpace(line) && !line.StartsWith("#", StringComparison.Ordinal) && line.Contains("="))
                        {
                            string[] pair = line.Split('=');
                            if (pair[0].Trim().ToUpper().Equals("PORT"))
                            {
                                PortName = pair[1];
                            }
                            if (pair[0].Trim().ToUpper().Equals("BOSSAC"))
                            {
                                Bossac = pair[1].Trim();
                            }
                            else if (pair[0].Trim().ToUpper().Equals("BAUDRATE"))
                            {
                                int.TryParse(pair[1], out baudrate);
                            }
                            else if (pair[0].Trim().StartsWith("cmd"))
                            {
                                CmdLines.Add(pair[1]);
                            }
                        }
                    }
                }
            }
            else { }
            
        }




    }
}
