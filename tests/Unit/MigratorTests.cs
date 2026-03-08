namespace MinimigTests.Unit;

public class MigratorTests
{
    [Fact]
    public void Migrator_get_version()
    {
        //Arrange
        const string version = "0.12.0";

        //Act
        string result = Migrator.GetVersion();

        //Assert
        Assert.Contains(version, result);
    }
}
