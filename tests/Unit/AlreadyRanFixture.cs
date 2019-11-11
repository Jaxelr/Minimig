using Minimig;
using System;
using Xunit;

namespace MinimigTests.Unit
{
    public class AlreadyRanFixture
    {
        [Fact]
        public void Check_if_migration_row_already_ran_last()
        {
            //Arrange
            var row = new Fakes.FakeMigrationRow();
            var rows = new MigrationRow[1] { row };
            
            //Act
            var ran = new AlreadyRan(rows);
            var found = ran.Last;

            //Assert
            Assert.Equal(row.Id, found.Id);
            Assert.Equal(row.Hash, found.Hash);
            Assert.Equal(row.Filename, found.Filename);
            Assert.Equal(row.Duration, found.Duration);
            Assert.Equal(row.ExecutionDate, found.ExecutionDate);
        }

        [Fact]
        public void Check_if_migration_row_already_ran_by_filename()
        {
            //Arrange
            var row = new Fakes.FakeMigrationRow();
            var rows = new MigrationRow[1] { row };

            //Act
            var ran = new AlreadyRan(rows);
            var found = ran.ByFilename[row.Filename];

            //Assert
            Assert.Equal(row.Id, found.Id);
            Assert.Equal(row.Hash, found.Hash);
            Assert.Equal(row.Filename, found.Filename);
            Assert.Equal(row.Duration, found.Duration);
            Assert.Equal(row.ExecutionDate, found.ExecutionDate);
        }

        [Fact]
        public void Check_if_migration_row_already_ran_by_hash()
        {
            //Arrange
            var row = new Fakes.FakeMigrationRow();
            var rows = new MigrationRow[1] { row };

            //Act
            var ran = new AlreadyRan(rows);
            var found = ran.ByHash[row.Hash];

            //Assert
            Assert.Equal(row.Id, found.Id);
            Assert.Equal(row.Hash, found.Hash);
            Assert.Equal(row.Filename, found.Filename);
            Assert.Equal(row.Duration, found.Duration);
            Assert.Equal(row.ExecutionDate, found.ExecutionDate);
        }


        [Fact]
        public void Check_migration_row_count()
        {
            //Arrange
            var row = new Fakes.FakeMigrationRow();
            var rows = new MigrationRow[1] { row };

            //Act
            var ran = new AlreadyRan(rows);
            int count = ran.Count;

            //Assert
            Assert.Equal(1, count);
        }
    }
}
