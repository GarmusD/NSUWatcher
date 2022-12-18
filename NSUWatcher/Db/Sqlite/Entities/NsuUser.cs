using SQLite.CodeFirst;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NSUWatcher.Db.Sqlite.Entities
{ 
    public class NsuUser
    {
        [Key]
        [Autoincrement]
        public int UserId { get; set; }
        [MaxLength(50)]
        [Collate(CollationFunction.NoCase)]
        public string Name { get; set; }
        public string Password { get; set; }
        public int UserType { get; set; }
        public virtual ICollection<NsuUserPermissions> Permissions { get; set; }
    }
}
