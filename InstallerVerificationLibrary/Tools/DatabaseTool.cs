namespace InstallerVerificationLibrary.Tools
{
    using System;
    using System.Collections.ObjectModel;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.Linq;
    using InstallerVerificationLibrary.Logging;

    public static class DatabaseTool
    {
        private const string LocalHost = "(local)";
        private const string InitialCatalog = "master";

        public static void DropDatabase(string database)
        {
            DropDatabase(database, null, null);
        }

        public static void DropDatabase(string database, string server, string instanceName)
        {
            if (!DatabaseExist(database, server, instanceName))
            {
                Log.WriteInfo(
                    "Database '" + database + "' server '" + server + "' instance '" + instanceName + "' was not found.",
                    "DropDatabase");
                return;
            }

            Log.WriteInfo(
                "Droping database '" + database + "' server '" + server + "' instance '" + instanceName + "'",
                "DropDatabase");
            using (var con = new SqlConnection(BuildConnectionsString(server, instanceName)))
            {
                try
                {
                    con.Open();
                }
                catch (SqlException)
                {
                    Log.WriteError(
                        "Could not open connection to databaseserver '" + server + "' instance '" + instanceName + "'",
                        "DropDatabase");
                    return;
                }

                var dropCommand = string.Format(CultureInfo.CurrentUICulture, "Drop Database [{0}]", database);
                var singelUserCommand = string.Format(CultureInfo.CurrentUICulture,
                    "ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE", database);
                var dropCom = new SqlCommand(dropCommand);
                var singelUserCom = new SqlCommand(singelUserCommand);
                dropCom.Connection = con;
                singelUserCom.Connection = con;
                try
                {
                    singelUserCom.ExecuteNonQuery();
                    dropCom.ExecuteNonQuery();
                }
                catch (SqlException e)
                {
                    if (!e.Message.Contains("does not exist"))
                    {
                        throw;
                    }
                }
                finally
                {
                    dropCom.Dispose();
                    singelUserCom.Dispose();
                }

                if (DatabaseExist(database, server, instanceName))
                {
                    Log.WriteError(
                        "Database '" + database + "' server '" + server + "' instance '" + instanceName +
                        "' still exist", "DropDatabase");
                }
            }
        }

        public static bool DatabaseExist(string database, string server, string instanceName)
        {
            return GetDatabases(server, instanceName).Any(db => string.Compare(database, db, StringComparison.OrdinalIgnoreCase) == 0);
        }

        public static Collection<string> GetDatabases(string server, string instanceName)
        {
            var databases = new Collection<string>();
            using (var con = new SqlConnection(BuildConnectionsString(server, instanceName)))
            {
                try
                {
                    con.Open();
                }
                catch (SqlException)
                {
                    Log.WriteError("Could not open connection to database server", "GetDatabases");
                    return databases;
                }

                using (var command = new SqlCommand("Select Name From sys.databases"))
                {
                    command.Connection = con;
                    try
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                databases.Add(reader.GetString(0));
                            }
                        }
                    }
                    catch (SqlException e)
                    {
                        Log.WriteError("Error when quering for db. Exception:" + e.Message, "GetDatabases");
                    }
                }
            }

            return databases;
        }

        public static bool UserIsMemberOfRole(string database, string server, string instanceName, string user,
            string role)
        {
            if (!DatabaseExist(database, server, instanceName))
            {
                Log.WriteInfo("Database '" + database + "' instance '" + instanceName + "' was not found.",
                    "UserIsMemberOfRole");
                return false;
            }

            using (var con = new SqlConnection(BuildConnectionsString(server, instanceName)))
            {
                try
                {
                    con.Open();
                }
                catch (SqlException)
                {
                    Log.WriteError("Could not open connection to database server", "UserIsMemberOfRole");
                    return false;
                }

                var command = @"USE [" + database + "]; select is_rolemember('" + role + "','" + user + "')";
                using (var com = new SqlCommand(command))
                {
                    com.Connection = con;
                    try
                    {
                        var obj = com.ExecuteScalar();
                        if (string.IsNullOrEmpty(obj.ToString()))
                        {
                            return false;
                        }
                    }
                    catch (SqlException e)
                    {
                        Log.WriteError(
                            "SQL error when quering for roles. SQL command '" + command + "' Exception:" + e.Message,
                            "UserIsMemberOfRole");
                        return false;
                    }
                }
            }

            return true;
        }

        public static void AttachDatabase(string database, string server, string instanceName, string pathToMdfFile,
            string pathToLdfFile)
        {
            using (var con = new SqlConnection(BuildConnectionsString(server, instanceName)))
            {
                try
                {
                    con.Open();
                }
                catch (SqlException)
                {
                    Log.WriteError("Could not open connection to database server", "DropDatabase");
                    return;
                }

                var attachCommand = string.Format(CultureInfo.CurrentUICulture, @"sp_attach_db '{0}', '{1}', '{2}'",
                    database, pathToMdfFile, pathToLdfFile);
                using (var attachCom = new SqlCommand(attachCommand))
                {
                    attachCom.Connection = con;
                    try
                    {
                        attachCom.ExecuteNonQuery();
                    }
                    catch (SqlException e)
                    {
                        if (!e.Message.Contains("does not exist"))
                        {
                            throw;
                        }
                    }
                }
            }
        }

        public static string BuildConnectionsString()
        {
            return BuildConnectionsString(LocalHost, null);
        }

        public static string BuildConnectionsString(string instanceName)
        {
            return BuildConnectionsString(LocalHost, instanceName);
        }

        public static string BuildConnectionsString(string server, string instanceName)
        {
            var builder = new SqlConnectionStringBuilder();

            if (server == null)
            {
                server = LocalHost;
            }

            if (string.IsNullOrEmpty(instanceName))
            {
                builder.DataSource = server;
            }
            else
            {
                builder.DataSource = server + "\\" + instanceName;
            }

            builder.IntegratedSecurity = true;
            builder.InitialCatalog = InitialCatalog;

            return builder.ConnectionString;
        }
    }
}