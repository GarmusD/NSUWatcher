using System.Collections.Generic;

namespace NSUWatcher.Interfaces.NsuUsers
{
    public enum NsuUserType
    {
        User = 0,
        Admin = 1
    }

    public interface INsuUser
    {
        int Id { get; }
        string UserName { get; }
        string Password { get; }
        NsuUserType UserType { get; }
        ICollection<string> Permissions { get; }
    }
}
