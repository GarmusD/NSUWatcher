using JWT.Algorithms;
using JWT.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NSU.Shared;
using NSU.Shared.DTO.NsuNet;
using NSU.Shared.NSUNet;
using NSU.Shared.Serializer;
using NSUShared.DTO.NsuNet;
using NSUWatcher.Exceptions;
using NSUWatcher.Interfaces;
using NSUWatcher.Interfaces.NsuUsers;
using System;
using System.Collections.Generic;
using System.IO;

namespace NSUWatcher.Services.NSUWatcherNet.NetMessenger.Processors
{
#nullable enable
    public class SysMsgProcessor : IMsgProcessor
    {
        private readonly ILogger _logger;
        private readonly INsuSystem _nsuSystem;
        private readonly INsuUsers _nsuUsers;
        private readonly JwtConfig? _jwtConfig;
        private readonly List<OldToken>? _oldTokens;

        public SysMsgProcessor(INsuSystem nsuSystem, INsuUsers nsuUsers, IConfiguration config, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<SysMsgProcessor>();
            _nsuSystem = nsuSystem;
            _nsuUsers = nsuUsers;
            _jwtConfig = config.GetSection("jwt")?.Get<JwtConfig>();
            if (!ValidateJwtConfig())
                throw new ConfigurationValueMissingException("A valid 'jwt' configuration not found.");
            string oldTokensFile = Path.Combine(NSUWatcherFolders.ConfigFolder, "oldTokens.json");
            if(File.Exists(oldTokensFile))
            {
                _oldTokens = JsonConvert.DeserializeObject<List<OldToken>>(File.ReadAllText(oldTokensFile));
            }
        }

        private bool ValidateJwtConfig()
        {
            return _jwtConfig != null &&
                !string.IsNullOrEmpty(_jwtConfig.Secret) &&
                !string.IsNullOrEmpty(_jwtConfig.Issuer) &&
                !string.IsNullOrEmpty(_jwtConfig.Audience);
        }

        public bool ProcessMessage(JObject message, NetClientData clientData, out INetMessage? response)
        {
            string target = (string)message[JKeys.Generic.Target]!;
            string action = (string)message[JKeys.Generic.Action]!;
            string? commandId = (string?)message[JKeys.Generic.Action];

            if (target == JKeys.Syscmd.TargetName)
            {
                switch (action)
                {
                    case JKeys.SystemAction.Handshake:
                        response = ProcessActionHandshake(commandId);
                        return true;

                    case JKeys.SystemAction.Ping:
                        response = ProcessActionPing(commandId);
                        return true;

                    case JKeys.SystemAction.Login:
                        response = ProcessActionLogin(clientData, message, commandId);
                        return true;

                    case JKeys.Syscmd.Snapshot:
                        response = ProcessActionSnapshot(clientData, message, commandId);
                        return true;

                    default:
                        break;
                }
            }
            response = null;
            return false;
        }

        private INetMessage? ProcessActionHandshake(string? commandId)
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

        private INetMessage? ProcessActionPing(string? commandId) 
        {
            SystemPongResponse response = new SystemPongResponse()
            {
                Target = JKeys.Syscmd.TargetName,
                Action = JKeys.SystemAction.Pong,
            };
            return new NetMessage(response);
        }

        private INetMessage? ProcessActionLogin(NetClientData clientData, JObject message, string? commandId)
        {
            string loginType = message.ContainsKey(JKeys.Generic.Content) ? (string)message[JKeys.Generic.Content]! : string.Empty;
            return loginType switch
            {
                JKeys.ActionLogin.LoginWithCredentials => LoginWithCredentials(clientData, (string?)message[JKeys.ActionLogin.UserName], (string?)message[JKeys.ActionLogin.Password], commandId),
                JKeys.ActionLogin.LoginWithHash => LoginWithToken(clientData, (string?)message[JKeys.ActionLogin.Hash], commandId),
                _ => LoginError("Unsupported login type", commandId)
            };
        }

        private INetMessage? LoginWithCredentials(NetClientData clientData, string? userName, string? password, string? commandId)
        {
            _logger.LogDebug($"LoginWithCredentials(). UserName: {userName}");
            if (userName == null || password == null)
                return LoginError(JKeys.ErrCodes.Login.InvalidUsrNamePassword, commandId);
            clientData.NsuUser = _nsuUsers.GetUser(userName, password);
            if (clientData.NsuUser == null)
            {
                return LoginError(JKeys.ErrCodes.Login.InvalidUsrNamePassword, commandId);
            }
            return new NetMessage(
                new LoginWithCredentialsResponse(
                    CreateJWT(clientData.NsuUser, _jwtConfig!.Issuer, _jwtConfig!.Audience, _jwtConfig!.Secret),
                    commandId)
            );
        }

        private INetMessage? LoginWithToken(NetClientData clientData, string? token, string? commandId)
        {
            _logger.LogDebug($"LoginWithToken(). Token: {token}");
            if (string.IsNullOrEmpty(token)) return LoginError(JKeys.ErrCodes.Login.InvalidHash, commandId);

            // try to decode JWT
            var decodedJwt = DecodeJWT(token!, _jwtConfig!.Issuer, _jwtConfig!.Audience, _jwtConfig!.Secret);
            if (decodedJwt.HasValue && decodedJwt.Value.IsValid(_jwtConfig.Issuer, _jwtConfig.Audience))
            {
                var jwtData = decodedJwt.Value;
                _logger.LogDebug($"Getting user info (token user: {jwtData.Username})");
                clientData.NsuUser = _nsuUsers.GetUser(jwtData.UserId, jwtData.Username, (NsuUserType)jwtData.UserType);
                if (clientData.NsuUser != null)
                {
                    _logger.LogDebug($"Token valid! Sending LoginWithCredentialsResponse.");
                    return new NetMessage( new LoginWithCredentialsResponse(token!, commandId) );
                }
            }
            // jwt is not valid, it's an old token?
            _logger.LogDebug($"Token not valid! Looking for an old token.");
            OldToken? oldToken = _oldTokens?.Find(x => x.Hash == token);
            if(oldToken != null) 
            {
                _logger.LogDebug($"An old token found! ID: {oldToken.Value.Id}. Converting...");
                return ConvertOldToken(clientData, commandId);
            }

            // Somebody tries to hack?
            _logger.LogWarning($"LoginWithToken() failed. IP: {clientData.IPAddress}");
            return LoginError(JKeys.ErrCodes.Login.InvalidHash, commandId);
        }

        private INetMessage? ConvertOldToken(NetClientData clientData, string? commandId)
        {
            if (_jwtConfig!.ConvertFromOld != null && !string.IsNullOrEmpty(_jwtConfig.ConvertFromOld.UserName) && !string.IsNullOrEmpty(_jwtConfig.ConvertFromOld.Password))
            {
                return LoginWithCredentials(clientData, _jwtConfig.ConvertFromOld.UserName, _jwtConfig.ConvertFromOld.Password, commandId);
            }
            return LoginError(JKeys.ErrCodes.Login.InvalidHash, commandId);
        }

        private INetMessage? LoginError(string error, string? commandId)
        {
            return Error.Create(JKeys.Syscmd.TargetName, JKeys.SystemAction.Login, error, commandId);
        }

        private INetMessage? ProcessActionSnapshot(NetClientData clientData, JObject message, string? commandId)
        {
            if (clientData.NsuUser == null)
            {
                return Error.Create(JKeys.Syscmd.TargetName, JKeys.Syscmd.Snapshot, JKeys.ErrCodes.Login.AuthRequired, commandId);
            }
            string? content = (string?)message[JKeys.Generic.Content];
            SnapshotType snapshotType = SnapshotType.Xml;
            if (content != null && content == "json") snapshotType = SnapshotType.Json;
            string snapshot = _nsuSystem.GetSnapshot(snapshotType);
            return new NetMessage(new SnapshotResponse(snapshot, commandId));
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

        public static JwtTokenValues? DecodeJWT(string token, string issuer, string audience, string secret)
        {
            var decodedStr = DecodeJWTtoString(token, issuer, audience, secret);
            if (decodedStr != null)
            {
                var serializer = new NsuSerializer();
                return serializer.Deserialize<JwtTokenValues>(decodedStr);
            }
            return null;
        }

        public static string? DecodeJWTtoString(string token, string issuer, string audience, string secret)
        {
            try
            {
                return JwtBuilder.Create()
                    .WithAlgorithm(new HMACSHA256Algorithm())
                    .WithSecret(secret)
                    .IssuedAt(DateTime.UtcNow)
                    .Issuer(issuer)
                    .Audience(audience)
                    .Decode(token);
            }
            catch { return null; }
        }

        private class JwtConfig
        {
            public string Secret { get; set; } = string.Empty;
            public string Issuer { get; set; } = string.Empty;
            public string Audience { get; set; } = string.Empty;
            public ConvertToNewToken? ConvertFromOld { get; set; }
        }

        private class ConvertToNewToken
        {
            public string UserName { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        public struct JwtTokenValues
        {
            [JsonProperty("iat")]
            public double IssueAt { get; set; }

            [JsonProperty("iss")]
            public string Issuer { get; set; }

            [JsonProperty("aud")]
            public string Audience { get; set; }

            [JsonProperty("userid")]
            public int UserId { get; set; }

            [JsonProperty("username")]
            public string Username { get; set; }

            [JsonProperty("usertype")]
            public int UserType { get; set; }

            public bool IsValid(string issuer, string audience) =>
                Issuer == issuer &&
                Audience == audience &&
                !string.IsNullOrEmpty(Username);
        }

        public struct OldToken
        {
            [JsonProperty("id")]
            public int Id { get; set;}

            [JsonProperty("userid")]
            public int UserId { get; set;}

            [JsonProperty("deviceid")]
            public string DeviceId { get; set; }

            [JsonProperty("hash")]
            public string Hash { get; set; }
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
}
