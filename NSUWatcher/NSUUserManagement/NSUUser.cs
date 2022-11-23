namespace NSUWatcher.NSUUserManagement
{
    public class NSUUser
    {
        public int DBID { get; internal set; }
        public string Username { get; internal set; }
        public NSUUserType UserType { get; internal set; }
        public string UserUID { get; internal set; }
        public NSUUserPermissions Permissions { get; internal set; }
        public bool NeedChangePassword { get; internal set; }
        public string DeviceType { get; internal set; }
        public string DeviceId { get; internal set; }
        public string Hash { get; internal set; }
        public string PushID { get; internal set; }

        public NSUUser(int usrDBID, string usrName, NSUUserType usrType, string usrUID, bool usrBuiltIn, bool chgPsw = false)
        {
            DBID = usrDBID;
            Username = usrName;
            UserType = usrType;
            UserUID = usrUID;
            _ = usrBuiltIn;
            NeedChangePassword = chgPsw;
            DeviceType = string.Empty;
            DeviceId = string.Empty;
            Hash = string.Empty;
            PushID = string.Empty;
            Permissions = new NSUUserPermissions();
        }

        public NSUUser()
        {
            DBID = -1;
            Username = string.Empty;
            UserType = NSUUserType.Unknown;
            UserUID = string.Empty;
            NeedChangePassword = false;
            DeviceType = string.Empty;
            DeviceId = string.Empty;
            Hash = string.Empty;
            PushID = string.Empty;
            Permissions = new NSUUserPermissions();
        }
    }
}

