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
            var row = new MigrationRow()
            {
                Id = 1,
                Filename = "c:\\temp\\abc",
                Duration = 0,
                Hash = "uniqueKey",
                ExecutionDate = DateTime.Now
            };

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
            string Filename = "c:\\temp\\abc";
            var row = new MigrationRow()
            {
                Id = 1,
                Filename = Filename,
                Duration = 0,
                Hash = "uniqueKey",
                ExecutionDate = DateTime.Now
            };

            var rows = new MigrationRow[1] { row };

            //Act
            var ran = new AlreadyRan(rows);
            var found = ran.ByFilename[Filename];

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
            string key = "uniqueKey";
            var row = new MigrationRow()
            {
                Id = 1,
                Filename = "c:\\temp\\abc",
                Duration = 0,
                Hash = key,
                ExecutionDate = DateTime.Now
            };

            var rows = new MigrationRow[1] { row };

            //Act
            var ran = new AlreadyRan(rows);
            var found = ran.ByHash[key];

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
            var row = new MigrationRow()
            {
                Id = 1,
                Filename = "c:\\temp\\abc",
                Duration = 0,
                Hash = "uniqueKey",
                ExecutionDate = DateTime.Now
            };

            var rows = new MigrationRow[1] { row };

            //Act
            var ran = new AlreadyRan(rows);
            int count = ran.Count;

            //Assert
            Assert.Equal(1, count);
        }
    }
}
