using Microsoft.Extensions.Logging;
using NSUWatcher.Db;
using NSUWatcher.Interfaces.NsuUsers;

namespace NSUWatcher.NSUUserManagement
{
    public class NSUUsers : INsuUsers
    {
        private readonly ILoggerFactory _loggerFactory;

        public NSUUsers(ILoggerFactory loggerFactory)
        {
            _loggerFactory= loggerFactory;
        }

        public INsuUser GetUser(string username, string password)
        {
            return new NsuWatcherDbContext(_loggerFactory)
                .NsuUsersDbContext
                .GetUser(username, PasswordHasher.ComputeHash(password));
        }

        public bool UserExists(string username)
        {
            return new NsuWatcherDbContext(_loggerFactory)
                .NsuUsersDbContext
                .UserExists(username);
        }
    }
}
