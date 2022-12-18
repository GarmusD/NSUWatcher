using JWT.Algorithms;
using JWT.Builder;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NSU.Shared;
using NSU.Shared.DTO.NsuNet;
using NSU.Shared.NSUNet;
using NSUWatcher.Interfaces.NsuUsers;
using System;
using System.Collections.Generic;

namespace NSUWatcher.NSUWatcherNet.NetMessenger
{
#nullable enable
    public class SysMsgProcessor : IMsgProcessor
    {
        private readonly ILogger _logger;
        private readonly INsuUsers _nsuUsers;

        public SysMsgProcessor(INsuUsers nsuUsers, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<SysMsgProcessor>();
            _nsuUsers = nsuUsers;
        }

        public bool ProcessMessage(JObject message, NetClientData clientData, out INetMessage? response)
        {
            string target = (string)message[JKeys.Generic.Target]!;
            string action = (string)message[JKeys.Generic.Action]!;
            string? commandId = (string?)message[JKeys.Generic.Action];

            if (target == JKeys.Syscmd.TargetName)
            {
                if(action == JKeys.SystemAction.Handshake)
                {
                    response = ProcessActionHandshake(commandId);
                    return true;
                }
                else if (action == JKeys.SystemAction.Login)
                {
                    response = ProcessActionLogin(clientData, message, commandId);
                    return true;
                }
            }
            response = null;
            return false;
        }

        private INetMessage ProcessActionHandshake(string? commandId)
        {
            HandshakeResponse response = new HandshakeResponse()
            { 
                Target = JKeys.Syscmd.TargetName,
                Action = JKeys.SystemAction.Handshake,
                Name = "NSUServer",
                Version = $"{Messenger.VERSION_MAJOR}.{Messenger.VERSION_MINOR}",
                Protocol = Messenger.PROTOCOL_VERSION,
                CommandID = commandId
            };
            return new NetMessage(response);
        }

        private INetMessage ProcessActionLogin(NetClientData clientData, JObject message, string? commandId)
        {
            string loginType = message.ContainsKey(JKeys.Generic.Content) ? (string)message[JKeys.Generic.Content]! : string.Empty;
            return loginType switch
            {
                "credentials" => LoginWithCredentials(clientData, (string?)message[JKeys.ActionLogin.UserName], (string?)message[JKeys.ActionLogin.Password], commandId),
                _ => LoginError("Unsupported login type", commandId)
            };
        }

        private INetMessage LoginWithCredentials(NetClientData clientData, string? userName, string? password, string? commandId)
        {
            _logger.LogDebug($"LoginWithCredentials(). UserName: {userName}");
            if (userName == null || password == null) 
                return StandartResponses.Error(JKeys.Syscmd.TargetName, JKeys.SystemAction.Login, JKeys.ErrCodes.Login.InvalidUsrNamePassword, commandId);
            clientData.NsuUser = _nsuUsers.GetUser(userName, password);
            if (clientData.NsuUser == null)
            {
                return StandartResponses.Error(JKeys.Syscmd.TargetName, JKeys.SystemAction.Login, JKeys.ErrCodes.Login.InvalidUsrNamePassword, commandId);
            }
            return null;
        }

        private INetMessage LoginWithToken(string? token, string? commandId)
        {
            return null;
        }

        private INetMessage LoginError(string error, string? commandId)
        {
            return null;
        }

        public static string CreateJWT(INsuUser user, string issuer, string audience, string secret)
        {
            return JwtBuilder.Create()
                .WithAlgorithm(new HMACSHA256Algorithm())
                .WithSecret(secret)
                .IssuedAt(DateTime.UtcNow)
                .Issuer(issuer)
                .Audience(audience)
                .AddClaim("userid", user.Id)
                .AddClaim("username", user.UserName)
                .AddClaim("usertype", (int)user.UserType)
                .Encode();
        }

        public static Dictionary<string, object> DecodeJWT(string token, string issuer, string audience, string secret)
        {
            return JwtBuilder.Create()
                .WithAlgorithm(new HMACSHA256Algorithm())
                .WithSecret(secret)
                .IssuedAt(DateTime.UtcNow)
                .Issuer(issuer)
                .Audience(audience)
                .Decode<Dictionary<string, object>>(token);
        }


        /*
        private void ActionLogin(NetClientData clientData, JObject data)
        {
            NSULog.Debug(LogTag, "ActionLogin.");
            switch ((string)data[JKeys.Generic.Content])
            {
                case JKeys.ActionLogin.LoginWithCredentials:
                    NSULog.Debug(LogTag, "ActionLogin: Logging with Credentials.");
                    clientData.NsuUser = nsusys.Users.Login((string)data[JKeys.ActionLogin.UserName], (string)data[JKeys.ActionLogin.Password]);
                    if (clientData.NsuUser != null)
                    {
                        clientData.LoggedIn = true;
                        if (data.Property(JKeys.UserInfo.UserAccepts) != null)
                        {
                            clientData.ClientAccepts = (NetClientAccepts)(int)data[JKeys.UserInfo.UserAccepts];
                        }
                        JObject jo = new JObject();
                        jo[JKeys.Generic.Target] = JKeys.Syscmd.TargetName;
                        jo[JKeys.Generic.Action] = JKeys.SystemAction.Login;
                        jo[JKeys.Generic.Result] = JKeys.Result.Ok;
                        jo[JKeys.ActionLogin.DeviceID] = clientData.NsuUser.DeviceId;
                        jo[JKeys.ActionLogin.Hash] = clientData.NsuUser.Hash;
                        jo[JKeys.ActionLogin.NeedChangePassword] = clientData.NsuUser.NeedChangePassword;
                        nsusys.SetLastCmdID(clientData, jo);
                        SendToClient(NetClientRequirements.CreateStandartClientOnly(clientData), jo);
                    }
                    else
                    {
                        clientData.LoggedIn = false;
                        clientData.ClientAccepts = NetClientAccepts.None;
                        clientData.IsReady = false;

                        JObject jo = new JObject();
                        jo[JKeys.Generic.Target] = JKeys.Syscmd.TargetName;
                        jo[JKeys.Generic.Action] = JKeys.SystemAction.Login;
                        jo[JKeys.Generic.Result] = JKeys.Result.Error;
                        jo[JKeys.Generic.ErrCode] = JKeys.ErrCodes.Login.InvalidUsrNamePassword;
                        nsusys.SetLastCmdID(clientData, jo);
                        SendToClient(NetClientRequirements.CreateStandartClientOnly(clientData), jo);
                    }
                    return;
                case JKeys.ActionLogin.LoginWithHash:
                    NSULog.Debug(LogTag, "ActionLogin: Logging in with Hash");
                    clientData.NsuUser = nsusys.Users.LoginUsingHash((string)data[JKeys.ActionLogin.DeviceID], (string)data[JKeys.ActionLogin.Hash]);
                    if (clientData.NsuUser != null)
                    {
                        clientData.LoggedIn = true;
                        if (data.Property(JKeys.UserInfo.UserAccepts) != null)
                        {
                            clientData.ClientAccepts = (NetClientAccepts)(int)data[JKeys.UserInfo.UserAccepts];
                        }
                        JObject jo = new JObject();
                        jo[JKeys.Generic.Target] = JKeys.Syscmd.TargetName;
                        jo[JKeys.Generic.Action] = JKeys.SystemAction.Login;
                        jo[JKeys.Generic.Result] = JKeys.Result.Ok;
                        jo[JKeys.ActionLogin.DeviceID] = clientData.NsuUser.DeviceId;
                        jo[JKeys.ActionLogin.Hash] = clientData.NsuUser.Hash;
                        jo[JKeys.ActionLogin.NeedChangePassword] = clientData.NsuUser.NeedChangePassword;
                        nsusys.SetLastCmdID(clientData, jo);
                        SendToClient(NetClientRequirements.CreateStandartClientOnly(clientData), jo);
                    }
                    else
                    {
                        clientData.LoggedIn = false;
                        clientData.ClientAccepts = NetClientAccepts.None;
                        clientData.IsReady = false;

                        JObject jo = new JObject();
                        jo[JKeys.Generic.Target] = JKeys.Syscmd.TargetName;
                        jo[JKeys.Generic.Action] = JKeys.SystemAction.Login;
                        jo[JKeys.Generic.Result] = JKeys.Result.Error;
                        jo[JKeys.Generic.ErrCode] = JKeys.ErrCodes.Login.InvalidHash;
                        nsusys.SetLastCmdID(clientData, jo);
                        SendToClient(NetClientRequirements.CreateStandartClientOnly(clientData), jo);
                    }
                    return;
            }
        }*/
    }
#nullable disable
}
