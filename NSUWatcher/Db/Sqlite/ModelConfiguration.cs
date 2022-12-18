using NSUWatcher.Db.Sqlite.Entities;
using System.Data.Entity;

namespace NSUWatcher.Db.Sqlite
{
    public class ModelConfiguration
    {
        public static void Configure(DbModelBuilder modelBuilder)
        {
            ConfigureNsuUserEntity(modelBuilder);
            ConfigureNsuEserPermissionsEntity(modelBuilder);
        }

        private static void ConfigureNsuUserEntity(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NsuUser>()
                .ToTable("User");
        }

        private static void ConfigureNsuEserPermissionsEntity(DbModelBuilder modelBuilder) 
        {
            modelBuilder.Entity<NsuUserPermissions>()
                .ToTable("Permissions")
                .HasRequired(r => r.User)
                .WithRequiredPrincipal()
                .WillCascadeOnDelete(true);
        }
    }
}
