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
                throw new Exception($"Invalid folder or possible unscaped \\; current folder argument \"{MigrationsFolder}\"");
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
            
            return $@"Persist Security Info=False;Integrated Security=true;Initial Catalog={Database};server={
                (string.IsNullOrEmpty(Server) ? "." : Server)}";
        }

        internal string GetMigrationsTable() => string.IsNullOrEmpty(MigrationsTable) ? "Migrations" : MigrationsTable;
   
        internal string GetMigrationsTableSchema() => string.IsNullOrEmpty(MigrationsTableSchema) ? "dbo" : MigrationsTableSchema;

        internal string GetFolder() => string.IsNullOrEmpty(MigrationsFolder) ? Directory.GetCurrentDirectory() : MigrationsFolder;
        
    }
}
