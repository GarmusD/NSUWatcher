using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSUWatcher.Db;
using NSUWatcher.Interfaces.NsuUsers;
using System.Collections.Generic;
using System.IO;

namespace NSUWatcher.NSUUserManagement
{
#nullable enable
    public class NSUUsers : INsuUsers
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;
        private readonly NsuWatcherDbContext _nsuWatcherDbContext;
        // As there will not be many users, they can be stored in memory,
        // reducing the number of accesses to the SD card.
        private readonly List<INsuUser> _allUsers = new List<INsuUser>();
        
            
        public NSUUsers(ILoggerFactory loggerFactory)
        {
            _loggerFactory= loggerFactory;
            _logger = loggerFactory?.CreateLoggerShort<NSUUsers>() ?? NullLoggerFactory.Instance.CreateLoggerShort<NSUUsers>();
            _nsuWatcherDbContext = new NsuWatcherDbContext(_loggerFactory);
            LoadAllUsers();
        }

        public UserOperationResult Create(string userName, string password, bool isAdmin)
        {
            using var usrContext = _nsuWatcherDbContext.NsuUsersDbContext;
            password = PasswordHasher.ComputeHash(password);
            return usrContext.CreateUser(userName, password, isAdmin);
        }

        public UserOperationResult Delete(string userName)
        {
            using var usrContext = _nsuWatcherDbContext.NsuUsersDbContext;
            return usrContext.DeleteUser(userName);
        }

        public INsuUser? GetUser(string username, string password)
        {
            string usrNameLo = username.ToLower();
            password = PasswordHasher.ComputeHash(password);
            return _allUsers.Find(x => x.UserName.ToLower() == usrNameLo && x.Password == password);
        }

        public INsuUser? GetUser(int userId, string username, NsuUserType userType)
        {
            string usrNameLo = username.ToLower();
            return _allUsers.Find(x => x.Id == userId && x.UserName.ToLower() == usrNameLo && x.UserType == userType);
        }

        public bool UserExists(string username)
        {
            string usrNameLo = username.ToLower();
            return _allUsers.Exists(x => x.UserName.ToLower() == usrNameLo);
        }

        private void LoadAllUsers()
        {
            using var usrContext = _nsuWatcherDbContext.NsuUsersDbContext;
            _allUsers.Clear();
            _allUsers.AddRange(usrContext.GetAllUsers());
        }
    }
}
