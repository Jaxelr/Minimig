using System;
using System.Text.RegularExpressions;
using Minimig;
using Xunit;

namespace MinimigTests.Unit
{
    public class ConnectionContextFixture
    {
        private string Database => "master";

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
        public void Construct_connection_context_exception_missing_database()
        {
            //Arrange
            var options = new Options() { Database = string.Empty, Provider = DatabaseProvider.SqlServer };

            //Act
            void action() => new ConnectionContext(options);

            //Assert
            Assert.Throws<Exception>(action);
        }

        [Fact]
        public void Construct_connection_context()
        {
            //Arrange
            string connectionString = $"Server=.;Database={Database};Trusted_Connection=true;";
            var provider = DatabaseProvider.SqlServer;
            var options = new Options() { ConnectionString = connectionString, Provider = provider };

            //Act
            using (var context = new ConnectionContext(options))
            {

                //Assert
                Assert.Equal(context.Provider, provider);
                Assert.False(context.IsPreview);
                Assert.Equal(context.Database, Database);
            }
        }

        [Fact]
        public void Construct_connection_context_exception_missing_database_on_connection_string()
        {
            //Arrange
            string connectionString = $"Server=.;Database=;Trusted_Connection=true;";
            var provider = DatabaseProvider.SqlServer;
            var options = new Options() { ConnectionString = connectionString, Provider = provider };

            //Act
            void action() => new ConnectionContext(options);

            //Assert
            Assert.Throws<Exception>(action);
        }

        [Fact]
        public void Verify_command_splitter_sql_server()
        {
            //Arrange
            string connectionString = $"Server=.;Database={Database};Trusted_Connection=true;";
            var provider = DatabaseProvider.SqlServer;
            var options = new Options() { ConnectionString = connectionString, Provider = provider };

            //Act
            using (var context = new ConnectionContext(options))
            {
                //Assert
                Assert.Equal(context.CommandSplitter.Options, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                Assert.Equal(@"^\s*GO\s*$", context.CommandSplitter.ToString());
            }

        }
    }
}
