using System;
using System.Collections.Generic;
using System.Data;
using Minimig;
using MinimigTests.Fakes;
using Xunit;

namespace MinimigTests.Integration;

/// <summary>
/// These are integration tests to a live connection that require an existing active database and cant be entirely mocked.
/// Note: Some of these unit tests require that we use trusted connections which means that the sql instance cannot be a docker image.
/// </summary>
public class ConnectionContextTests
{
    /*
     * Some of these unit tests require that we use trusted connections which means that the sql instance cannot be a docker image.
     */

    public class ConnectionContextFixture
    {
        //We do this to pass the connection from Appveyor or locally
        string sqlServerConnEnv = Environment.GetEnvironmentVariable("Sql_Connection");
        if (string.IsNullOrEmpty(sqlServerConnEnv))
            sqlServerConnEnv = "Server=(local);Database=master;Trusted_Connection=true;";

        string postgresConnEnv = Environment.GetEnvironmentVariable("Postgres_Connection");
        if (string.IsNullOrEmpty(postgresConnEnv))
            postgresConnEnv = "Server=localhost;Port=5432;Database=postgres;Username=postgres;Password=Password12!;";

        string mysqlConnEnv = Environment.GetEnvironmentVariable("MySql_Connection");
        if (string.IsNullOrEmpty(mysqlConnEnv))
            mysqlConnEnv = "Server=127.0.0.1;Port=3306;Database=test;User Id=root;";

        return
        [
            [sqlServerConnEnv, DatabaseProvider.sqlserver],
            [postgresConnEnv, DatabaseProvider.postgresql],
            [mysqlConnEnv, DatabaseProvider.mysql]
        ];
    }

            //Act
            using var context = new ConnectionContext(options);
            context.Open();
            context.BeginTransaction();

            //Assert
            Assert.Equal(ConnectionState.Open, context.Connection.State);
            Assert.True(context.HasPendingTransaction);
        }

        [Theory]
        [MemberData(nameof(GetConnectionData))]
        public void Connection_commit_without_begin_invalid_operation_exception(string connectionString, DatabaseProvider provider)
        {
            //Arrange
            var options = new Options() { Connection = connectionString, Provider = provider };

            //Act
            using var context = new ConnectionContext(options);
            context.Open();
            void action() => context.Commit();

            //Assert
            Assert.Throws<InvalidOperationException>(action);
        }

        [Theory]
        [MemberData(nameof(GetConnectionData))]
        public void Connection_has_completed_transactions(string connectionString, DatabaseProvider provider)
        {
            //Arrange
            var options = new Options() { Connection = connectionString, Provider = provider };

            //Act
            var context = new ConnectionContext(options);
            context.Open();
            context.BeginTransaction();
            context.Commit();

            //Assert
            Assert.Equal(ConnectionState.Open, context.Connection.State);
            Assert.False(context.HasPendingTransaction);
        }

        [Theory]
        [MemberData(nameof(GetConnectionData))]
        public void Connection_has_completed_transactions_on_preview(string connectionString, DatabaseProvider provider)
        {
            //Arrange
            var options = new Options() { Connection = connectionString, Provider = provider, Preview = true };

            //Act
            var context = new ConnectionContext(options);
            context.Open();
            context.BeginTransaction();
            context.Commit();

            //Assert
            Assert.Equal(ConnectionState.Open, context.Connection.State);
            Assert.False(context.HasPendingTransaction);
        }

        [Theory]
        [MemberData(nameof(GetConnectionData))]
        public void Connection_has_rollback_transactions(string connectionString, DatabaseProvider provider)
        {
            //Arrange
            var options = new Options() { Connection = connectionString, Provider = provider };

            //Act
            using var context = new ConnectionContext(options);
            context.Open();
            context.BeginTransaction();
            context.Rollback();

            //Assert
            Assert.Equal(ConnectionState.Open, context.Connection.State);
            Assert.False(context.HasPendingTransaction);
        }

        [Theory]
        [MemberData(nameof(GetConnectionData))]
        public void Execute_command_connection(string connectionString, DatabaseProvider provider)
        {
            //Arrange
            var options = new Options() { Connection = connectionString, Provider = provider };

            //Act
            using var context = new ConnectionContext(options);
            context.Open();
            context.ExecuteCommand("SELECT 1");

            //Assert
            Assert.Equal(ConnectionState.Open, context.Connection.State);
        }

        [Theory]
        [MemberData(nameof(GetConnectionData))]
        public void Execute_create_and_drop_migration_table(string connectionString, DatabaseProvider provider)
        {
            //Arrange
            var options = new Options() { Connection = connectionString, Provider = provider };

            //Act
            using var context = new ConnectionContext(options);
            context.Open();
            context.CreateMigrationsTable();
            bool exists = context.MigrationTableExists();
            context.DropMigrationsTable();
            bool existsAfter = context.MigrationTableExists();

            //Assert
            Assert.Equal(ConnectionState.Open, context.Connection.State);
            Assert.True(exists);
            Assert.False(existsAfter);
        }

        [Theory]
        [MemberData(nameof(GetConnectionData))]
        public void Execute_create_and_drop_schema(string connectionString, DatabaseProvider provider)
        {
            //Arrange
            const string schema = "minimigtest";
            var options = new Options() { Connection = connectionString, Provider = provider, Schema = schema };

            //Act
            using var context = new ConnectionContext(options);
            context.Open();
            context.BeginTransaction();
            context.ExecuteCommand($"Create schema {schema}");
            context.Commit();
            bool existsSchema = context.SchemaMigrationExists();

            //Clean Up
            context.ExecuteCommand($"Drop schema {schema}");

            //Assert
            Assert.Equal(ConnectionState.Open, context.Connection.State);
            Assert.True(existsSchema);
        }

        [Theory]
        [MemberData(nameof(GetConnectionData))]
        public void Execute_create_and_drop_schema_table(string connectionString, DatabaseProvider provider)
        {
            //Arrange
            const string schema = "minimigtest2";
            const string table = "minimigtabletest";
            var options = new Options() { Connection = connectionString, Provider = provider, Schema = schema, Table = table };

            //Act
            using var context = new ConnectionContext(options);
            context.Open();
            context.BeginTransaction();
            context.ExecuteCommand($"Create schema {schema}");
            context.Commit();
            bool existsSchema = context.SchemaMigrationExists();
            context.CreateMigrationsTable();
            bool existsTable = context.SchemaMigrationExists();

            //Clean Up
            context.DropMigrationsTable();
            context.ExecuteCommand($"Drop schema {schema}");
            context.Dispose();

            //Assert
            Assert.Equal(ConnectionState.Closed, context.Connection.State);
            Assert.True(existsSchema);
            Assert.True(existsTable);
        }

        [Theory]
        [MemberData(nameof(GetConnectionData))]
        public void Execute_create_migration_table_and_insert_row(string connectionString, DatabaseProvider provider)
        {
            //Arrange
            const string table = "minimigTableTest2";
            var options = new Options() { Connection = connectionString, Provider = provider, Table = table };
            var row = new FakeMigrationRow();

        //Assert
        Assert.Equal(ConnectionState.Open, context.Connection.State);
        Assert.True(existsSchema);

        //Clean Up
        context.ExecuteCommand($"Drop schema {schema}");
    }

        [Theory]
        [MemberData(nameof(GetConnectionData))]
        public void Execute_create_migration_table_and_insert_check_row(string connectionString, DatabaseProvider provider)
        {
            //Arrange
            const string table = "minimigTableTest3";
            var options = new Options() { Connection = connectionString, Provider = provider, Table = table };
            var row = new FakeMigrationRow();
            const string dateFormat = "yyyy-MM-dd hh:mm";

            //Act
            using var context = new ConnectionContext(options);
            context.Open();
            context.CreateMigrationsTable();
            context.InsertMigrationRecord(row);
            var ran = context.GetAlreadyRan();
            context.DropMigrationsTable();

        //Assert
        Assert.Equal(ConnectionState.Open, context.Connection.State);
        Assert.True(existsSchema);
        Assert.True(existsTable);

        //Clean Up
        context.DropMigrationsTable();
        context.ExecuteCommand($"Drop schema {schema}");
        context.Dispose();
    }

            //Act
            var context = new ConnectionContext(options);
            context.Open();
            context.CreateMigrationsTable();
            context.InsertMigrationRecord(row);
            row.Duration = newDuration;
            row.Hash = newHash;
            context.UpdateMigrationRecordHash(row);
            var ran = context.GetAlreadyRan();
            context.DropMigrationsTable();
            context.Dispose();

            //Assert
            Assert.Equal(ConnectionState.Closed, context.Connection.State);
            Assert.Equal(ran.Last.Hash, row.Hash);
            Assert.Equal(ran.Last.Id, row.Id);
            Assert.Equal(ran.Last.Filename, row.Filename);
            Assert.Equal(ran.Last.ExecutionDate.ToString(dateFormat), row.ExecutionDate.ToString(dateFormat));
            Assert.Equal(ran.Last.Duration, row.Duration);
        }

        [Theory]
        [MemberData(nameof(GetConnectionData))]
        public void Update_migration_without_record(string connectionString, DatabaseProvider provider)
        {
            //Arrange
            var options = new Options() { Connection = connectionString, Provider = provider };
            var row = new FakeMigrationRow();

            //Act
            using var context = new ConnectionContext(options);
            context.Open();
            context.CreateMigrationsTable();
            void action() => context.UpdateMigrationRecordHash(row);

            //Assert
            Assert.Throws<Exception>(action);

            //Cleanup
            context.DropMigrationsTable();
        }

        [Theory]
        [MemberData(nameof(GetConnectionData))]
        public void Execute_create_migration_table_and_update_filename_row(string connectionString, DatabaseProvider provider)
        {
            //Arrange
            const string table = "minimigTableTest5";
            string filePath = $"SampleMigrations\\{provider}\\0001 - Add One and Two tables.sql";
            var options = new Options() { Connection = connectionString, Provider = provider, Table = table };
            var migration = new FakeMigration(filePath);
            var row = new FakeMigrationRow(migration.Filename, migration.Hash);
            const string dateFormat = "yyyy-MM-dd hh:mm";

            //Act
            using var context = new ConnectionContext(options);
            context.Open();
            context.CreateMigrationsTable();
            context.InsertMigrationRecord(row);
            context.RenameMigration(migration);
            var ran = context.GetAlreadyRan();
            context.DropMigrationsTable();

            //Assert
            Assert.Equal(ConnectionState.Open, context.Connection.State);
            Assert.Equal(ran.Last.Hash, row.Hash);
            Assert.Equal(ran.Last.Id, row.Id);
            Assert.Equal(ran.Last.Filename, row.Filename);
            Assert.Equal(ran.Last.ExecutionDate.ToString(dateFormat), row.ExecutionDate.ToString(dateFormat));
            Assert.Equal(ran.Last.Duration, row.Duration);
        }

        [Theory]
        [MemberData(nameof(GetConnectionData))]
        public void Rename_migration_without_record(string connectionString, DatabaseProvider provider)
        {
            //Arrange
            string filePath = $"SampleMigrations\\{provider}\\0001 - Add One and Two tables.sql";
            var options = new Options() { Connection = connectionString, Provider = provider };
            var migration = new FakeMigration(filePath);

            //Act
            using var context = new ConnectionContext(options);
            context.Open();
            context.CreateMigrationsTable();
            void action() => context.RenameMigration(migration);

            //Assert
            Assert.Throws<Exception>(action);

            //Cleanup
            context.DropMigrationsTable();
        }
    }
}
