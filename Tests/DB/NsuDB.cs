using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSUWatcher.Db;

namespace Tests.DB
{
    [TestClass]
    public class NsuDB
    {
        [TestMethod]
        public void TestCreateDbIfNotExists()
        {
            NsuWatcherDbContext dbContext = new NsuWatcherDbContext(null);
            bool result = dbContext.NsuUsersDbContext.UserExists("admin");
            Assert.IsTrue(result);
        }
    }
}
