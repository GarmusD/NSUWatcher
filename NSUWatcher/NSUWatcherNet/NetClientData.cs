using NSU.Shared.NSUNet;
using NSUWatcher.Interfaces.NsuUsers;
using System;

namespace NSUWatcher.NSUWatcherNet
{
#nullable enable
    public class NetClientData : IEquatable<NetClientData>
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
        public bool LoggedIn => NsuUser != null;
        public INsuUser? NsuUser { get; set; } = null;
        public bool IsReady { get; set; } = false;
        public NetClientAccepts ClientAccepts { get; set; } = NetClientAccepts.Alarm | NetClientAccepts.Error | NetClientAccepts.System;
        public string CommandID { get; set; } = string.Empty;

        public override string ToString()
        {
            string userName = NsuUser != null ? NsuUser.UserName : "null";
            return $"User name: '{userName}'. User IP: '{IPAddress}'";
        }

        public bool Equals(NetClientData other)
        {
            if (other == null) return false;
            return ClientID.Equals(other.ClientID);
        }
    }
}

