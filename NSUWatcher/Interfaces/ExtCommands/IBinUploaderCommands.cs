namespace NSUWatcher.Interfaces.ExtCommands
{
    public interface IBinUploaderCommands
    {
        IExternalCommand UploadDataChunk(int progress, string data);
    }
}
