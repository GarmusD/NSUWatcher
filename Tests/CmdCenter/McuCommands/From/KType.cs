using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSUWatcher.Interfaces.MCUCommands.From;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.CmdCenter.McuCommands.From
{
    [TestClass]
    public class KType : FromMcuBase
    {
        [TestMethod]
        public void Snapshot()
        {
            string testJson = "{\"target\":\"ktype\",\"action\":\"snapshot\",\"content\":\"configplus\",\"cfgpos\":0,\"enabled\":1,\"name\":\"dumu_temp\",\"interval\":5,\"value\":71}";
            Check<IKTypeSnapshot>(testJson);
        }

        [TestMethod]
        public void Info()
        { 
            string testJson = "{\"target\":\"ktype\",\"action\":\"info\",\"name\":\"dumu_temp\",\"value\":66}";
            Check<IKTypeInfo>(testJson);
        }
    }
}
