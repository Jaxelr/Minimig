using System;
using System.Collections.Generic;
using System.Data;
using Minimig;
using Xunit;

namespace MinimigTests.Integration
{
    public class ConnectionContextFixture
    {
        public static IEnumerable<object[]> GetData()
        {
            //We do this to pass the connection from Appveyor or locally
            string sqlServerConnEnv = Environment.GetEnvironmentVariable("Sql_Connection");
            if(string.IsNullOrEmpty(sqlServerConnEnv))
            {
                sqlServerConnEnv = $"Server=(local);Database=master;Trusted_Connection=true;";
            }

            string postgresConnEnv = Environment.GetEnvironmentVariable("Postgres_Connection");
            if(string.IsNullOrEmpty(postgresConnEnv))
            {
                postgresConnEnv = $"Server=localhost;Port=5432;Database=postgres;Integrated Security=true;Username=postgres";
            }

            return new List<object[]>
            {
                new object[] { sqlServerConnEnv, DatabaseProvider.SqlServer },
                new object[] { postgresConnEnv, DatabaseProvider.Postgres }
            };
        }

        [Theory]
        [MemberData(nameof(GetData))]
        public void Open_connection(string connectionString, DatabaseProvider provider)
        {
            //Arrange
            var options = new Options() { ConnectionString = connectionString, Provider = provider };

            //Act
            var context = new ConnectionContext(options);
            context.Open();

            //Assert
            Assert.Equal(ConnectionState.Open, context.Connection.State);
        }

        [Theory]
        [MemberData(nameof(GetData))]
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
        [MemberData(nameof(GetData))]
        public void Connection_has_pending_transactions(string connectionString, DatabaseProvider provider)
        {
            //Arrange
            var options = new Options() { ConnectionString = connectionString, Provider = provider };

            //Act
            var context = new ConnectionContext(options);
            context.Open();
            context.BeginTransaction();

            //Assert
            Assert.Equal(ConnectionState.Open, context.Connection.State);
            Assert.True(context.HasPendingTransaction);
        }

        [Theory]
        [MemberData(nameof(GetData))]
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
        [MemberData(nameof(GetData))]
        public void Connection_has_rollback_transactions(string connectionString, DatabaseProvider provider)
        {
            //Arrange
            var options = new Options() { ConnectionString = connectionString, Provider = provider };

            //Act
            var context = new ConnectionContext(options);
            context.Open();
            context.BeginTransaction();
            context.Rollback();

            //Assert
            Assert.Equal(ConnectionState.Open, context.Connection.State);
            Assert.False(context.HasPendingTransaction);
        }

        [Theory]
        [MemberData(nameof(GetData))]
        public void Execute_command_connection(string connectionString, DatabaseProvider provider)
        {
            //Arrange
            var options = new Options() { ConnectionString = connectionString, Provider = provider };

            //Act
            var context = new ConnectionContext(options);
            context.Open();
            context.ExecuteCommand("SELECT 1");

            //Assert
            Assert.Equal(ConnectionState.Open, context.Connection.State);
        }

        [Theory]
        [MemberData(nameof(GetData))]
        public void Execute_create_and_drop_migration_table(string connectionString, DatabaseProvider provider)
        {
            //Arrange
            var options = new Options() { ConnectionString = connectionString, Provider = provider };

            //Act
            var context = new ConnectionContext(options);
            context.Open();
            context.CreateMigrationsTable();
            bool exists = context.MigrationTableExists();
            context.DropMigrationsTable();
            bool existsAfter = context.MigrationTableExists();
            context.Dispose();

            //Assert
            Assert.Equal(ConnectionState.Closed, context.Connection.State);
            Assert.True(exists);
            Assert.False(existsAfter);
        }

        [Theory]
        [MemberData(nameof(GetData))]
        public void Execute_create_and_drop_schema(string connectionString, DatabaseProvider provider)
        {
            //Arrange
            string schema = "minimigtest";
            var options = new Options() { ConnectionString = connectionString, Provider = provider, MigrationsTableSchema = schema};

            //Act
            var context = new ConnectionContext(options);
            context.Open();
            context.BeginTransaction();
            context.ExecuteCommand($"Create schema {schema}");
            context.Commit();
            bool existsSchema = context.SchemaMigrationExists();

            //Clean Up
            context.ExecuteCommand($"Drop schema {schema}");
            context.Dispose();

            //Assert
            Assert.Equal(ConnectionState.Closed, context.Connection.State);
            Assert.True(existsSchema);
        }

        [Theory]
        [MemberData(nameof(GetData))]
        public void Execute_create_and_drop_schema_table(string connectionString, DatabaseProvider provider)
        {
            //Arrange
            string schema = "minimigtest";
            string table = "minimigtabletest";
            var options = new Options() { ConnectionString = connectionString, Provider = provider, MigrationsTableSchema = schema, MigrationsTable = table };

            //Act
            var context = new ConnectionContext(options);
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
    }
}
