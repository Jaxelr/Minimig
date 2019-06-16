using System;
using Minimig;
using Xunit;

namespace MinimigTests.UnitTests
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
        public void Construct_connection_context_exception()
        {
            //Arrange
            var options = new Options();

            //Act
            void action() => new ConnectionContext(options);

            //Assert
            Assert.Throws<Exception>(action);
        }

        [Fact]
        public void Construct_connection_context_exception_with_provider()
        {
            //Arrange
            var options = new Options() { Database = Database, Provider = DatabaseProvider.Postgres };

            //Act
            void action() => new ConnectionContext(options);

            //Assert
            Assert.Throws<NotImplementedException>(action);
        }

        [Fact]
        public void Construct_connection_context()
        {
            //Arrange
            var provider = DatabaseProvider.SqlServer;
            var options = new Options() { ConnectionString = connectionString };

            //Act
            var context = new ConnectionContext(options);

            //Assert
            Assert.Equal(context.Provider, provider);
            Assert.False(context.IsPreview);
            Assert.Equal(context.Database, Database);
        }
    }
}
