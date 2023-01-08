namespace NSUWatcher.Interfaces.NsuUsers
{
#nullable enable
    public enum UserOperationResult
    {
        Success,
        Failure,
        UserExists,
        UserNotExists,
    }

    public interface INsuUsers
    {
        UserOperationResult Create(string userName, string password, bool isAdmin);
        UserOperationResult Delete(string userName);
        INsuUser? GetUser(string userName, string password);
        INsuUser? GetUser(int userId, string userName, NsuUserType userType);
        bool UserExists(string userName);
    }
}
