using Minimig;
using System;
using Xunit;

namespace MinimigTests.Unit
{
    public class OptionsFixture
    {
        [Theory]
        [InlineData(@"MyMigration")]
        public void Get_migration_table_with_value(string inputTable)
        {
            //Arrange
            var options = new Options() { MigrationsTable = inputTable };

            //Act
            string table = options.GetMigrationsTable();

            //Assert
            Assert.Equal(inputTable, table);
        }

        [Fact]
        public void Get_migration_table_default()
        {
            //Arrange
            const string inputTable = "Migrations";
            var options = new Options();

            //Act
            string table = options.GetMigrationsTable();

            //Assert
            Assert.Equal(inputTable, table);
        }

        [Theory]
        [InlineData(@"C:\\temp\\")]
        public void Get_migration_folder_with_value(string inputFolder)
        {
            //Arrange
            var options = new Options() { MigrationsFolder = inputFolder };

            //Act
            string folder = options.GetFolder();

            //Assert
            Assert.Equal(inputFolder, folder);
        }

        [Fact]
        public void Get_migration_folder_default()
        {
            //Arrange
            string inputFolder = System.IO.Directory.GetCurrentDirectory();
            var options = new Options();

            //Act
            string folder = options.GetFolder();

            //Assert
            Assert.Equal(inputFolder, folder);
        }

        [Theory]
        [InlineData(@"C:\\temp\\")]
        public void Get_migration_connection_string_with_value(string inputConnection)
        {
            //Arrange
            var options = new Options() { ConnectionString = inputConnection };

            //Act
            string conn = options.GetConnectionString(DatabaseProvider.SqlServer);

            //Assert
            Assert.Equal(inputConnection, conn);
        }

        [Theory]
        [InlineData("myServer", "myDb")]
        public void Get_connection_string_with_server_and_database(string inputServer, string inputDatabase)
        {
            //Arrange
            string inputConnection = $"Persist Security Info=False;Integrated Security=true;Initial Catalog={inputDatabase};server={inputServer}";
            var options = new Options() { Server = inputServer, Database = inputDatabase };

            //Act
            string conn = options.GetConnectionString(DatabaseProvider.SqlServer);

            //Assert
            Assert.Equal(inputConnection, conn);
        }

        [Theory]
        [InlineData("myDb")]
        public void Get_connection_string_with_database(string inputDatabase)
        {
            //Arrange
            string inputConnection = $"Persist Security Info=False;Integrated Security=true;Initial Catalog={inputDatabase};server=localhost";
            var options = new Options() { Database = inputDatabase };

            //Act
            string conn = options.GetConnectionString(DatabaseProvider.SqlServer);

            //Assert
            Assert.Equal(inputConnection, conn);
        }

        [Fact]
        public void Assert_valid_exception()
        {
            //Arrange
            var options = new Options();

            //Act
            Action action = new Action(options.AssertValid);

            //Assert
            Assert.Throws<Exception>(action);
        }

        [Fact]
        public void Assert_valid_exception_migrations_table()
        {
            //Arrange
            var options = new Options() { MigrationsTable = "$Inv@lid N@me" };

            //Act
            Action action = new Action(options.AssertValid);

            //Assert
            Assert.Throws<Exception>(action);
        }
    }
}
