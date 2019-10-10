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
    }
}
