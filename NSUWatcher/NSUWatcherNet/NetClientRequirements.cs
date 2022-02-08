using NSU.Shared.NSUNet;
using System;

namespace NSUWatcher.NSUWatcherNet
{
	public class NetClientRequirements
	{
		public int UserLevelExact
		{
			get{ return userLevelExact; }
			set{ userLevelExact = value; }
		}int userLevelExact = -1;

		public int UserLevelHigherOrEqual
		{
			get{ return userLevelHigherOrEqual; }
			set{ userLevelHigherOrEqual = value; }
		}int userLevelHigherOrEqual = -1;

		public Guid ExcludeClientID
		{
			get{ return excludeClientID; }
			set{ excludeClientID = value; }
		}Guid excludeClientID = Guid.Empty;

		public Guid ClientIDMustEqual
		{
			get{ return clientIDMustEqual; }
			set{ clientIDMustEqual = value; }
		}Guid clientIDMustEqual = Guid.Empty;

		public int LoggedIn
		{
			get{ return loggedIn; }
			set{ loggedIn = value; }
		}int loggedIn = -1;

		public int LoginStageEqual
		{
			get{ return loginStageEqual; }
			set{ loginStageEqual = value; }
		}int loginStageEqual = -1;

		public int IsReady
		{
			get{ return isReady; }
			set{ isReady = value; }
		}int isReady = -1;

		public NetClientAccepts ClientAccepts {
			get{ return netClientAccepts; }
			set{ netClientAccepts = value; }
		}NetClientAccepts netClientAccepts = NetClientAccepts.None;

        public static bool Check(NetClientRequirements req, NetClientData clientData)
		{
			if (req.IsReady != -1)
			{
				if (clientData.IsReady != true)
				{
					return false;
				}
			}

			if (req.LoggedIn != -1)
			{
				if (clientData.LoggedIn != true)
				{
					return false;
				}
			}

			if(req.UserLevelExact != -1)
			{
				if(!(req.UserLevelExact != (int)clientData.ClientType))
				{
					return false;
				}
			}

			if (req.UserLevelHigherOrEqual != -1)
			{
				if (!((int)clientData.ClientType < req.UserLevelHigherOrEqual)) {
                    return false;
                }
            }

            if (!req.ExcludeClientID.Equals(Guid.Empty))
			{
				if(req.ExcludeClientID.Equals(clientData.ClientID))
				{
						return false;
				}
			}

			if (!req.ClientIDMustEqual.Equals (Guid.Empty))
			{
				if (!req.ClientIDMustEqual.Equals (clientData.ClientID))
				{
					return false;
				}
			}

			if (req.ClientAccepts != NetClientAccepts.None)
			{
				if (!clientData.ClientAccepts.HasFlag (req.ClientAccepts))
				{
					return false;
				}
			}

			return true;
		}


		public static NetClientRequirements CreateStandart(bool loggedIn = true, bool isReady = true)
		{
			var req = new NetClientRequirements();
			if(loggedIn)
				req.LoggedIn = 1;
			if(isReady)
				req.IsReady = 1;
			return req;
		}

		public static NetClientRequirements CreateStandart(NetClientAccepts acc, bool loggedIn = true, bool isReady = true)
		{
			var req = new NetClientRequirements();
			if(loggedIn)
				req.LoggedIn = 1;
			if(isReady)
				req.IsReady = 1;
			req.ClientAccepts = acc;
			return req;
		}

        public static NetClientRequirements CreateStandartAcceptInfo(bool loggedIn = true, bool isReady = true)
        {
            var req = CreateStandart ();
            req.ClientAccepts = NetClientAccepts.Info;
            return req;
        }

        public static NetClientRequirements CreateStandartClientOnly(NetClientData data, bool loggedIn = false, bool isReady = false)
		{
			NetClientRequirements req = CreateStandart(loggedIn, isReady);
			req.ClientIDMustEqual = data.ClientID;
			return req;
		}

		public static NetClientRequirements CreateStandartClientExclude(NetClientData data, bool loggedIn = true, bool isReady = true)
		{
			NetClientRequirements req = CreateStandart(loggedIn, isReady);
			req.ExcludeClientID = data.ClientID;
			return req;
		}

		public static NetClientRequirements CreateStandartClientOnlyAccept(NetClientData data, NetClientAccepts acc, bool loggedIn = true, bool isReady = true)
		{
			NetClientRequirements req = CreateStandartClientOnly(data, loggedIn, isReady);
			req.ClientAccepts = acc;
			return req;
		}

		public static NetClientRequirements CreateStandartClientExcludeAccept(NetClientData data, NetClientAccepts acc, bool loggedIn = true, bool isReady = true)
		{
			NetClientRequirements req = CreateStandartClientExclude(data, loggedIn, isReady);
			req.ClientAccepts = acc;
			return req;
		}
	}
}

