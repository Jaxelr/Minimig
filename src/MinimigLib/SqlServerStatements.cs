using System.Text.RegularExpressions;

namespace Minimig
{
    internal class SqlServerStatements : ISqlStatements
    {
        public Regex CommandSplitter { get; } = new Regex(@"^\s*GO\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        public string DoesMigrationsTableExist { get; }
        public string DoesSchemaMigrationTableExist { get; }
        public string CreateMigrationsTable { get; }
        public string RenameMigration { get; }
        public string UpdateMigrationHash { get; }
        public string InsertMigration { get; }
        public string GetAlreadyRan { get; }

        internal SqlServerStatements(string migrationsTableName, string schemaName = "dbo")
        {
            DoesSchemaMigrationTableExist = $@"SELECT count(*) FROM sys.schemas WHERE s.name = '{schemaName}'";

            DoesMigrationsTableExist = $@"
                SELECT count(*) FROM sys.tables t
                JOIN sys.schemas s
                    ON  t.schema_id = t.schema_id
                WHERE t.name = '{migrationsTableName}' AND s.name = '{schemaName}';";

            CreateMigrationsTable = $@"
                CREATE TABLE [{schemaName}].[{migrationsTableName}]
                (
                    Id int not null Identity(1,1),
                    Filename nvarchar(260) not null,
                    Hash varchar(40) not null,
                    ExecutionDate datetime not null,
                    Duration int not null,

                    constraint PK_{schemaName}_{migrationsTableName} primary key clustered (Id),
                    constraint UX_{schemaName}_{migrationsTableName}_Filename unique (Filename),
                    constraint UX_{schemaName}_{migrationsTableName}_Hash unique (Hash)
                );";

            RenameMigration = $"update [{schemaName}].[{migrationsTableName}] set Filename = @Filename where Hash = @Hash;";
            UpdateMigrationHash = $"update [{schemaName}].[{migrationsTableName}] set Hash = @Hash, ExecutionDate = @ExecutionDate, Duration = @Duration where Filename = @Filename;";
            InsertMigration = $"insert [{schemaName}].[{migrationsTableName}] (Filename, Hash, ExecutionDate, Duration) values (@Filename, @Hash, @ExecutionDate, @Duration);";
            GetAlreadyRan = $"select * from [{schemaName}].[{migrationsTableName}] order by ExecutionDate, Id;";
        }
    }
}