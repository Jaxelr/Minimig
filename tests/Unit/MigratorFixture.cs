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
            string version = "0.3.0";

            //Act
            string result = Migrator.GetVersion();

            //Assert
            Assert.Equal(version, result);
        }

        [Theory]
        [InlineData(".", "master", true)]
        public void Migrator_instantiation_preview(string server, string database, bool isPreview)
        {
            //Arrange
            var option = new Options() { Server = server, Database = database, IsPreview = isPreview };

            //Act
            using (var mig = new Migrator(option))
            {
                //Assert
                Assert.Empty(mig.Migrations);
            }
        }
    }
}
