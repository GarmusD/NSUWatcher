using MySql.Data.MySqlClient;

namespace NSUWatcher.DBUtils
{
    public class DBUtility
	{
		private MySqlConnection mysql;
		private MySqlCommand mysqlcmd;

        private object slock = new object();

		public DBUtility(string connString)
		{
			mysql = new MySqlConnection(connString);
			mysql.Open();
			CheckMySQLTables();
		}

		public void Dispose()
		{
			try
			{
				mysqlcmd.Dispose();			
				mysql.Close();
				mysql.Dispose();			
			}
			finally
			{
				mysqlcmd = null;
				mysql = null;
			}
		}

		private void CheckMySQLTables ()
		{
			string sql;			
            sql = "CREATE  TABLE IF NOT EXISTS `NSU`.`tsensor_names` (" +
				"`id` INT NOT NULL AUTO_INCREMENT , " +
				"`type` VARCHAR(32) NOT NULL , " +
				"`name` VARCHAR(32) NOT NULL , " +
				"PRIMARY KEY (`id`) );";
            using (mysqlcmd = GetMySqlCmd())
            {
                mysqlcmd.CommandText = sql;
                mysqlcmd.ExecuteNonQuery();
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
            using (mysqlcmd = GetMySqlCmd())
            {
                mysqlcmd.CommandText = sql;
                mysqlcmd.ExecuteNonQuery();
            }

			sql = "CREATE TABLE IF NOT EXISTS `NSU`.`status_names` (" +
				"`id` INT NOT NULL AUTO_INCREMENT, " +
				"`type` VARCHAR(32) NOT NULL, " +
				"`name` VARCHAR(32) NOT NULL, " +
				"PRIMARY KEY (`id`) );";
            using (mysqlcmd = GetMySqlCmd())
            {
                mysqlcmd.CommandText = sql;
                mysqlcmd.ExecuteNonQuery();
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
            using (mysqlcmd = GetMySqlCmd())
            {
                mysqlcmd.CommandText = sql;
                mysqlcmd.ExecuteNonQuery();
            }
		}

		public MySqlCommand GetMySqlCmd ()
		{
            return mysql.CreateCommand();
		}
	}
}

