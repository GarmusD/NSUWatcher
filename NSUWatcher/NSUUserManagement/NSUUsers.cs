using System;
using MySql.Data.MySqlClient;
using NSUWatcher.DBUtils;
using NSU.Shared;
using System.Data.SqlClient;
using NSU.Shared.NSUTypes;

namespace NSUWatcher.NSUUserManagement
{
    public enum NSUUserType
    {
        Unknown = 0,
        User = 1,
        Admin = 2
    }

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

    public class NSUUsers
    {
        private const string LogTag = "NSUUser";
        //private const string DefaultAdminUserName = "admin";
        //private const string DefaultAdminPassword = "admin";
        //private MySqlCommand mysqlcmd;
        private readonly MySqlDBUtility _dbutil;

        public NSUUsers(MySqlDBUtility value)
        {
            _dbutil = value;
            CheckTables();
        }

        private void CheckTables()
        {
            string sql = "CREATE TABLE IF NOT EXISTS `NSU`.`users` (" +
                "`id` INT NOT NULL AUTO_INCREMENT, " +
                "`usertype` INT NOT NULL, " +
                "`username` CHAR(32) NOT NULL, " +
                "`password` VARCHAR(60) NOT NULL, " +
                "`useruid` CHAR(42), " +
                "`builtin` INT NOT NULL DEFAULT 0, " +
                "PRIMARY KEY (`id`) )ENGINE=InnoDB DEFAULT CHARSET=utf8";
            using (var mysqlcmd = _dbutil.GetMySqlCmd())
            {
                mysqlcmd.CommandText = sql;
                mysqlcmd.ExecuteNonQuery();
            }

            sql = "CREATE TABLE IF NOT EXISTS `NSU`.`userhashes` (" +
                "`id` INT NOT NULL AUTO_INCREMENT, " +
                "`userid` INT NOT NULL, " +
                "`devicetype` VARCHAR(30), " +
                "`deviceid` CHAR(42), " +
                "`hash` VARCHAR(60) NOT NULL, " +
                "`pushid` VARCHAR(60), " +
                "PRIMARY KEY (`id`) )ENGINE=InnoDB DEFAULT CHARSET=utf8";
            using (var mysqlcmd = _dbutil.GetMySqlCmd())
            {
                mysqlcmd.CommandText = sql;
                mysqlcmd.ExecuteNonQuery();
            }
        }

        private static string GetUID()
        {
            return Guid.NewGuid().ToString().Replace("{", string.Empty).Replace("}", string.Empty);
        }

        public bool UsernameExist(string userName)
        {
            string sql = "SELECT `id` FROM `users` WHERE `username` like @usrName;";
            using (var mysqlcmd = _dbutil.GetMySqlCmd())
            {
                mysqlcmd.CommandText = sql;
                mysqlcmd.Parameters.Clear();
                mysqlcmd.Parameters.Add("@usrName", MySqlDbType.VarChar).Value = userName;
                mysqlcmd.Prepare();

                try
                {
                    var result = mysqlcmd.ExecuteReader();
                    return result.HasRows;
                }
                catch (InvalidOperationException e)
                {
                    NSULog.Exception("MySql", e.Message);
                }
            }
            return false;
        }

        public bool CreateNewUser(string userName, string password, NSUUserType userType = NSUUserType.User)
        {
            string sql = "INSERT INTO `NSU`.`users`(`usertype`,`username`,`password`,`useruid`) " +
                "VALUES(" + Environment.NewLine +
                    "@usrType, " + Environment.NewLine +
                    "@usrName, " + Environment.NewLine +
                    "@usrPassword, " + Environment.NewLine +
                    "@usrUID);";
            var hash = PasswordHasher.ComputeHash(password);
            NSULog.Debug("NSUUserManagement - Hash:", hash);
            using (var mysqlcmd = _dbutil.GetMySqlCmd())
            {
                mysqlcmd.CommandText = sql;
                mysqlcmd.Parameters.Clear();
                mysqlcmd.Parameters.Add("@usrType", MySqlDbType.Int32).Value = (int)userType;
                mysqlcmd.Parameters.Add("@usrName", MySqlDbType.VarChar).Value = userName;
                mysqlcmd.Parameters.Add("@usrPassword", MySqlDbType.VarChar).Value = hash;
                mysqlcmd.Parameters.Add("@usrUID", MySqlDbType.VarChar).Value = GetUID();
                mysqlcmd.Prepare();

                try
                {
                    mysqlcmd.ExecuteNonQuery();
                    return true;
                }
                catch (InvalidOperationException e)
                {
                    NSULog.Exception("MySql", e.Message);
                }
            }
            return false;
        }

        public bool DeleteUser(string userName)
        {
            string sql = "DELETE FROM `users` WHERE `username` like @usrName;";
            using (var mysqlcmd = _dbutil.GetMySqlCmd())
            {
                mysqlcmd.CommandText = sql;
                mysqlcmd.Parameters.Clear();
                mysqlcmd.Parameters.Add("@usrName", MySqlDbType.VarChar).Value = userName;
                mysqlcmd.Prepare();

                try
                {
                    mysqlcmd.ExecuteNonQuery();
                    return true;
                }
                catch (InvalidOperationException e)
                {
                    NSULog.Exception("MySql", e.Message);
                }
            }
            return false;
        }

        private NSUUser CreateLoggedInUserHash(NSUUser user)
        {
            string sql = "INSERT INTO `userhashes`(`userid`, `deviceid`, `hash`) VALUES(@userid, @deviceid, @hash)";
            using (var cmd = _dbutil.GetMySqlCmd())
            {
                string hash = GetUID();
                string deviceid = GetUID();
                cmd.CommandText = sql;
                cmd.Parameters.Add("@userid", MySqlDbType.Int32).Value = user.DBID;
                cmd.Parameters.Add("@deviceid", MySqlDbType.VarChar).Value = deviceid;
                cmd.Parameters.Add("@hash", MySqlDbType.VarChar).Value = hash;
                cmd.Prepare();
                try
                {
                    cmd.ExecuteNonQuery();
                    user.DeviceId = deviceid;
                    user.Hash = hash;
                }
                catch (Exception ex)
                {
                    NSULog.Exception(LogTag, ex.Message);
                }
            }
            return user;
        }

        private NSUUser UpdateLoggedInUserHash(NSUUser user)
        {
            string sql = "UPDATE `userhashes` SET `hash` = @hash WHERE `userid` = @userid AND `deviceid` = @hash;";
            using (var cmd = _dbutil.GetMySqlCmd())
            {
                string hash = GetUID();
                var tr = cmd.Connection.BeginTransaction();
                cmd.CommandText = sql;
                cmd.Parameters.Add("@userid", MySqlDbType.Int32).Value = user.DBID;
                cmd.Parameters.Add("@deviceid", MySqlDbType.VarChar).Value = user.DeviceId;
                cmd.Parameters.Add("@hash", MySqlDbType.VarChar).Value = hash;
                cmd.Prepare();
                try
                {
                    int res = cmd.ExecuteNonQuery();
                    if (res == 1)
                    {
                        user.Hash = hash;
                        tr.Commit();
                    }
                    else
                        tr.Rollback();
                }
                catch (Exception ex)
                {
                    NSULog.Exception(LogTag, ex.Message);
                }
            }
            return user;
        }

        private NSUUser ReadUserInfo(NSUUser user)
        {
            if (user == null)
                return null;
            string sql = "SELECT * FROM `users` WHERE `id`=@userid";
            using (var cmd = _dbutil.GetMySqlCmd())
            {
                cmd.CommandText = sql;
                cmd.Parameters.Add("@userid", MySqlDbType.Int32).Value = user.DBID;
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        bool chgpsw = false;
                        user.Username = reader.GetString("username");
                        user.UserType = (NSUUserType)reader.GetInt32("usertype");
                        user.UserUID = reader.GetString("useruid");
                        user.NeedChangePassword = chgpsw;
                    }
                }
            }
            return user;
        }

        public NSUUser Login(string userName, string userPassword)
        {
            NSUUser user = null;

            NSULog.Debug(LogTag, "Looking for user " + userName);
            NSULog.Debug(LogTag, "and password " + userPassword);


            string sql = "SELECT * FROM `users` WHERE `username`=@un";
            using (var mysqlcmd = _dbutil.GetMySqlCmd())
            {
                mysqlcmd.CommandText = sql;
                mysqlcmd.Parameters.AddWithValue("@un", userName);
                //mysqlcmd.Parameters.AddWithValue("@up", PasswordHasher.ComputeHash(userPassword));
                using (var reader = mysqlcmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var dbpsw = reader.GetString("password");
                        NSULog.Debug(LogTag, "Password from db: " + dbpsw);
                        var pswh = PasswordHasher.ComputeHash(userPassword);
                        if (dbpsw.Equals(pswh, StringComparison.InvariantCulture))
                        {
                            NSULog.Debug(LogTag, "Passwords are equal.");
                            bool chgpsw = false;
                            bool builtin = reader.GetBoolean("builtin");
                            user = new NSUUser(
                                reader.GetInt32("id"),
                                reader.GetString("username"),
                                (NSUUserType)reader.GetInt32("usertype"),
                                reader.GetString("useruid"),
                                builtin,
                                chgpsw
                            );
                            //read permissions
                        }
                        else
                        {
                            NSULog.Debug(LogTag, "Passwords not equal.");
                            NSULog.Debug(LogTag, "PSW1 " + dbpsw);
                            NSULog.Debug(LogTag, "PSW2 " + pswh);
                            var x = string.Compare(dbpsw, pswh);
                            NSULog.Debug(LogTag, "string.Compare result: " + x.ToString());
                            NSULog.Debug(LogTag, "PSW1Hex " + NSU.Shared.NSUUtils.Utils.GetHexString(dbpsw));
                            NSULog.Debug(LogTag, "PSW2Hex " + NSU.Shared.NSUUtils.Utils.GetHexString(pswh));
                        }
                    }
                }
            }
            if (user != null)
            {
                user = CreateLoggedInUserHash(user);
            }
            return user;
        }

        public NSUUser LoginUsingHash(string deviceid, string hash)
        {
            NSUUser user = null;
            string sql = "SELECT * FROM `userhashes` WHERE `deviceid`=@deviceid AND `hash`=@hash";
            using (var mysqlcmd = _dbutil.GetMySqlCmd())
            {
                NSULog.Debug(LogTag, "Selecting user by DeviceID and Hash");
                mysqlcmd.CommandText = sql;
                mysqlcmd.Parameters.AddWithValue("@deviceid", deviceid);
                mysqlcmd.Parameters.AddWithValue("@hash", hash);
                using (var reader = mysqlcmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        NSULog.Debug(LogTag, "User exists. Getting DBID");
                        user = new NSUUser();
                        user.DBID = reader.GetInt32("userid");
                        user.DeviceId = deviceid;
                        user.Hash = hash;
                    }
                }
            }
            if (user != null)
            {
                NSULog.Debug(LogTag, "Updating user hash");
                UpdateLoggedInUserHash(user);
                NSULog.Debug(LogTag, "Reading user info");
                ReadUserInfo(user);
                NSULog.Debug(LogTag, "Reading user permissions");
                ReadPermisions(user);
            }
            return user;
        }

        private NSUUser ReadPermisions(NSUUser user)
        {
            return user;
        }

        public string SetPushID(NSUUser user, string value)
        {
            string sql = "UPDATE `userhashes` SET `pushid`=@pid WHERE `deviceid`=@did";
            using (var mysqlcmd = _dbutil.GetMySqlCmd())
            {
                var tr = mysqlcmd.Connection.BeginTransaction();
                mysqlcmd.CommandText = sql;
                mysqlcmd.Parameters.Add("@pid", MySqlDbType.VarChar, 60).Value = value;
                mysqlcmd.Parameters.Add("@did", MySqlDbType.VarChar).Value = user.DeviceId;
                var res = mysqlcmd.ExecuteNonQuery();

                if (res == 0)//Error - no device exists
                {
                    tr.Rollback();
                    return "device_not_found";
                }
                else if (res == 1)
                {
                    tr.Commit();
                    user.PushID = value;
                }
                else // should never reach here - too many records with same deviceid - somewhere error in the code
                {
                    tr.Rollback();
                    return "too_many_devices";
                }
            }
            return string.Empty;
        }
    }
}

