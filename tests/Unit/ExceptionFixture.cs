using System.Text.RegularExpressions;
using Minimig;
using Xunit;

namespace MinimigTests.Unit
{
    public class ExceptionFixture
    {
        [Theory]
        [InlineData("..\\..\\..\\..\\sampleMigrations\\0001 - Add One and Two tables.sql")]
        public void Check_migration_exception(string filePath)
        {
            //Arrange
            const string modified = "has been modified since it was run";
            var regex = new Regex("\r\n|\n\r|\n|\r", RegexOptions.Compiled);
            var mig = new Migration(filePath, regex);

            //Act
            var exception = new MigrationChangedException(mig);

            //Assert
            Assert.Contains(modified, exception.Message);
            Assert.Contains(mig.Filename, exception.Message);
        }
    }
}
