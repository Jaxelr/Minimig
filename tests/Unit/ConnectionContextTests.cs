using System;
using System.Text.RegularExpressions;
using Minimig;
using Xunit;

namespace MinimigTests.Unit;

public class ConnectionContextTests
{
    private static string Database => "master";

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
        var options = new Options() { Database = Database, Provider = DatabaseProvider.unknown };

        //Act
        void action() => new ConnectionContext(options);

        //Assert
        Assert.Throws<NotImplementedException>(action);
    }

    [Fact]
    public void Construct_connection_context_exception_missing_database()
    {
        //Arrange
        var options = new Options() { Database = string.Empty, Provider = DatabaseProvider.sqlserver };

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
        const DatabaseProvider provider = DatabaseProvider.sqlserver;
        var options = new Options() { ConnectionString = connectionString, Provider = provider };

        //Act
        using var context = new ConnectionContext(options);

        //Assert
        Assert.Equal(provider, context.Provider);
        Assert.False(context.IsPreview);
        Assert.Equal(Database, context.Database);
    }

    [Fact]
    public void Construct_connection_context_exception_missing_database_on_connection_string()
    {
        //Arrange
        const string connectionString = "Server=.;Database=;Trusted_Connection=true;";
        const DatabaseProvider provider = DatabaseProvider.sqlserver;
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
        const DatabaseProvider provider = DatabaseProvider.sqlserver;
        var options = new Options() { ConnectionString = connectionString, Provider = provider };

        //Act
        using var context = new ConnectionContext(options);

        //Assert
        Assert.Equal(RegexOptions.IgnoreCase | RegexOptions.Multiline, context.CommandSplitter.Options);
        Assert.Equal(@"^\s*GO\s*$", context.CommandSplitter.ToString());
    }

    [Fact]
    public void Verify_command_splitter_postgresql()
    {
        //Arrange
        const string connectionString = "Server=localhost;Port=5432;Database=postgres;";
        const DatabaseProvider provider = DatabaseProvider.postgresql;
        var options = new Options() { ConnectionString = connectionString, Provider = provider };

        //Act
        using var context = new ConnectionContext(options);

        //Assert
        Assert.Equal(RegexOptions.IgnoreCase | RegexOptions.Multiline, context.CommandSplitter.Options);
        Assert.Equal(@"^\s*;\s*$", context.CommandSplitter.ToString());
    }
}
