using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NSUWatcher.Interfaces.MCUCommands;
using System;

namespace NSUWatcher.Services.NSUWatcherNet
{
    public class OldCode
    {
        /*
        internal void SendServerHandshakeToClient(NetClientData clientData)
        {
            try
            {
                if (clientData != null)
                {
                    _logger.Debug("Responding to Handshake request.");
                    JObject jo = new JObject
                    {
                        [JKeys.Generic.Target] = JKeys.Syscmd.TargetName,
                        [JKeys.Generic.Action] = JKeys.SystemAction.Handshake,
                        [JKeys.ServerInfo.Name] = "NSUServer",
                        [JKeys.ServerInfo.Version] = $"{VERSION_MAJOR}.{VERSION_MINOR}",
                        [JKeys.ServerInfo.Protocol] = PROTOCOL_VERSION
                    };
                    jo = SetLastCmdID(clientData, jo);
                    NetServer.SendString(NetClientRequirements.CreateStandartClientOnly(clientData), jo, null, true);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "SendServerHandshakeToClient(): Exception:");
            }
        }

        void NetworkDataReceivedHandler(object sender, NetServer.DataReceivedArgs args)
        {
            //_logger.Debug($"ProcessClientMessage(datatype = {args.datatype}).");
            if (args.datatype == NetDataType.String)
            {
                string cmd = args.GetAsString();
                if (cmd.StartsWith("{"))//JSON string
                {
                    try
                    {
                        var netData = JsonConvert.DeserializeObject(cmd) as JObject;
                        if (netData == null)
                        {
                            _logger.Debug($"JsonConvert.DeserializeObject({cmd}) FAILED.");
                            return;
                        }
                        if (netData.Property(JKeys.Generic.Target) == null)
                        {
                            //error
                            var response = new JObject();
                            response.Add(JKeys.Generic.Result, JKeys.Result.Error);
                            response.Add(JKeys.Generic.ErrCode, "inv_msg_format");
                            response.Add(JKeys.Generic.Message, "Unsupported message format. No target.");
                            if (netData.Property(JKeys.Generic.CommandID) != null)
                                response.Add(JKeys.Generic.CommandID, netData[JKeys.Generic.CommandID]);
                            NetServer.SendString(NetClientRequirements.CreateStandartClientOnly(args.ClientData), response, "");
                            return;
                        }
                        if (args.ClientData.ProtocolVersion == 1)
                        {
                            args.ClientData.ProtocolVersion = 2;
                        }
                        args.ClientData.CommandID = ReadCmdID(netData);

                        _nsupart = FindPart((string)netData[JKeys.Generic.Target]);
                        if (_nsupart != null)
                        {
                            _nsupart.ProccessNetworkData(args.ClientData, netData);
                        }
                        else
                        {
                            _logger.Debug("Part [" + (string)netData[JKeys.Generic.Target] + "] NOT FOUND.");
                        }
                    }
                    catch (Exception exception)
                    {
                        _logger.Error("NSUSys.ClientProcess()", exception.Message);

                        var response = new JObject();
                        response.Add(JKeys.Generic.Result, JKeys.Result.Error);
                        response.Add(JKeys.Generic.ErrCode, "exception");
                        response.Add(JKeys.Generic.Message, exception.Message);
                        NetServer.SendString(NetClientRequirements.CreateStandartClientOnly(args.ClientData), response, "");
                    }
                }
                else
                {
                    _logger.Debug("Unsupported protocol version. Disconnecting client.");
                    args.ClientData.ProtocolVersion = 1;
                    NetServer.SendString(NetClientRequirements.CreateStandartClientOnly(args.ClientData), null, "UNSUPPORTED PROTOCOL VERSION");
                    NetServer.DisconnectClient(args.ClientData);
                }

            }
            else if (args.datatype == NetDataType.Binary)
            {

            }
        }
        */
    }
}
