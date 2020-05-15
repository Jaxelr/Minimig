using System;
using System.Collections.Generic;
using System.Data;
using Minimig;
using MinimigTests.Fakes;
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
            using (var context = new ConnectionContext(options))
            {
                context.Open();

                //Assert
                Assert.Equal(ConnectionState.Open, context.Connection.State);
            }
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
            using (var context = new ConnectionContext(options))
            {
                context.Open();
                context.BeginTransaction();

                //Assert
                Assert.Equal(ConnectionState.Open, context.Connection.State);
                Assert.True(context.HasPendingTransaction);
            }
        }

        [Theory]
        [MemberData(nameof(GetData))]
        public void Connection_commit_without_begin_invalid_operation_exception(string connectionString, DatabaseProvider provider)
        {
            //Arrange
            var options = new Options() { ConnectionString = connectionString, Provider = provider };

            //Act
            using (var context = new ConnectionContext(options))
            {
                context.Open();
                void action() => context.Commit();

                //Assert
                Assert.Throws<InvalidOperationException>(action);
            }
        }

        [Theory]
        [MemberData(nameof(GetData))]
        public void Connection_has_completed_transactions(string connectionString, DatabaseProvider provider)
        {
            //Arrange
            var options = new Options() { ConnectionString = connectionString, Provider = provider };

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

        [Theory]
        [MemberData(nameof(GetData))]
        public void Connection_has_completed_transactions_on_preview(string connectionString, DatabaseProvider provider)
        {
            //Arrange
            var options = new Options() { ConnectionString = connectionString, Provider = provider, IsPreview = true };

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

        [Theory]
        [MemberData(nameof(GetData))]
        public void Connection_has_rollback_transactions(string connectionString, DatabaseProvider provider)
        {
            //Arrange
            var options = new Options() { ConnectionString = connectionString, Provider = provider };

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

        [Theory]
        [MemberData(nameof(GetData))]
        public void Execute_command_connection(string connectionString, DatabaseProvider provider)
        {
            //Arrange
            var options = new Options() { ConnectionString = connectionString, Provider = provider };

            //Act
            using (var context = new ConnectionContext(options))
            {
                context.Open();
                context.ExecuteCommand("SELECT 1");

                //Assert
                Assert.Equal(ConnectionState.Open, context.Connection.State);
            }
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
        [MemberData(nameof(GetData), parameters: "minimigtest")]
        public void Execute_create_and_drop_schema(string connectionString, DatabaseProvider provider, string schema)
        {
            //Arrange
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
        [MemberData(nameof(GetData), parameters: new object[] { "minimigtest", "minimigtabletest" })]
        public void Execute_create_and_drop_schema_table(string connectionString, DatabaseProvider provider, string schema, string table)
        {
            //Arrange
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

        [Theory]
        [MemberData(nameof(GetData), parameters: "minimigTableTest2")]
        public void Execute_create_migration_table_and_insert_row(string connectionString, DatabaseProvider provider, string table)
        {
            //Arrange
            var options = new Options() { ConnectionString = connectionString, Provider = provider, MigrationsTable = table };
            var row = new FakeMigrationRow();

            //Act
            var context = new ConnectionContext(options);
            context.Open();
            context.CreateMigrationsTable();
            bool exists = context.MigrationTableExists();
            context.InsertMigrationRecord(row);
            context.DropMigrationsTable();
            bool existsAfter = context.MigrationTableExists();
            context.Dispose();

            //Assert
            Assert.Equal(ConnectionState.Closed, context.Connection.State);
            Assert.True(exists);
            Assert.False(existsAfter);
        }

        [Theory]
        [MemberData(nameof(GetData), parameters: "minimigTableTest3")]
        public void Execute_create_migration_table_and_insert_check_row(string connectionString, DatabaseProvider provider, string table)
        {
            //Arrange
            var options = new Options() { ConnectionString = connectionString, Provider = provider, MigrationsTable = table };
            var row = new FakeMigrationRow();
            const string dateFormat = "yyyy-MM-dd hh:mm:ss";

            //Act
            var context = new ConnectionContext(options);
            context.Open();
            context.CreateMigrationsTable();
            context.InsertMigrationRecord(row);
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
        [MemberData(nameof(GetData), parameters: "minimigTableTest4")]
        public void Execute_create_migration_table_and_update_check_row(string connectionString, DatabaseProvider provider, string table)
        {
            //Arrange
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
        [MemberData(nameof(GetData))]
        public void Update_migration_without_record(string connectionString, DatabaseProvider provider)
        {
            //Arrange
            var options = new Options() { ConnectionString = connectionString, Provider = provider };
            var row = new FakeMigrationRow();

            //Act
            using (var context = new ConnectionContext(options))
            {
                context.Open();
                context.CreateMigrationsTable();
                void action() => context.UpdateMigrationRecordHash(row);

                //Assert
                Assert.Throws<Exception>(action);

                //Cleanup
                context.DropMigrationsTable();
                context.Dispose();
            }
        }

        [Theory]
        [MemberData(nameof(GetData), parameters: new object[] { "minimigTableTest5", "..\\..\\..\\..\\sampleMigrations\\0001 - Add One and Two tables.sql" })]
        public void Execute_create_migration_table_and_update_filename_row(string connectionString, DatabaseProvider provider, string table, string filePath)
        {
            //Arrange
            var options = new Options() { ConnectionString = connectionString, Provider = provider, MigrationsTable = table };
            var migration = new FakeMigration(filePath);
            var row = new FakeMigrationRow(migration.Filename, migration.Hash);
            const string dateFormat = "yyyy-MM-dd hh:mm:ss";

            //Act
            var context = new ConnectionContext(options);
            context.Open();
            context.CreateMigrationsTable();
            context.InsertMigrationRecord(row);
            context.RenameMigration(migration);
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
        [MemberData(nameof(GetData), parameters: "..\\..\\..\\..\\sampleMigrations\\0001 - Add One and Two tables.sql")]
        public void Rename_migration_without_record(string connectionString, DatabaseProvider provider, string filePath)
        {
            //Arrange
            var options = new Options() { ConnectionString = connectionString, Provider = provider };
            var migration = new FakeMigration(filePath);

            //Act
            using (var context = new ConnectionContext(options))
            {
                context.Open();
                context.CreateMigrationsTable();
                void action() => context.RenameMigration(migration);

                //Assert
                Assert.Throws<Exception>(action);

                //Cleanup
                context.DropMigrationsTable();
                context.Dispose();
            }
        }
    }
}
