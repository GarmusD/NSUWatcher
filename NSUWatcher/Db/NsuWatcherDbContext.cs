using Microsoft.Extensions.Logging;
using NSUWatcher.Db.Sqlite;
using NSUWatcher.Interfaces.DB;
using System;

namespace NSUWatcher.Db
{
#nullable enable
    public class NsuWatcherDbContext : INsuDbContext
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly string? _dbName;

        public NsuWatcherDbContext(ILoggerFactory loggerFactory, string? dbName = null)
        {
            _loggerFactory = loggerFactory;
            _dbName = dbName;
        }
        
        public INsuWatcherUsersDbContext NsuUsersDbContext => new NsuWatcherUsersDbContext(_loggerFactory, _dbName);
    }
}
