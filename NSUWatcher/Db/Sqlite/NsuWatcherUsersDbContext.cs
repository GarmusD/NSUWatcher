using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSUWatcher.Db.Sqlite.Data;
using NSUWatcher.Db.Sqlite.Entities;
using NSUWatcher.Interfaces.DB;
using NSUWatcher.Interfaces.NsuUsers;
using System.IO;
using System.Linq;

namespace NSUWatcher.Db.Sqlite
{
#nullable enable
    public class NsuWatcherUsersDbContext : INsuWatcherUsersDbContext
    {
        private readonly ILogger _logger;
        private readonly string _connectionString;
        public NsuWatcherUsersDbContext(ILoggerFactory loggerFactory, string? dbName = null)
        {
            _logger = loggerFactory?.CreateLoggerShort<NsuWatcherUsersDbContext>() ?? NullLoggerFactory.Instance.CreateLoggerShort<NsuWatcherUsersDbContext>();
            string dbFile = Path.Combine(NSUWatcherFolders.DataFolder, dbName ?? "nsuwatcher.db");
            _connectionString = $"Data Source={dbFile}";
        }

        public string CreateUser(string userName, string password, bool isAdmin)
        {
            using var sqlitedb = new NsuDbContext(_connectionString);
            if(UserExists(userName)) 
            {
                return "user exists";
            }
            sqlitedb.Users.Add(new NsuUser() 
            { 
                Name = userName,
                Password = password,
                UserType = isAdmin ? 1 : 0
            });
            int count = sqlitedb.SaveChanges();
            return count > 0 ? "user created" : "user not created";
        }        

        public INsuUser? GetUser(string username, string password)
        {
            using var sqlitedb = new NsuDbContext(_connectionString);
            var nsuUser = sqlitedb.Users.Where(x => x.Name.ToLower() == username.ToLower() && x.Password == password).FirstOrDefault();
            return BindNsuUser(nsuUser);
        }

        public bool UserExists(string username)
        {
            using var sqlitedb = new NsuDbContext(_connectionString);
            return sqlitedb.Users.Any(x => x.Name.ToLower() == username.ToLower());
        }
        
        public string DeleteUser(string userName)
        {
            using var sqlitedb = new NsuDbContext(_connectionString);
            var nsuUser = sqlitedb.Users.Where(x => x.Name.ToLower() == userName.ToLower()).FirstOrDefault();
            if (nsuUser == null)
            {
                return "user not exists";
            }
            sqlitedb.Users.Remove(nsuUser);            
            int count = sqlitedb.SaveChanges();
            return count > 0 ? "user deleted" : "user not deleted";
        }

        private INsuUser? BindNsuUser(NsuUser? nsuUser) 
        {
            if (nsuUser == null) return null;
            return new NsuUserImpl(nsuUser);
        }
    }
}
