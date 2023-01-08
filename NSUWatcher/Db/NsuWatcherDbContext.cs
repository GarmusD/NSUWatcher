using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSUWatcher.Interfaces.DB;
using System;
using System.IO;

namespace NSUWatcher.Db
{
#nullable enable
    public class NsuWatcherDbContext : INsuDbContext
    {
        private const string DefaultDbName = "nsuwatcher.db";
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;
        private readonly string? _dbName;
        private readonly FileSystemWatcher _fsw;

        public NsuWatcherDbContext(ILoggerFactory loggerFactory, string? dbName = null)
        {
            _loggerFactory = loggerFactory;
            _logger = loggerFactory?.CreateLoggerShort<NsuWatcherDbContext>() ?? NullLoggerFactory.Instance.CreateLoggerShort<NsuWatcherDbContext>();

            _dbName = dbName ?? DefaultDbName;
            bool pathProvided = Path.GetFileName(_dbName) != _dbName;
            if(!pathProvided)
                _dbName = Path.Combine(NSUWatcherFolders.DataFolder, _dbName);
            _dbName = Path.GetFullPath(_dbName);
            var s1 = Path.GetDirectoryName(_dbName);
            var s2 = Path.GetFileName(_dbName);
            var s3 = Path.GetFullPath(_dbName);
            _logger.LogDebug($"Path.GetDirectoryName(_dbName) - {s1}");
            _logger.LogDebug($"Path.GetFileName(_dbName) - {s2}");
            _logger.LogDebug($"Path.GetFullPath(_dbName) - {s3}");

            //_fsw = new FileSystemWatcher(Path.GetDirectoryName(_dbName), Path.GetFileName(_dbName))
            _fsw = new FileSystemWatcher(Path.GetDirectoryName(_dbName))
            {
                NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite
            };
            _fsw.Changed += (s, e) => 
            {
                _logger.LogDebug("Database file Changed.");
            };
        }

        //public INsuWatcherUsersDbContext NsuUsersDbContext => new NsuWatcherUsersDbContext(_loggerFactory, _dbName);
        public INsuWatcherUsersDbContext NsuUsersDbContext => new LiteDb.NsuWatcherUsersDbContext(_loggerFactory, _dbName);
    }
}
