namespace NSUWatcher.Db.LiteDb
{
    public class NsuUserEntity
    {
        public int Id { get; set; }
        public bool Enabled { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public int UserType { get; set; }
        public string[] Permissions { get; set; }
    }
}
