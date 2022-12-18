using NSUWatcher.Interfaces.NsuUsers;

namespace NSUWatcher.Interfaces.DB
{
#nullable enable
    public interface INsuWatcherUsersDbContext
    {
        bool UserExists(string username);
        INsuUser? GetUser(string username, string password);
        string CreateUser(string userName, string password, bool isAdmin);
        string DeleteUser(string userName);
    }
}
