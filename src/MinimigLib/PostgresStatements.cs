using System.Text.RegularExpressions;

namespace Minimig
{
    internal class PostgresStatements : ISqlStatements
    {
        public Regex CommandSplitter { get; } = new Regex(@"^\s\S");
        public string DoesMigrationsTableExist { get; }
        public string DoesSchemaMigrationExist { get; }
        public string CreateMigrationsTable { get; }
        public string DropMigrationsTable { get; }
        public string RenameMigration { get; }
        public string UpdateMigrationHash { get; }
        public string InsertMigration { get; }
        public string GetAlreadyRan { get; }

        internal PostgresStatements(string migrationsTableName, string schemaName = "public")
        {
            DoesSchemaMigrationExist = $@"SELECT count(*) FROM information_schema.schemata WHERE schema_name = '{schemaName}';";

            DoesMigrationsTableExist = $@"
                SELECT count(*) FROM information_schema.tables 
                WHERE table_name = '{migrationsTableName}'
                AND table_schema = '{schemaName};";

            CreateMigrationsTable = $@"
                CREATE TABLE {schemaName}.{migrationsTableName}
                (
                    Id SERIAL,
                    Filename varchar(260) not null,
                    Hash varchar(40) not null,
                    ExecutionDate timestamp not null,
                    Duration int not null,

                    constraint PK_{schemaName}_{migrationsTableName} primary key (Id),
                    constraint UX_{schemaName}_{migrationsTableName}_Filename unique (Filename),
                    constraint UX_{schemaName}_{migrationsTableName}_Hash unique (Hash)
                );
                ";

            DropMigrationsTable = $@"DROP TABLE {schemaName}.{migrationsTableName}";
            RenameMigration = $"UPDATE {schemaName}.{migrationsTableName} SET Filename = @Filename WHERE Hash = @Hash;";
            UpdateMigrationHash = $"UPDATE {schemaName}.{migrationsTableName} SET Hash = @Hash, ExecutionDate = @ExecutionDate, Duration = @Duration WHERE Filename = @Filename;";
            InsertMigration = $"INSERT {schemaName}.{migrationsTableName} (Filename, Hash, ExecutionDate, Duration) VALUES (@Filename, @Hash, @ExecutionDate, @Duration);";
            GetAlreadyRan = $"SELECT * FROM {schemaName}.{migrationsTableName} ORDER BY ExecutionDate, Id;";
        }
    }
}