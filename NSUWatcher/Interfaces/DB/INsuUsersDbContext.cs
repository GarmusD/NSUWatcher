using NSUWatcher.Interfaces.NsuUsers;
using System;
using System.Collections.Generic;

namespace NSUWatcher.Interfaces.DB
{
#nullable enable
    public interface INsuWatcherUsersDbContext : IDisposable
    {
        ICollection<INsuUser> GetAllUsers();
        UserOperationResult CreateUser(string userName, string password, bool isAdmin);
        UserOperationResult DeleteUser(string userName);
        bool UserExists(string username);
        INsuUser? GetUser(string username, string password);
        INsuUser? GetUser(int userId, string username, NsuUserType userType);
    }
}
