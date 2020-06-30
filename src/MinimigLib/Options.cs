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

        internal string GetConnectionString(DatabaseProvider provider)
        {
            if (!string.IsNullOrEmpty(ConnectionString))
                return ConnectionString;

            if (string.IsNullOrEmpty(Database))
                throw new Exception("No database assign to infer Connection String");

            if (provider is DatabaseProvider.sqlserver)
                return $"Persist Security Info=False;Integrated Security=true;Initial Catalog={Database};server={(string.IsNullOrEmpty(Server) ? "." : Server)}";
            else if (provider is DatabaseProvider.postgres)
                return $"Server={(string.IsNullOrEmpty(Server) ? "localhost" : Server)};Port=5432;Database={Database};Integrated Security=true;";
            else
#pragma warning disable RCS1079 // Throwing of new NotImplementedException.
                throw new NotImplementedException($"Unsupported DatabaseProvider {Provider}");
#pragma warning restore RCS1079 // Throwing of new NotImplementedException.
        }

        internal string GetMigrationsTable() => string.IsNullOrEmpty(MigrationsTable) ? "Migrations" : MigrationsTable;

        internal string GetMigrationsTableSchema()
        {
            if (string.IsNullOrEmpty(MigrationsTableSchema))
            {
                switch (Provider)
                {
                    case DatabaseProvider.postgres:
                        return "public";

                    default:
                        return "dbo";
                }
            }
            else
            {
                return MigrationsTableSchema;
            }
        }

        public DatabaseProvider MapDatabaseProvider(string input)
        {
            if (Enum.TryParse(input.ToLowerInvariant(), out DatabaseProvider provider))
            {
                return provider;
            }

            throw new Exception("The string provided as a provider doesnt correspond to one of the possible values (sqlserver, postgres)");
        }

        internal string GetFolder() => string.IsNullOrEmpty(MigrationsFolder) ? Directory.GetCurrentDirectory() : MigrationsFolder;
    }
}
