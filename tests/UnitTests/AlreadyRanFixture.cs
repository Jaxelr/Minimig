using Minimig;
using Xunit;

namespace MinimigTests.UnitTests
{
    public class AlreadyRanFixture
    {
        [Fact]
        public void Check_if_migration_row_already_ran_false()
        {
            //Arrange
            var row = new MigrationRow();
            var rows = new MigrationRow[1] { row };

            //Act
            var ran = new AlreadyRan(rows);

            //Assert
        }
    }
}