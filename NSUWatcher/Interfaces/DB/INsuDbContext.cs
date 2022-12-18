using System.Data.Entity;

namespace NSUWatcher.Interfaces.DB
{
    internal interface INsuDbContext
    {
        INsuWatcherUsersDbContext NsuUsersDbContext { get; }
    }
}
