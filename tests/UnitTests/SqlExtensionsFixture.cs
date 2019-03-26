using Minimig;
using Xunit;

namespace MinimigTests.UnitTests
{
    public class SqlExtensionsFixture
    {
        [Fact]
        public void Add_parameter_sql_command()
        {
            //Arrange
            var command = new System.Data.SqlClient.SqlCommand();
            string myParam = nameof(myParam);
            string myValue = nameof(myValue);

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
            var conn = new System.Data.SqlClient.SqlConnection();
            string sql = "select * from sys.tables;";
            int? timeout = 30;

            //Act
            var cmd = conn.NewCommand(sql, null, timeout);

            //Assert
            Assert.Equal(cmd.CommandText, sql);
            Assert.Null(cmd.Transaction);
            Assert.Equal(cmd.CommandTimeout, timeout);
        }
    }
}