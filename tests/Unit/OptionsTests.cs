using System;
using Minimig;
using Xunit;

namespace MinimigTests.Unit;

public class OptionsTests
{
    [Theory]
    [InlineData("MyMigration")]
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
        string conn = options.GetConnectionString(DatabaseProvider.sqlserver);

        //Assert
        Assert.Equal(inputConnection, conn);
    }

    [Theory]
    [InlineData(".", "master")]
    public void Get_connection_string_with_server_and_database_sqlserver(string inputServer, string inputDatabase)
    {
        //Arrange
        string inputConnection = $"Persist Security Info=False;Integrated Security=true;Initial Catalog={inputDatabase};server={inputServer}";
        var options = new Options() { Server = inputServer, Database = inputDatabase };

        //Act
        string conn = options.GetConnectionString(DatabaseProvider.sqlserver);

        //Assert
        Assert.Equal(inputConnection, conn);
    }

    [Theory]
    [InlineData("localhost", "postgres")]
    public void Get_connection_string_with_server_and_database_postgres(string inputServer, string inputDatabase)
    {
        //Arrange
        string inputConnection = $"Server={inputServer};Port=5432;Database={inputDatabase};Integrated Security=true;";
        var options = new Options() { Server = inputServer, Database = inputDatabase };

        //Act
        string conn = options.GetConnectionString(DatabaseProvider.postgresql);

        //Assert
        Assert.Equal(inputConnection, conn);
    }

    [Theory]
    [InlineData("master")]
    public void Get_connection_string_with_database(string inputDatabase)
    {
        //Arrange
        string inputConnection = $"Persist Security Info=False;Integrated Security=true;Initial Catalog={inputDatabase};server=.";
        var options = new Options() { Database = inputDatabase };

        //Act
        string conn = options.GetConnectionString(DatabaseProvider.sqlserver);

        //Assert
        Assert.Equal(inputConnection, conn);
    }

    [Theory]
    [InlineData("myMigrations", "master")]
    public void Assert_valid_migrations_table(string table, string db)
    {
        //Arrange
        var options = new Options() { MigrationsTable = table, Database = db };

        //Act
        var action = new Action(options.AssertValid);
        var result = (Options) action.Target;
        action.Invoke();

        //Assert
        Assert.Equal(result.MigrationsTable, table);
        Assert.Equal(result.Database, db);
        Assert.NotNull(result);
    }

    [Fact]
    public void Assert_valid_exception()
    {
        //Arrange
        var options = new Options();

        //Act
        var action = new Action(options.AssertValid);

        //Assert
        Assert.Throws<Exception>(action);
    }

    [Fact]
    public void Assert_valid_exception_invalid_path()
    {
        //Arrange
        var options = new Options() { MigrationsFolder = "C:\\randommissingpath\\" };

        //Act
        var action = new Action(options.AssertValid);

        //Assert
        Assert.Throws<Exception>(action);
    }

    [Fact]
    public void Assert_valid_exception_migrations_table()
    {
        //Arrange
        var options = new Options() { MigrationsTable = "$Inv@lid N@me", Database = "master" };

        //Act
        var action = new Action(options.AssertValid);

        //Assert
        Assert.Throws<Exception>(action);
    }

    [Fact]
    public void Assert_valid_exception_migrations_table_empty()
    {
        //Arrange
        var options = new Options() { MigrationsTable = "" };

        //Act
        var action = new Action(options.AssertValid);

        //Assert
        Assert.Throws<Exception>(action);
    }

    [Fact]
    public void Map_Database_Provider_postgresql()
    {
        //Arrange
        const string provider = "postgresql";
        var options = new Options();

        //Act
        var databaseProvider = options.MapDatabaseProvider(provider);

        //Assert
        Assert.Equal(DatabaseProvider.postgresql, databaseProvider);
        Assert.Equal(provider, databaseProvider.ToString());
    }

    [Fact]
    public void Map_Database_Provider_sqlserver()
    {
        //Arrange
        const string provider = "sqlserver";
        var options = new Options();

        //Act
        var databaseProvider = options.MapDatabaseProvider(provider);

        //Assert
        Assert.Equal(DatabaseProvider.sqlserver, databaseProvider);
        Assert.Equal(provider, databaseProvider.ToString());
    }

    [Fact]
    public void Map_Database_Provider_mysql()
    {
        //Arrange
        const string provider = "mysql";
        var options = new Options();

        //Act
        var databaseProvider = options.MapDatabaseProvider(provider);

        //Assert
        Assert.Equal(DatabaseProvider.mysql, databaseProvider);
        Assert.Equal(provider, databaseProvider.ToString());
    }

    [Fact]
    public void Map_Database_Provider_invalid()
    {
        //Arrange
        const string provider = "somethingelse";
        var options = new Options();

        //Act & Assert
        Assert.Throws<Exception>(() => options.MapDatabaseProvider(provider));
    }
}
