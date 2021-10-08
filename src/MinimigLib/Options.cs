using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Minimig
{
    public class Options
    {
        public string ConnectionString { get; set; }
        public string Database { get; set; }
        public string Server { get; set; }
        public int CommandTimeout { get; set; } = 30;
        public string MigrationsFolder { get; set; }
        public bool IsPreview { get; set; }
        public bool UseGlobalTransaction { get; set; }
        public string MigrationsTable { get; set; }
        public string MigrationsTableSchema { get; set; }
        public TextWriter Output { get; set; }
        public bool Force { get; set; }
        public DatabaseProvider Provider { get; set; }

        /// <summary>
        /// Determine if the current options satisfy a plausible execution of a migration
        /// </summary>
        public void AssertValid()
        {
            if (!Directory.Exists(GetFolder()))
            {
                throw new Exception($"Invalid folder or possible unscaped \\; current folder argument is \"{MigrationsFolder}\"");
            }

            if (string.IsNullOrEmpty(ConnectionString) == string.IsNullOrEmpty(Database))
            {
                throw new Exception("Either a connection string or a database must be specified.");
            }

            if (!string.IsNullOrEmpty(MigrationsTable))
            {
                if (!Regex.IsMatch(MigrationsTable, "^[a-zA-Z]+$"))
                    throw new Exception("Migrations table name can only contain letters A-Z.");
            }
        }

        /// <summary>
        /// Construct a ConnectionString  based on the current options for the provider given.
        /// </summary>
        /// <param name="provider"></param>
        /// <returns>A valid ConnectionString</returns>
        internal string GetConnectionString(DatabaseProvider provider)
        {
            if (!string.IsNullOrEmpty(ConnectionString))
                return ConnectionString;

            if (string.IsNullOrEmpty(Database))
                throw new Exception("No database assign to infer Connection String");

            if (provider is DatabaseProvider.sqlserver)
                return $"Persist Security Info=False;Integrated Security=true;Initial Catalog={Database};server={(string.IsNullOrEmpty(Server) ? "." : Server)}";
            else if (provider is DatabaseProvider.postgresql)
                return $"Server={(string.IsNullOrEmpty(Server) ? "localhost" : Server)};Port=5432;Database={Database};Integrated Security=true;";
            else if (provider is DatabaseProvider.mysql)
                return $"Server={(string.IsNullOrEmpty(Server) ? "localhost" : Server)};Port=3306;Database={Database};IntegratedSecurity=yes;";
            else
#pragma warning disable RCS1079 // Throwing of new NotImplementedException.
                throw new NotImplementedException($"Unsupported DatabaseProvider {Provider}");
#pragma warning restore RCS1079 // Throwing of new NotImplementedException.
        }

        /// <summary>
        /// Get the current migrations table
        /// </summary>
        /// <returns>The current migrations table</returns>
        internal string GetMigrationsTable() => string.IsNullOrEmpty(MigrationsTable) ? "Migrations" : MigrationsTable;

        /// <summary>
        /// Get the current migrations schema
        /// </summary>
        /// <returns>The current migrations schema</returns>
        internal string GetMigrationsTableSchema()
        {
            if (string.IsNullOrEmpty(MigrationsTableSchema))
            {
                switch (Provider)
                {
                    case DatabaseProvider.postgresql:
                        return "public";
                    case DatabaseProvider.mysql:
                        return "mysql";
                    default:
                        return "dbo";
                }
            }
            else
            {
                return MigrationsTableSchema;
            }
        }

        /// <summary>
        /// Map the provider text into the plausible enums
        /// </summary>
        /// <param name="input"></param>
        /// <returns>An enum with the mapped provider</returns>
        public DatabaseProvider MapDatabaseProvider(string input)
        {
            if (Enum.TryParse(input.ToLowerInvariant(), out DatabaseProvider provider))
            {
                return provider;
            }

            throw new Exception("The string provided as a provider doesnt correspond to one of the possible values (sqlserver, postgresql, mysql)");
        }

        /// <summary>
        /// Get the current migrations folder (defaults to current directory)
        /// </summary>
        /// <returns>A string with the directory requested</returns>
        internal string GetFolder() => string.IsNullOrEmpty(MigrationsFolder) ? Directory.GetCurrentDirectory() : MigrationsFolder;
    }
}
