using MySql.Data.MySqlClient;
using System;

namespace NSUWatcher.DBUtils
{
    public class MySqlDBUtility : IDisposable
	{
		private readonly MySqlConnection _mySql;

		public MySqlDBUtility(string connString)
		{
			_mySql = new MySqlConnection(connString);
			_mySql.Open();
			CheckMySQLTables();
		}

		public void Dispose()
		{
			_mySql?.Dispose();			
		}

		public static string MakeConnectionString(Config config)
		{
			if(config == null) throw new ArgumentNullException(nameof(config), "Config object cannot be null.");
			return
				$"Server={config.DBServerHost};" +
				$"Database={config.DBName};" +
				$"User ID={config.DBUserName};" +
				$"Password={config.DBUserPassword};" +
				"Pooling=false";
		}

		private void CheckMySQLTables ()
		{
			string sql;			
            sql = "CREATE  TABLE IF NOT EXISTS `NSU`.`tsensor_names` (" +
				"`id` INT NOT NULL AUTO_INCREMENT , " +
				"`type` VARCHAR(32) NOT NULL , " +
				"`name` VARCHAR(32) NOT NULL , " +
				"PRIMARY KEY (`id`) );";
            using (var mySqlCmd = GetMySqlCmd())
            {
                mySqlCmd.CommandText = sql;
                mySqlCmd.ExecuteNonQuery();
            }

			sql = "CREATE TABLE IF NOT EXISTS `temperatures` (" +
				"`id` int(11) NOT NULL AUTO_INCREMENT, " +
				"`time` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP, " +
				"`sid` int(11) NOT NULL, " +
				"`value` float NOT NULL, " +
				"PRIMARY KEY (`id`), " +
				"KEY `time_index` (`time`), " +
				"KEY `sid_index` (`sid`) " +
				") ENGINE=InnoDB;";
            using (var mySqlCmd = GetMySqlCmd())
            {
                mySqlCmd.CommandText = sql;
                mySqlCmd.ExecuteNonQuery();
            }

			sql = "CREATE TABLE IF NOT EXISTS `NSU`.`status_names` (" +
				"`id` INT NOT NULL AUTO_INCREMENT, " +
				"`type` VARCHAR(32) NOT NULL, " +
				"`name` VARCHAR(32) NOT NULL, " +
				"PRIMARY KEY (`id`) );";
            using (var mySqlCmd = GetMySqlCmd())
            {
                mySqlCmd.CommandText = sql;
                mySqlCmd.ExecuteNonQuery();
            }

			sql = "CREATE TABLE IF NOT EXISTS `NSU`.`status` (" +
				"`id` int(11) NOT NULL AUTO_INCREMENT, " +
				"`time` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP, " +
				"`sid` int(11) NOT NULL, " +
				"`value` int NOT NULL, " +
				"PRIMARY KEY (`id`), " +
				"KEY `time_index` (`time`), " +
				"KEY `sid_index` (`sid`) " +
				") ENGINE=InnoDB;";
            using (var mySqlCmd = GetMySqlCmd())
            {
                mySqlCmd.CommandText = sql;
                mySqlCmd.ExecuteNonQuery();
            }
		}

		public MySqlCommand GetMySqlCmd ()
		{
            return _mySql?.CreateCommand();
		}
	}
}

