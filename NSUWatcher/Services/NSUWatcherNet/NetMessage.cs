using Newtonsoft.Json;
using NSU.Shared.NSUNet;
using System;

namespace NSUWatcher.NSUWatcherNet
{
    public class NetMessage : INetMessage
    {
        public DataType DataType { get; }
        public object Data { get; }

        public NetMessage(string message)
        {
            Data = message;
            DataType = DataType.String;
        }

        public NetMessage(byte[] data)
        {
            Data = data;
            DataType = DataType.Bytes;
        }

        public NetMessage(object data)
        {
            Data = JsonConvert.SerializeObject(data);
            DataType = DataType.String;
        }

        public byte[] AsBytes()
        {
            if (Data is byte[] arr)
                return arr;
            throw new InvalidCastException();
        }

        public string AsString()
        {
            if (Data is string str)
                return str;
            throw new InvalidCastException();
        }
    }
}
