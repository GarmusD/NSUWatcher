using System;
using System.IO;
using System.Collections.Generic;
using NSU.Shared;

namespace NSUWatcher
{
    public class Config
    {
        //Default values
        public string CfgFile { get; set; }
        public string PortName { get; set; }
        public int BaudRate { get { return baudrate; } set { baudrate = value; } }
        int baudrate;
        public bool ArduinoPauseBoot { get; set; }

        public string NSUXMLConfigFilePath { get; set; }
        public string NSUXMLConfigFile { get;  set;}

        //private declarations
        readonly string LogTag = "Config";
		
		public List<string> CmdLines = new List<string>();
		
        static Config instance;
        public static Config Instance(string cfgfile = "/etc/NSU/NSUWatcher.conf")
        {
            instance = instance ?? new Config(cfgfile);
            return instance;
        }

        Config(string cfgfile)
        {
            PortName = "/dev/ttymxc3";
            baudrate = 115200;
            NSUXMLConfigFilePath = "/var/lib/nsuwatcher";
            NSUXMLConfigFile = "nsuconfig.xml";

            NSULog.Debug(LogTag, "Main(). Reading config file.");
            if (File.Exists(cfgfile))
            {
                using (StreamReader sr = new StreamReader(cfgfile))
                {
                    string line;
                    while (!sr.EndOfStream)
                    {
                        line = sr.ReadLine().Trim();
                        if (!string.IsNullOrWhiteSpace(line) && !line.StartsWith("#", StringComparison.Ordinal))
                        {
                            string[] pair = line.Split('=');
                            if (pair[0].ToUpper().Equals("PORT"))
                            {
                                PortName = pair[1];
                            }
                            else if (pair[0].ToUpper().Equals("BAUDRATE"))
                            {
                                int.TryParse(pair[1], out baudrate);
                            }
                            else if (pair[0].StartsWith("cmd"))
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
