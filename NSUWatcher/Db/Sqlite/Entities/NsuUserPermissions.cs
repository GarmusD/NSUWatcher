using SQLite.CodeFirst;
using System.ComponentModel.DataAnnotations.Schema;

namespace NSUWatcher.Db.Sqlite.Entities
{
    public class NsuUserPermissions
    {
        [Autoincrement]
        public int Id { get; set; }        
        public virtual NsuUser User { get; set; }
        public string Permission { get; set; }
    }
}
