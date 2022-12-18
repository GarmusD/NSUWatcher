using SQLite.CodeFirst;
using System.Data.Entity;

namespace NSUWatcher.Db.Sqlite
{
    internal class NsuDbInitializer : SqliteDropCreateDatabaseWhenModelChanges<NsuDbContext>
    {
        public NsuDbInitializer(DbModelBuilder modelBuilder) : base(modelBuilder, typeof(CustomHistory))
        {
        }
    }
}
