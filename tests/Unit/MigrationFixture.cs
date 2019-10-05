using System.Text.RegularExpressions;
using Minimig;
using Xunit;
using System.IO;
using System;

namespace MinimigTests.Unit
{
    public class MigrationFixture
    {
        [Theory]
        [InlineData("..\\..\\..\\..\\sampleMigrations\\0001 - Add One and Two tables.sql")]
        public void Check_migration(string filePath)
        {
            //Arrange
            var regex = new Regex("\r\n|\n\r|\n|\r", RegexOptions.Compiled);
            string filename = Path.GetFileName(filePath);

            //Act
            var migration = new Migration(filePath, regex);

            //Assert
            Assert.Equal(migration.Filename, filename);
            Assert.True(Guid.TryParse(migration.Hash, out _));
            Assert.True(migration.UseTransaction);
        }

        [Theory]
        [InlineData("..\\..\\..\\..\\sampleMigrations\\0003 - Insert Three data.sql")]
        public void Check_migration_no_transaction(string filePath)
        {
            //Arrange
            var regex = new Regex("\r\n|\n\r|\n|\r", RegexOptions.Compiled);
            string filename = Path.GetFileName(filePath);

            //Act
            var migration = new Migration(filePath, regex);

            //Assert
            Assert.Equal(migration.Filename, filename);
            Assert.True(Guid.TryParse(migration.Hash, out _));
            Assert.False(migration.UseTransaction);
        }
    }
}
