using Minimig;
using Xunit;

namespace MinimigTests.Unit
{
    public class MigratorFixture
    {
        [Fact]
        public void Migrator_get_version()
        {
            //Arrange
            const string version = "0.11.0";

            //Act
            string result = Migrator.GetVersion();

            //Assert
            Assert.Equal(version, result);
        }

        [Theory]
        [InlineData("(local)", "master", true, "previewTable")]
        public void Migrator_instantiation_preview(string server, string database, bool isPreview, string table)
        {
            //Arrange
            var option = new Options() { Server = server, Database = database, IsPreview = isPreview, MigrationsTable = table };

            //Act
            using var mig = new Migrator(option);

            //Assert
            Assert.Empty(mig.Migrations);
        }
    }
}
