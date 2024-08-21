using Minimig;
using MinimigTests.Fakes;
using Xunit;

namespace MinimigTests.Unit;

public class ExceptionTests
{
    [Theory]
    [InlineData("SampleMigrations\\SqlServer\\0001 - Add One and Two tables.sql")]
    public void Check_migration_exception(string filePath)
    {
        //Arrange
        const string modified = "has been modified since it was run";
        var mig = new FakeMigration(filePath);

        //Act
        var exception = new MigrationChangedException(mig);

        //Assert
        Assert.Contains(modified, exception.Message);
        Assert.Contains(mig.Filename, exception.Message);
    }
}
