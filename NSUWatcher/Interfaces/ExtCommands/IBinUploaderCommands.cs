namespace NSUWatcher.Interfaces.ExtCommands
{
    public interface IBinUploaderCommands
    {
        public IExternalCommand UploadDataChunk(int progress, string data);
    }
}
