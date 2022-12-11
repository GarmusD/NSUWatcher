using NSUWatcher.Db.Models;
using System.Data.Entity;

namespace NSUWatcher.Db.Sqlite
{
    public class NsuUsersContext : DbContext
    {
        
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            
        }
        public DbSet<NsuUser> Users { get; set; }
    }
}
