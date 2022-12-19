using NSU.Shared;
using NSU.Shared.DTO.ExtCommandContent;
using NSU.Shared.Serializer;
using NSUWatcher.Interfaces;
using NSUWatcher.Interfaces.ExtCommands;

namespace NSUWatcher.CommandCenter.ExtCommands
{
    public class BinUploaderCommands : IBinUploaderCommands
    {
        private readonly INsuSerializer _serializer;

        public BinUploaderCommands(INsuSerializer serializer)
        {
            _serializer = serializer;
        }

        public IExternalCommand UploadDataChunk(int progress, string data)
        {
            return new ExternalCommand()
            {
                Target = JKeys.BinUploader.TargetName,
                Action = JKeys.BinUploader.Data,
                Content = _serializer.Serialize(new BinUploadDataContent(progress, data))
            };
        }
    }
}
