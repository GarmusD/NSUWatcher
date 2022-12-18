namespace NSUWatcher.Interfaces.NsuUsers
{
#nullable enable
    public interface INsuUsers
    {
        INsuUser? GetUser(string username, string password);
        bool UserExists(string username);
    }
}
