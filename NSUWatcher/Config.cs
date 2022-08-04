using System;
using System.IO;
using System.Collections.Generic;
using NSU.Shared;

namespace NSUWatcher
{
    public class Config
    {
        private const string LogTag = "Config";


        public static string CfgFile { get; set; } = "/etc/NSU/NSUWatcher.conf";
        public int NetServerPort { get; private set; }
        public string PortName { get; private set; }
        public int BaudRate { get; private set; }
        public string Bossac { get; private set; }
        public string BossacPort { get; private set; }
        public bool ArduinoPauseBoot { get; set; }

        public string DBServerHost { get; private set; }
        public string DBName { get; private set; }
        public string DBUserName { get; private set; }
        public string DBUserPassword { get; private set; }

        public string NSUWritablePath { get; private set; }
        public string NSUXMLSnapshotFile { get; private set; }

        public List<string> UserCommands => _cmdLines;

        //private declarations

        private readonly List<string> _cmdLines = new List<string>();

        public Config()
        {
            PortName = "/dev/ttymxc3";
            BaudRate = 115200;
            Bossac = "/usr/bin/bossac-udoo";
            BossacPort = "ttymxc3";

            NSUWritablePath = "/var/lib/nsuwatcher";
            NSUXMLSnapshotFile = "snapshot.xml";

            NSULog.Debug(LogTag, $"Main(). Reading config file '{CfgFile}'.");
            if (File.Exists(CfgFile))
            {
                foreach (string line in File.ReadLines(CfgFile))
                {
                    if (IsDataLine(line))
                    {
                        ParseLine(line);
                    }
                }
            }
            else
            {
                NSULog.Error(LogTag, $"CfgFile '{CfgFile}' not exists.");
                throw new FileNotFoundException(CfgFile);
            }

        }

        private static bool IsDataLine(string line)
        {
            return  !string.IsNullOrWhiteSpace(line) && 
                    !line.StartsWith("#", StringComparison.Ordinal) && 
                    line.Contains("=");
        }

        private void ParseLine(string line)
        {
            char[] separators = { '=' };
            string[] pair = line.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            string parameter = pair[0].Trim().ToLower();
            string value = pair[1].Trim().ToLower();
            switch (parameter)
            {
                case "netport":
                    NetServerPort = -1;
                    if (int.TryParse(value, out int netPort))
                        NetServerPort = netPort;
                    break;

                case "port":
                    PortName = value;
                    break;

                case "bossac":
                    Bossac = value;
                    break;

                case "baudrate":
                    if (int.TryParse(value, out int baudrate))
                        BaudRate = baudrate;
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
                    _cmdLines.Add(value);
                    break;

                default:
                    if (parameter.StartsWith("cmd"))
                    {
                        _cmdLines.Add(value);
                    }
                    break;
            }
        }

        public static void WriteConfigTemplateToFile(string filename)
        {
            string[] cfgLines = new string[]
            {
                @"# NSUWatcher config",
                @"# NetServer port",
                @"netport = portnumber",
                @"# Arduino serial",
                @"port=/dev/ttymxc3",
                @"baudrate = 115200",
                @"# Commands to exec",
                @"cmd1=relay open 16",
                @"# DB",
                @"dbserver=db_host",
                @"dbname=db_name",
                @"dbuser=db_user_name",
                @"dbpassword=db_password"
            };
            File.WriteAllLines(filename, cfgLines);
        }


    }
}
