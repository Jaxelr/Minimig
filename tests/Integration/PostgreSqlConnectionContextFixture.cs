using System;
using System.Collections.Generic;
using System.Data;
using Minimig;
using Xunit;

namespace MinimigTests.Integration
{
    public class PostgreSqlConnectionContextFixture
    {
        private string Database => "postgres";

        private static string connectionString;

        public PostgreSqlConnectionContextFixture()
        {
            connectionString = $"Server=localhost;Port=5432;Database={Database};Integrated Security=true;Username=postgres";

            string connEnv = Environment.GetEnvironmentVariable("Postgres_Connection");

            if(!string.IsNullOrEmpty(connEnv))
            {
                connectionString = connEnv;
            }
        }

        public static IEnumerable<object[]> GetData()
        {
            connectionString = $"Server=localhost;Port=5432;Database=postgres;Integrated Security=true;Username=postgres";

            return new List<object[]>
            {
                new object[] { connectionString, DatabaseProvider.Postgres }
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
        
        [Fact]
        public void Dispose_connection()
        {
            //Arrange
            var options = new Options() { ConnectionString = connectionString, Provider = DatabaseProvider.Postgres };

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
            var options = new Options() { ConnectionString = connectionString, Provider = DatabaseProvider.Postgres };

            //Act
            var context = new ConnectionContext(options);
            context.Open();
            context.BeginTransaction();

            //Assert
            Assert.Equal(ConnectionState.Open, context.Connection.State);
            Assert.True(context.HasPendingTransaction);
        }

        [Fact]
        public void Connection_has_completed_transactions()
        {
            //Arrange
            var options = new Options() { ConnectionString = connectionString, Provider = DatabaseProvider.Postgres };

            //Act
            var context = new ConnectionContext(options);
            context.Open();
            context.BeginTransaction();
            context.Commit();

            //Assert
            Assert.Equal(ConnectionState.Open, context.Connection.State);
            Assert.False(context.HasPendingTransaction);
        }

        [Fact]
        public void Connection_has_rollback_transactions()
        {
            //Arrange
            var options = new Options() { ConnectionString = connectionString, Provider = DatabaseProvider.Postgres };

            //Act
            var context = new ConnectionContext(options);
            context.Open();
            context.BeginTransaction();
            context.Rollback();

            //Assert
            Assert.Equal(ConnectionState.Open, context.Connection.State);
            Assert.False(context.HasPendingTransaction);
        }

        [Fact]
        public void Execute_command_connection()
        {
            //Arrange
            var options = new Options() { ConnectionString = connectionString, Provider = DatabaseProvider.Postgres };

            //Act
            var context = new ConnectionContext(options);
            context.Open();
            context.ExecuteCommand("SELECT 1");

            //Assert
            Assert.Equal(ConnectionState.Open, context.Connection.State);
        }
    }
}