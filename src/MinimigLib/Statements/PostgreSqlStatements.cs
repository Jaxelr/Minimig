using System.Text.RegularExpressions;

namespace Minimig
{
    internal class PostgreSqlStatements : ISqlStatements
    {
        public Regex CommandSplitter { get; } = new Regex(@"^\s*;\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        public string DoesMigrationsTableExist { get; }
        public string DoesSchemaMigrationExist { get; }
        public string CreateMigrationsTable { get; }
        public string DropMigrationsTable { get; }
        public string RenameMigration { get; }
        public string UpdateMigrationHash { get; }
        public string InsertMigration { get; }
        public string GetAlreadyRan { get; }

        internal PostgreSqlStatements(string migrationsTableName, string schemaName = "public")
        {
            DoesSchemaMigrationExist = $"SELECT COUNT(*) FROM information_schema.schemata WHERE schema_name = '{schemaName}';";

            DoesMigrationsTableExist = $@"
                SELECT COUNT(*) FROM information_schema.tables t
                INNER JOIN information_schema.schemata s
                    ON t.table_schema = s.schema_name
                WHERE t.table_name = '{migrationsTableName}' AND s.schema_name = '{schemaName}';";

            CreateMigrationsTable = $@"
                CREATE TABLE ""{schemaName}"".""{migrationsTableName}""
                (
	                Id serial unique,
	                Filename varchar(260) not null unique,
	                Hash varchar(40) not null unique,
	                ExecutionDate timestamp not null,
	                Duration int not null
                );";

            DropMigrationsTable = $@"DROP TABLE ""{schemaName}"".""{migrationsTableName}"";";
            RenameMigration = $@"UPDATE ""{schemaName}"".""{migrationsTableName}"" SET Filename = @Filename WHERE Hash = @Hash;";
            UpdateMigrationHash = $@"UPDATE ""{schemaName}"".""{migrationsTableName}"" SET Hash = @Hash, ExecutionDate = @ExecutionDate, Duration = @Duration WHERE Filename = @Filename;";
            InsertMigration = $@"INSERT INTO ""{schemaName}"".""{migrationsTableName}"" (Filename, Hash, ExecutionDate, Duration) values (@Filename, @Hash, @ExecutionDate, @Duration);";
            GetAlreadyRan = $@"SELECT * FROM ""{schemaName}"".""{migrationsTableName}"" ORDER BY ExecutionDate, Id;";
        }
    }
}
