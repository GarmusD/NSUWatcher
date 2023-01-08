using NSU.Shared.NSUNet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NSUWatcher.Services.NSUWatcherNet
{
	public class NetClientRequirements
	{
        public int UserLevelExact { get; set; } = -1;
        public int UserLevelHigherOrEqual { get; set; } = -1;
		public List<Guid> ClientIDMustEqual { get; set;} = new List<Guid>();
        public int LoggedIn { get; set; } = -1;
        public int LoginStageEqual { get; set; } = -1;
        public int IsReady { get; set; } = -1;

        public NetClientAccepts ClientAccepts { get; set; } = NetClientAccepts.None;

        public static bool Check(NetClientRequirements req, NetClientData clientData)
		{
			if (req.IsReady != -1 && clientData.IsReady != true) return false;

			if (req.LoggedIn != -1 && clientData.LoggedIn != true) return false;

			if (req.UserLevelExact != -1 && !(req.UserLevelExact != (int)clientData.ClientType)) return false;

			if (req.UserLevelHigherOrEqual != -1 && !((int)clientData.ClientType < req.UserLevelHigherOrEqual)) return false;

			if (req.ClientIDMustEqual.Any() && !req.ClientIDMustEqual.Contains(clientData.ClientID)) return false;

			if (req.ClientAccepts != NetClientAccepts.None && !clientData.ClientAccepts.HasFlag(req.ClientAccepts)) return false;

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
			req.ClientIDMustEqual.Add(data.ClientID);
			return req;
		}

        public static NetClientRequirements CreateStandartClientsOnly(Guid[] clientIDs, bool loggedIn = false, bool isReady = false)
        {
            NetClientRequirements req = CreateStandart(loggedIn, isReady);
            req.ClientIDMustEqual.AddRange(clientIDs);
            return req;
        }

		public static NetClientRequirements CreateStandartClientOnlyAccept(NetClientData data, NetClientAccepts acc, bool loggedIn = true, bool isReady = true)
		{
			NetClientRequirements req = CreateStandartClientOnly(data, loggedIn, isReady);
			req.ClientAccepts = acc;
			return req;
		}
	}
}

