using Microsoft.Data.SqlClient;
using Minimig;
using Xunit;

namespace MinimigTests.Unit;

public class SqlExtensionsTests
{
    [Fact]
    public void Add_parameter_sql_command()
    {
        //Arrange
        var command = new SqlCommand();
        const string myParam = nameof(myParam);
        const string myValue = nameof(myValue);

        //Act
        command.AddParameter(myParam, myValue);

        //Assert
        Assert.Equal(myParam, command.Parameters[0].ParameterName);
        Assert.Equal(myValue, command.Parameters[0].Value);
    }

    [Fact]
    public void Add_command_to_connection()
    {
        //Arrange
        var conn = new SqlConnection();
        const string sql = "select count(*) from sys.tables;";
        int? timeout = 30;

        //Act
        var cmd = conn.NewCommand(sql, null, timeout);

        //Assert
        Assert.Equal(sql, cmd.CommandText);
        Assert.Null(cmd.Transaction);
        Assert.Equal(timeout, cmd.CommandTimeout);
    }
}
