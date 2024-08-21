using MinimigTests.Fakes;
using Xunit;

namespace MinimigTests.Unit;

public class AlreadyRanTests
{
    [Fact]
    public void Check_if_migration_row_already_ran_last()
    {
        //Arrange
        var row = new FakeMigrationRow();
        var ran = new FakeAlreadyRan(row);

        //Act
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
        var row = new FakeMigrationRow();
        var ran = new FakeAlreadyRan(row);

        //Act
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
        var row = new FakeMigrationRow();
        var ran = new FakeAlreadyRan(row);

        //Act
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
        var row = new FakeMigrationRow();
        var ran = new FakeAlreadyRan(row);

        //Act
        int count = ran.Count;

        //Assert
        Assert.Equal(1, count);
    }
}
