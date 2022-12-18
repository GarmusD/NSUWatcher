using SQLite.CodeFirst;
using System;

namespace NSUWatcher.Db.Sqlite
{
    internal class CustomHistory : IHistory
    {
        public int Id { get; set; }
        public string Hash { get; set; }
        public string Context { get; set; }
        public DateTime CreateDate { get; set; }
    }
}