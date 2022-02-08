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
		public NSUUserType UserType	{ get; internal set; }
		public string UserUID { get; internal set; }
		public bool BuiltIn { get; internal set; }
		public NSUUserPermissions Permissions{ get; internal set; }
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
			BuiltIn = usrBuiltIn;
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
            BuiltIn = false;
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
        private const string DefaultAdminUserName = "admin";
        private const string DefaultAdminPassword = "admin";
        //private MySqlCommand mysqlcmd;
        private DBUtility dbutil;

		public NSUUsers(DBUtility value)
		{
            dbutil = value;
            CheckTables();
			CheckBuiltInUser();
		}

        private void CheckTables()
        {
            //var ssql = "DROP TABLE IF EXISTS `NSU`.`users`";
            //using (var mysqlcmd = dbutil.GetMySqlCmd())
            //{
            //    mysqlcmd.CommandText = ssql;
            //    mysqlcmd.ExecuteNonQuery();
            //}

            string sql = "CREATE TABLE IF NOT EXISTS `NSU`.`users` (" +
                "`id` INT NOT NULL AUTO_INCREMENT, " +
                "`usertype` INT NOT NULL, " +
                "`username` CHAR(32) NOT NULL, " +
                "`password` VARCHAR(60) NOT NULL, " +
                "`useruid` CHAR(42), " +
                "`builtin` INT NOT NULL DEFAULT 0, " +
                "PRIMARY KEY (`id`) )ENGINE=InnoDB DEFAULT CHARSET=utf8";
            using (var mysqlcmd = dbutil.GetMySqlCmd())
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
            using (var mysqlcmd = dbutil.GetMySqlCmd())
            {
                mysqlcmd.CommandText = sql;
                mysqlcmd.ExecuteNonQuery();
            }
        }

		public void CheckBuiltInUser()
		{
			if(!BuiltInUserExists())
			{
				CreateBuiltInUser();
			}
		}

		private bool BuiltInUserExists()
		{
            var result = false;
            string sql = "SELECT `id` FROM `users` WHERE `builtin` = 1";
            using (var cmd = dbutil.GetMySqlCmd())
            {
                cmd.CommandText = sql;
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                        result = true;
                }
            }
			return result;
		}

        private string GetUID()
        {
            return Guid.NewGuid().ToString().Replace("{", string.Empty).Replace("}", string.Empty);
        }

		private void CreateBuiltInUser()
		{
            string sql = "INSERT INTO `NSU`.`users`(`usertype`,`username`,`password`,`useruid`,`builtin`) " +
				"VALUES("+Environment.NewLine+
					"@usrType, "+Environment.NewLine+
					"@usrName, "+Environment.NewLine+
					"@usrPassword, "+Environment.NewLine+
					"@usrUID, "+Environment.NewLine+
					"@usrBuiltIn);";			
			var hash = PasswordHasher.ComputeHash(DefaultAdminPassword);
			NSULog.Debug("NSUUserManagement - Hash:", hash);
            using (var mysqlcmd = dbutil.GetMySqlCmd())
            {
                mysqlcmd.CommandText = sql;
                mysqlcmd.Parameters.Clear();
                mysqlcmd.Parameters.Add("@usrType", MySqlDbType.Int32).Value = (int)NSUUserType.Admin;
                mysqlcmd.Parameters.Add("@usrName", MySqlDbType.VarChar).Value = DefaultAdminUserName;
                mysqlcmd.Parameters.Add("@usrPassword", MySqlDbType.VarChar).Value = hash;
                mysqlcmd.Parameters.Add("@usrUID", MySqlDbType.VarChar).Value = GetUID();
                mysqlcmd.Parameters.Add("@usrBuiltIn", MySqlDbType.Int32).Value = 1;
                mysqlcmd.Prepare();

                try
                {
                    mysqlcmd.ExecuteNonQuery();
                }
                catch (MySqlException e)
                {
                    NSULog.Exception("MySql", e.Message);
                }
                catch (Exception ex)
                {
                    NSULog.Exception("CreateBuiltInUser()", ex.Message);
                }
            }
		}

        private NSUUser CreateLoggedInUserHash(NSUUser user)
        {
            string sql = "INSERT INTO `userhashes`(`userid`, `deviceid`, `hash`) VALUES(@userid, @deviceid, @hash)";
            using (var cmd = dbutil.GetMySqlCmd())
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
                catch(Exception ex)
                {
                    NSULog.Exception(LogTag, ex.Message);
                }
            }
            return user;
        }

        private NSUUser UpdateLoggedInUserHash(NSUUser user)
        {
            string sql = "UPDATE `userhashes` SET `hash` = @hash WHERE `userid` = @userid AND `deviceid` = @hash;";
            using (var cmd = dbutil.GetMySqlCmd())
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
            using (var cmd = dbutil.GetMySqlCmd())
            {
                cmd.CommandText = sql;
                cmd.Parameters.Add("@userid", MySqlDbType.Int32).Value = user.DBID;
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        bool chgpsw = false;
                        bool builtin = reader.GetBoolean("builtin");
                        if (builtin)
                        {
                            chgpsw = PasswordHasher.ComputeHash(DefaultAdminPassword).Equals(reader.GetString("password"));
                        }
                        user.Username = reader.GetString("username");
                        user.UserType = (NSUUserType)reader.GetInt32("usertype");
                        user.UserUID = reader.GetString("useruid");
                        user.BuiltIn = builtin;
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
            using (var mysqlcmd = dbutil.GetMySqlCmd())
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
                            if (builtin)
                            {
                                chgpsw = PasswordHasher.ComputeHash(DefaultAdminPassword).Equals(reader.GetString("password"));
                            }
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
            if(user != null)
            {
                user = CreateLoggedInUserHash(user);
            }
			return user;
		}

        public NSUUser LoginUsingHash(string deviceid, string hash)
        {
            NSUUser user = null;
            string sql = "SELECT * FROM `userhashes` WHERE `deviceid`=@deviceid AND `hash`=@hash";
            using (var mysqlcmd = dbutil.GetMySqlCmd())
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
            if(user != null)
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
            string ret = string.Empty;
            string sql = "UPDATE `userhashes` SET `pushid`=@pid WHERE `deviceid`=@did";
            using (var mysqlcmd = dbutil.GetMySqlCmd())
            {
                var tr = mysqlcmd.Connection.BeginTransaction();
                mysqlcmd.CommandText = sql;
                mysqlcmd.Parameters.Add("@pid", MySqlDbType.VarChar, 60).Value = value;
                mysqlcmd.Parameters.Add("@did", MySqlDbType.VarChar).Value = user.DeviceId;
                var res = mysqlcmd.ExecuteNonQuery();

                if(res == 0)//Error - no device exists
                {
                    tr.Rollback();
                    return "device_not_found";
                }
                else if(res == 1)
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

