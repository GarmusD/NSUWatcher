using NSUWatcher.Interfaces.NsuUsers;
using System.Collections.Generic;

namespace NSUWatcher.Db.LiteDb
{
    public class NsuUserImpl : INsuUser
    {
        public int Id { get; }
        public bool Enabled { get; }

        public string UserName { get; }

        public string Password { get; }

        public NsuUserType UserType { get; }

        public ICollection<string> Permissions { get; }

        public NsuUserImpl(NsuUserEntity userEntity)
        {
            Id= userEntity.Id;
            Enabled= userEntity.Enabled;
            UserName = userEntity.Name; 
            Password = userEntity.Password;
            UserType = (NsuUserType)userEntity.UserType;
            Permissions = userEntity.Permissions;
        }
    }
}
