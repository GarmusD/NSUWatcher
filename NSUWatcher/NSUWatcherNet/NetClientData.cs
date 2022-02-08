using NSU.Shared.NSUNet;
using NSUWatcher.NSUUserManagement;
using System;

namespace NSUWatcher.NSUWatcherNet
{
	public class NetClientData
	{
		public enum NetClientType
		{
			Unknow,
			User,
			Admin
		};

        public string IPAddress = string.Empty;
        public NetClientType ClientType { get; set; } = NetClientType.Unknow;
        public Guid ClientID { get; set; } = Guid.Empty;
        public int ProtocolVersion { get; set; } = 1;
        public bool CompressionSupported { get; set; } = true;
        public int CompressionProtocol { get; set; }
        public bool LoggedIn { get; set; } = false;
        public NSUUser UserData { get; set; } = null;
        public bool IsReady { get; set; } = false;
        public NetClientAccepts ClientAccepts { get; set; } = NetClientAccepts.Alarm | NetClientAccepts.Error | NetClientAccepts.System;
        public string CommandID { get; set; } = string.Empty;
    }
}

