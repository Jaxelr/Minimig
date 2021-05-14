﻿using System;
using System.Collections.Generic;
using System.Data;
using Minimig;
using MinimigTests.Fakes;
using Xunit;

namespace MinimigTests.Integration
{
    /*
     * Some of these unit tests require that we use trusted connections which means that the sql instance cannot be a docker image.
     */

    public class ConnectionContextFixture
    {
        public static IEnumerable<object[]> GetConnectionData()
        {
            //We do this to pass the connection from Appveyor or locally
            string sqlServerConnEnv = Environment.GetEnvironmentVariable("Sql_Connection");
            if (string.IsNullOrEmpty(sqlServerConnEnv))
            {
                sqlServerConnEnv = "Server=(local);Database=master;Trusted_Connection=true;";
            }

            string postgresConnEnv = Environment.GetEnvironmentVariable("Postgres_Connection");
            if (string.IsNullOrEmpty(postgresConnEnv))
            {
                postgresConnEnv = "Server=localhost;Port=5432;Database=postgres;Username=postgres;Password=postgres;";
            }

            string mysqlConnEnv = Environment.GetEnvironmentVariable("MySql_Connection");
            if (string.IsNullOrEmpty(mysqlConnEnv))
            {
                mysqlConnEnv = "Server=127.0.0.1;Port=3306;Database=test;User Id=root;";
            }

            Console.WriteLine($"Sql Server Connection: {sqlServerConnEnv}");
            Console.WriteLine($"Postgres Connection: {postgresConnEnv}");
            Console.WriteLine($"MySql Connection: {mysqlConnEnv}");

            return new List<object[]>
            {
                new object[] { sqlServerConnEnv, DatabaseProvider.sqlserver },
                new object[] { postgresConnEnv, DatabaseProvider.postgres },
                new object[] { mysqlConnEnv, DatabaseProvider.mysql }
            };
        }

        [Theory]
        [MemberData(nameof(GetConnectionData))]
        public void Open_connection(string connectionString, DatabaseProvider provider)
        {
            //Arrange
            var options = new Options() { ConnectionString = connectionString, Provider = provider };

            //Act
            using var context = new ConnectionContext(options);
            context.Open();

            //Assert
            Assert.Equal(ConnectionState.Open, context.Connection.State);
        }

        [Theory]
        [MemberData(nameof(GetConnectionData))]
        public void Dispose_connection(string connectionString, DatabaseProvider provider)
        {
            //Arrange
            var options = new Options() { ConnectionString = connectionString, Provider = provider };

            //Act
            var context = new ConnectionContext(options);
            context.Open();
            context.Dispose();

            //Assert
            Assert.Equal(ConnectionState.Closed, context.Connection.State);
        }

        [Theory]
        [MemberData(nameof(GetConnectionData))]
        public void Connection_has_pending_transactions(string connectionString, DatabaseProvider provider)
        {
            //Arrange
            var options = new Options() { ConnectionString = connectionString, Provider = provider };

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
            var options = new Options() { ConnectionString = connectionString, Provider = provider };

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
            var options = new Options() { ConnectionString = connectionString, Provider = provider };

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
            var options = new Options() { ConnectionString = connectionString, Provider = provider, IsPreview = true };

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
            var options = new Options() { ConnectionString = connectionString, Provider = provider };

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
            var options = new Options() { ConnectionString = connectionString, Provider = provider };

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
            var options = new Options() { ConnectionString = connectionString, Provider = provider };

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
            var options = new Options() { ConnectionString = connectionString, Provider = provider, MigrationsTableSchema = schema };

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
            var options = new Options() { ConnectionString = connectionString, Provider = provider, MigrationsTableSchema = schema, MigrationsTable = table };

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
            var options = new Options() { ConnectionString = connectionString, Provider = provider, MigrationsTable = table };
            var row = new FakeMigrationRow();

            //Act
            using var context = new ConnectionContext(options);
            context.Open();
            context.CreateMigrationsTable();
            bool exists = context.MigrationTableExists();
            context.InsertMigrationRecord(row);
            context.DropMigrationsTable();
            bool existsAfter = context.MigrationTableExists();

            //Assert
            Assert.Equal(ConnectionState.Open, context.Connection.State);
            Assert.True(exists);
            Assert.False(existsAfter);
        }

        [Theory]
        [MemberData(nameof(GetConnectionData))]
        public void Execute_create_migration_table_and_insert_check_row(string connectionString, DatabaseProvider provider)
        {
            //Arrange
            const string table = "minimigTableTest3";
            var options = new Options() { ConnectionString = connectionString, Provider = provider, MigrationsTable = table };
            var row = new FakeMigrationRow();
            const string dateFormat = "yyyy-MM-dd hh:mm:ss";

            //Act
            using var context = new ConnectionContext(options);
            context.Open();
            context.CreateMigrationsTable();
            context.InsertMigrationRecord(row);
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
        public void Execute_create_migration_table_and_update_check_row(string connectionString, DatabaseProvider provider)
        {
            //Arrange
            const string table = "minimigTableTest4";
            var options = new Options() { ConnectionString = connectionString, Provider = provider, MigrationsTable = table };
            var row = new FakeMigrationRow();
            const int newDuration = 20;
            string newHash = Guid.NewGuid().ToString();
            const string dateFormat = "yyyy-MM-dd hh:mm:ss";

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
            var options = new Options() { ConnectionString = connectionString, Provider = provider };
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
            var options = new Options() { ConnectionString = connectionString, Provider = provider, MigrationsTable = table };
            var migration = new FakeMigration(filePath);
            var row = new FakeMigrationRow(migration.Filename, migration.Hash);
            const string dateFormat = "yyyy-MM-dd hh:mm:ss";

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
            var options = new Options() { ConnectionString = connectionString, Provider = provider };
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
