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
