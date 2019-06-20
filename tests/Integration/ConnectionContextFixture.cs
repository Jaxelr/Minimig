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
            var context = new ConnectionContext(options);
            context.Open();

            //Assert
            Assert.Equal(ConnectionState.Open, context.Connection.State);
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
            var options = new Options() { ConnectionString = connectionString, Provider = DatabaseProvider.SqlServer };

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
            var options = new Options() { ConnectionString = connectionString, Provider = DatabaseProvider.SqlServer };

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
            var options = new Options() { ConnectionString = connectionString, Provider = DatabaseProvider.SqlServer };

            //Act
            var context = new ConnectionContext(options);
            context.Open();
            context.ExecuteCommand("SELECT 1");

            //Assert
            Assert.Equal(ConnectionState.Open, context.Connection.State);
        }
    }
}
