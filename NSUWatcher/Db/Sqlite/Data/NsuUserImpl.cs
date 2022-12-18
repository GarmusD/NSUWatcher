using NSUWatcher.Db.Sqlite.Entities;
using NSUWatcher.Interfaces.NsuUsers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NSUWatcher.Db.Sqlite.Data
{
    internal class NsuUserImpl : INsuUser
    {
        public int Id { get; }
        public string UserName  { get; }
        public string Password  { get; }
        public NsuUserType UserType  { get; }
        public ICollection<string> Permissions  { get; }

        public NsuUserImpl(NsuUser nsuUser)
        {
            Id = nsuUser.UserId;
            UserName = nsuUser.Name; 
            Password = nsuUser.Password;
            UserType = (NsuUserType)nsuUser.UserType;
            Permissions = nsuUser.Permissions.Select(x => x.Permission).ToList();
        }
    }
}
