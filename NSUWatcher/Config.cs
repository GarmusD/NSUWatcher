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

        public string DBServerHost { get; private set; }
        public string DBName { get; private set; }
        public string DBUserName { get; private set; }
        public string DBUserPassword { get; private set; }

        public string NSUWritablePath { get; set; }
        public string NSUXMLSnapshotFile { get;  set;}

        //private declarations
        
		private int baudrate;

		public List<string> CmdLines = new List<string>();
		
        static Config instance = null;
        public static Config Instance(string cfgfile = "/etc/NSU/NSUWatcher.conf")
        {
            instance = instance ?? new Config(cfgfile);
            return instance;
        }

        Config(string cfgfile)
        {
            char[] separators = { '=' };
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
                            string[] pair = line.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                            string parameter = pair[0].Trim().ToLower();
                            string value = pair[1].Trim().ToLower();
                            switch (parameter)
                            {
                                case "port":
                                    PortName = value;
                                    break;
                                case "bossac":
                                    Bossac = value;
                                    break;
                                case "baudrate":
                                    if (int.TryParse(value, out baudrate))
                                    {
                                        BaudRate = baudrate;
                                    }
                                    break;
                                case "dbserver":
                                    DBServerHost = value;
                                    break;
                                case "dbname":
                                    DBName = value;
                                    break;
                                case "dbuser":
                                    DBUserName = value;
                                    break;
                                case "dbpassword":
                                    CmdLines.Add(value);
                                    break;
                            default:
                                if (parameter.StartsWith("cmd"))
                                {
                                    CmdLines.Add(value);
                                }
                                break;
                            }
                        }
                    }
                }
            }
            else { }
            
        }

        private void WriteDefaultsToIniFile()
        {
            string[] cfgLines = new string[]
            {
                @"# NSUWatcher config",
                @"port=/dev/ttymxc3",
                @"baudrate = 115200",
                @"#Commands to exec",
                @"cmd1=relay open 16",
                @"#DB",
                @"dbserver=db_host",
                @"dbname=db_name",
                @"dbuser=db_user_name",
                @"dbpassword=db_user_password"
            };
            File.WriteAllLines(CfgFile, cfgLines);
        }


    }
}
