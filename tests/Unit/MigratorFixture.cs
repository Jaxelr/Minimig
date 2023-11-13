using Minimig;
using Xunit;

namespace MinimigTests.Unit;

public class MigratorFixture
{
    [Theory]
    [InlineData("(local)", "master", true, "previewTable")]
    public void Migrator_instantiation_preview(string server, string database, bool isPreview, string table)
    {
        //Arrange
        var option = new Options() { Server = server, Database = database, Preview = isPreview, Table = table };

        //Act
        using var mig = new Migrator(option);

        //Assert
        Assert.Empty(mig.Migrations);
    }
}
