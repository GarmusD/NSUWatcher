using Microsoft.Extensions.Logging;
using NSUWatcher.Db.Sqlite;
using NSUWatcher.Interfaces.DB;
using System;

namespace NSUWatcher.Db
{
    public class NsuWatcherDbContext : INsuDbContext
    {
        private readonly ILoggerFactory _loggerFactory;
        public NsuWatcherDbContext(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }
        
        public INsuWatcherUsersDbContext NsuUsersDbContext => new NsuWatcherUsersDbContext(_loggerFactory);
    }
}
