﻿using System.Text.RegularExpressions;

namespace Minimig;

internal class SqlServerStatements : ISqlStatements
{
    public Regex CommandSplitter { get; } = new Regex(@"^\s*GO\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline);
    public string DoesMigrationsTableExist { get; }
    public string DoesSchemaMigrationExist { get; }
    public string CreateMigrationsTable { get; }
    public string DropMigrationsTable { get; }
    public string RenameMigration { get; }
    public string UpdateMigrationHash { get; }
    public string InsertMigration { get; }
    public string GetAlreadyRan { get; }

    /// <summary>
    /// Define the SqlServer statements needed to run migrations as based on the definition needed for the provider
    /// </summary>
    /// <param name="migrationsTableName">Migrations table name to record the entries</param>
    /// <param name="schemaName">Migrations schema to use to map the entries</param>
    internal SqlServerStatements(string migrationsTableName, string schemaName = "dbo")
    {
        DoesSchemaMigrationExist = $"SELECT COUNT(*) FROM information_schema.schemata WHERE schema_name = '{schemaName}';";

        DoesMigrationsTableExist = $@"
                SELECT COUNT(*) FROM information_schema.tables t
                INNER JOIN information_schema.schemata s
                    ON t.table_schema = s.schema_name
                WHERE t.table_name = '{migrationsTableName}' AND s.schema_name = '{schemaName}';";

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

        DropMigrationsTable = $"DROP TABLE [{schemaName}].[{migrationsTableName}]";
        RenameMigration = $"UPDATE [{schemaName}].[{migrationsTableName}] SET Filename = @Filename WHERE Hash = @Hash;";
        UpdateMigrationHash = $"UPDATE [{schemaName}].[{migrationsTableName}] SET Hash = @Hash, ExecutionDate = @ExecutionDate, Duration = @Duration WHERE Filename = @Filename;";
        InsertMigration = $"INSERT [{schemaName}].[{migrationsTableName}] (Filename, Hash, ExecutionDate, Duration) values (@Filename, @Hash, @ExecutionDate, @Duration);";
        GetAlreadyRan = $"SELECT * FROM [{schemaName}].[{migrationsTableName}] ORDER BY ExecutionDate, Id;";
    }
}
