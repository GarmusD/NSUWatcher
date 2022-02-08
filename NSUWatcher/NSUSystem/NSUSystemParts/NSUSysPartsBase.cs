using Newtonsoft.Json.Linq;
using NSU.Shared;
using NSUWatcher.NSUWatcherNet;

namespace NSUWatcher.NSUSystem.NSUSystemParts
{
    public abstract class NSUSysPartsBase
    {
        protected NSUSys nsusys;
		public PartsTypes PartType{ get; set; }

		protected NSUSysPartsBase(NSUSys sys, PartsTypes type)
        {
            nsusys = sys;
            PartType = type;
        }

		public abstract string[] RegisterTargets();

        public abstract void Clear();

        //public ArduinoCmdResponse arduinoResponse = null;
        public abstract void ProccessArduinoData(JObject data);//(JObject data);
		public abstract void ProccessNetworkData(NetClientData clientData, JObject data);
        protected void SendToArduino(JObject data, bool waitAck = false)
        {
            if(nsusys.IsReady || PartType == PartsTypes.System)
                nsusys.SendToArduino(data);
        }
        protected void SendToArduino(string data, bool waitAck = false)
        {
            NSULog.Debug("SendToArduino()", string.Format("IsReady: {0}, PartType: {1}", nsusys.IsReady, PartType));
            if (nsusys.IsReady || PartType == PartsTypes.System)
                nsusys.SendToArduino(data);
        }
        protected void SendToClient(NetClientRequirements req,  JObject values, string msg = "")
		{
			nsusys.SendToClient(req, values, msg);
		}
        
        
        
    }
}

