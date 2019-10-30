using System;
using System.Data;
using Minimig;
using Xunit;

namespace MinimigTests.Integration
{
    public class ConnectionContextFixture
    {
        private string Database => "master";

        private readonly string connectionString;

        public ConnectionContextFixture()
        {
            connectionString = $"Server=(local);Database={Database};Trusted_Connection=true;";

            string connEnv = Environment.GetEnvironmentVariable("Sql_Connection");

            if (!string.IsNullOrEmpty(connEnv)) //We do this to pass the connection from Appveyor or locally.
            {
                connectionString = connEnv;
            }
        }

        [Fact]
        public void Open_connection()
        {
            //Arrange
            var options = new Options() { ConnectionString = connectionString, Provider = DatabaseProvider.SqlServer };

            //Act
            using (var context = new ConnectionContext(options))
            {
                context.Open();

                //Assert
                Assert.Equal(ConnectionState.Open, context.Connection.State);
            };
        }

        [Fact]
        public void Dispose_connection()
        {
            //Arrange
            var options = new Options() { ConnectionString = connectionString, Provider = DatabaseProvider.SqlServer };

            //Act
            var context = new ConnectionContext(options);
            context.Open();
            context.Dispose();

            //Assert
            Assert.Equal(ConnectionState.Closed, context.Connection.State);
        }

        [Fact]
        public void Connection_has_pending_transactions()
        {
            //Arrange
            var options = new Options() { ConnectionString = connectionString, Provider = DatabaseProvider.SqlServer };

            //Act
            using (var context = new ConnectionContext(options))
            {
                context.Open();
                context.BeginTransaction();

                //Assert
                Assert.Equal(ConnectionState.Open, context.Connection.State);
                Assert.True(context.HasPendingTransaction);
            }
        }

        [Fact]
        public void Connection_commit_without_begin_invalid_operation_exception()
        {
            //Arrange
            var options = new Options() { ConnectionString = connectionString, Provider = DatabaseProvider.SqlServer };

            //Act
            using (var context = new ConnectionContext(options))
            {
                context.Open();
                void action() => context.Commit();

                //Assert
                Assert.Throws<InvalidOperationException>(action);
            }
        }

        [Fact]
        public void Connection_has_completed_transactions()
        {
            //Arrange
            var options = new Options() { ConnectionString = connectionString, Provider = DatabaseProvider.SqlServer };

            //Act
            using (var connectionContext = new ConnectionContext(options))
            {
                var context = connectionContext;
                context.Open();
                context.BeginTransaction();
                context.Commit();

                //Assert
                Assert.Equal(ConnectionState.Open, context.Connection.State);
                Assert.False(context.HasPendingTransaction);
            }
        }

        [Fact]
        public void Connection_has_completed_transactions_on_preview()
        {
            //Arrange
            var options = new Options() { ConnectionString = connectionString, Provider = DatabaseProvider.SqlServer, IsPreview = true };

            //Act
            using (var connectionContext = new ConnectionContext(options))
            {
                var context = connectionContext;
                context.Open();
                context.BeginTransaction();
                context.Commit();

                //Assert
                Assert.Equal(ConnectionState.Open, context.Connection.State);
                Assert.False(context.HasPendingTransaction);
            }
        }


        [Fact]
        public void Connection_has_rollback_transactions()
        {
            //Arrange
            var options = new Options() { ConnectionString = connectionString, Provider = DatabaseProvider.SqlServer };

            //Act
            using (var context = new ConnectionContext(options))
            {
                context.Open();
                context.BeginTransaction();
                context.Rollback();

                //Assert
                Assert.Equal(ConnectionState.Open, context.Connection.State);
                Assert.False(context.HasPendingTransaction);
            }
        }

        [Fact]
        public void Execute_command_connection()
        {
            //Arrange
            var options = new Options() { ConnectionString = connectionString, Provider = DatabaseProvider.SqlServer };

            //Act
            using (var context = new ConnectionContext(options))
            {
                context.Open();
                context.ExecuteCommand("SELECT 1");

                //Assert
                Assert.Equal(ConnectionState.Open, context.Connection.State);
            }
        }

        [Fact]
        public void Execute_create_and_drop_migration_table()
        {
            //Arrange
            var options = new Options() { ConnectionString = connectionString, Provider = DatabaseProvider.SqlServer };

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
        [InlineData("minimigtest")]
        public void Execute_create_and_drop_schema(string schema)
        {
            //Arrange
            var options = new Options() { ConnectionString = connectionString, Provider = DatabaseProvider.SqlServer, MigrationsTableSchema = schema };

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
        [InlineData("minimigtest", "minimigTableTest")]
        public void Execute_create_and_drop_schema_table(string schema, string table)
        {
            //Arrange
            var options = new Options() { ConnectionString = connectionString, Provider = DatabaseProvider.SqlServer, MigrationsTableSchema = schema, MigrationsTable = table };

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
