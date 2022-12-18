using NSUWatcher.Db.Sqlite.Entities;
using System.Data.Entity;

namespace NSUWatcher.Db.Sqlite
{
    [DbConfigurationType(typeof(SqliteConfiguration))]
    public class NsuDbContext : DbContext
    {
        public NsuDbContext(string connectionString) : base(connectionString) { }
        
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            ModelConfiguration.Configure(modelBuilder);
            var initializer = new NsuDbInitializer(modelBuilder);
            Database.SetInitializer(initializer);
        }

        public DbSet<NsuUser> Users { get; set; }
    }
}
