using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSUWatcher;
using NSUWatcher.Db;
using System.IO;

namespace Tests.DB
{
    [TestClass]
    public class NsuDB
    {
        private const string DbName = "testDB.db";
        private const string UserName = "testUserName";

        private NsuWatcherDbContext _dbContext;
        private ILoggerFactory _loggerFactory;

        [TestInitialize]
        public void Init()
        {
            _loggerFactory = LoggerFactory.Create(builder => { });
        }

        [TestCleanup]
        public void Cleanup()
        {
            string dbFile = Path.Combine(NSUWatcherFolders.DataFolder, DbName);
            if (File.Exists(dbFile))
            { 
                File.Delete(dbFile); 
            }
        }

        [TestMethod]
        public void TestDB()
        {
            CreateDbIfNotExists();
            CreateUser();
            DeleteUser();
        }

        public void CreateDbIfNotExists()
        {
            _dbContext = new NsuWatcherDbContext(_loggerFactory, DbName);
            using var db = _dbContext.NsuUsersDbContext;
            Assert.IsNotNull(db);
        }

        public void CreateUser()
        {
            using var userContext = _dbContext.NsuUsersDbContext;
            var result = userContext.CreateUser(UserName, "testUserPassword", true);
            Assert.AreEqual(NSUWatcher.Interfaces.NsuUsers.UserOperationResult.Success, result);
        }

        public void DeleteUser()
        {
            using var userContext = _dbContext.NsuUsersDbContext;
            if (userContext.UserExists(UserName))
            {
                var result = userContext.DeleteUser(UserName);
                Assert.AreEqual(NSUWatcher.Interfaces.NsuUsers.UserOperationResult.Success, result);
            }
        }
    }
}
