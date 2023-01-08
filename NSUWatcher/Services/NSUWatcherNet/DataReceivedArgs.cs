using System;
//using System.Linq;
using System.Text;
using NSU.Shared.NSUNet;

namespace NSUWatcher.Services.NSUWatcherNet
{
    public partial class NetServer
    {
        /// <summary>
        /// EventArgs for DataReceived event
        /// </summary>
        public class DataReceivedArgs : EventArgs
        {
            public NetClientData ClientData { get; }
            public NetDataType DataType { get; }
            public byte[] Data { get; }
            public int Count { get; }
            public string DataAsString => _strData;

            private readonly string _strData = string.Empty;

            
            public DataReceivedArgs(NetClientData ClientData, NetDataType dt, byte[] buff, int cnt)
            {
                this.ClientData = ClientData;
                DataType = dt;
                Data = buff;
                Count = cnt;
                if (DataType == NetDataType.String)
                {
                    _strData = Encoding.ASCII.GetString(buff, 0, cnt);
                }
            }

            /// <summary>
            /// Only string data supported for now
            /// </summary>
            /// <param name="ClientData"></param>
            /// <param name="buff"></param>
            public DataReceivedArgs(NetClientData ClientData, string buff)
            {
                this.ClientData = ClientData;
                DataType = NetDataType.String;
                Data = Encoding.ASCII.GetBytes(buff);
                Count = Data.Length;
                _strData = buff;
            }
        };


    }
}
