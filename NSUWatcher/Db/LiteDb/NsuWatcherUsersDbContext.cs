using LiteDB;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSUWatcher.Interfaces.DB;
using NSUWatcher.Interfaces.NsuUsers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NSUWatcher.Db.LiteDb
{
    public class NsuWatcherUsersDbContext : INsuWatcherUsersDbContext
    {
        private readonly ILogger _logger;
        private readonly LiteDatabase _database;
        private readonly ILiteCollection<NsuUserEntity> _userCollection;

        public NsuWatcherUsersDbContext(ILoggerFactory loggerFactory, string dbName)
        {
            _logger = loggerFactory?.CreateLoggerShort<NsuWatcherUsersDbContext>() ?? NullLoggerFactory.Instance.CreateLoggerShort<NsuWatcherUsersDbContext>();
            _database = new LiteDatabase(dbName);
            _userCollection = _database.GetCollection<NsuUserEntity>("users");
        }

        public UserOperationResult CreateUser(string username, string password, bool isAdmin)
        {
            if (UserExists(username))
            {
                return UserOperationResult.UserExists;
            }
            try
            {
                NsuUserEntity user = new NsuUserEntity()
                {
                    Enabled = true,
                    Name = username,
                    Password = password,
                    UserType = isAdmin ? 1 : 0,
                    Permissions = new string[0]
                };
                _userCollection.Insert(user);
                return UserOperationResult.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateUser() Exception: {ex}");
                return UserOperationResult.Failure;
            }
        }

        public UserOperationResult DeleteUser(string username)
        {
            string usrNameLo = username.ToLower();
            var userEntity = _userCollection.FindOne(x => x.Name.ToLower() == usrNameLo);
            if (userEntity != null)
            {
                return _userCollection.Delete(userEntity.Id) ? UserOperationResult.Success : UserOperationResult.Failure;
            }
            return UserOperationResult.UserNotExists;
        }

        public ICollection<INsuUser> GetAllUsers()
        {
            return _userCollection.FindAll().Select(x => new NsuUserImpl(x)).ToList<INsuUser>();
        }

        public INsuUser GetUser(string username, string password)
        {
            string usrNameLo = username.ToLower();
            _logger.LogDebug($"Looking for user: username: {username}, password: {password}");
            var userEntity = _userCollection.FindOne(x => x.Name.ToLower() == usrNameLo && x.Password == password);
            if (userEntity != null)
            {
                _logger.LogDebug("User is found.");
                return new NsuUserImpl(userEntity);
            }
            _logger.LogDebug("User is not found.");
            return null;
        }

        public INsuUser GetUser(int userId, string username, NsuUserType userType)
        {
            string usrNameLo = username.ToLower();
            _logger.LogDebug($"Looking for user: userId: {userId}, username: {username}, userType: {(int)userType}");
            var userEntity = _userCollection.FindOne(x => x.Id == userId && x.Name.ToLower() == usrNameLo && x.UserType == (int)userType);
            if (userEntity != null)
            {
                _logger.LogDebug("User is found.");
                return new NsuUserImpl(userEntity);
            }
            _logger.LogDebug("User is not found.");
            return null;
        }

        public bool UserExists(string username)
        {
            string usrNameLo = username.ToLower();
            return _userCollection.Exists(x => x.Name.ToLower() == usrNameLo);
        }
        
        public void Dispose()
        {
            _database.Dispose();
        }
    }
}
