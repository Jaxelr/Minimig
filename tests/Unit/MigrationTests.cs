using System;
using System.IO;
using Minimig;
using MinimigTests.Fakes;
using Xunit;

namespace MinimigTests.Unit;

public class MigrationTests
{
    [Theory]
    [InlineData("SampleMigrations\\SqlServer\\0001 - Add One and Two tables.sql")]
    public void Get_migration(string filepath)
    {
        //Arrange
        string filename = Path.GetFileName(filepath);

        //Act
        var migration = new FakeMigration(filepath);

        //Assert
        Assert.Equal(migration.Filename, filename);
        Assert.True(Guid.TryParse(migration.Hash, out _));
        Assert.True(migration.UseTransaction);
    }

    [Theory]
    [InlineData("SampleMigrations\\SqlServer\\0003 - Insert Three data.sql")]
    public void Get_migration_no_transaction(string filepath)
    {
        //Arrange
        string filename = Path.GetFileName(filepath);

        //Act
        var migration = new FakeMigration(filepath);

        //Assert
        Assert.Equal(migration.Filename, filename);
        Assert.True(Guid.TryParse(migration.Hash, out _));
        Assert.False(migration.UseTransaction);
    }

    [Theory]
    [InlineData("SampleMigrations\\SqlServer\\0003 - Insert Three data.sql", "uniqueKey", "c:\\temp\\abc")]
    public void Get_migration_mode_run(string filepath, string key, string filename)
    {
        //Arrange
        var row = new FakeMigrationRow(filename, key);
        var ran = new FakeAlreadyRan(row);

        //Act
        var migration = new FakeMigration(filepath);
        var mode = migration.GetMigrateMode(ran);

        //Assert
        Assert.Equal(MigrateMode.Run, mode);
    }

    [Theory]
    [InlineData("SampleMigrations\\SqlServer\\0003 - Insert Three data.sql", "b8b9fcf3-1c1f-8040-8c19-3d87b26dab92", "c:\\temp\\abc")]
    public void Get_migration_mode_run_renamed(string filepath, string key, string filename)
    {
        //Arrange
        var row = new FakeMigrationRow(filename, key);
        var ran = new FakeAlreadyRan(row);

        //Act
        var migration = new FakeMigration(filepath);
        var mode = migration.GetMigrateMode(ran);

        //Assert
        Assert.Equal(MigrateMode.Rename, mode);
    }

    [Theory]
    [InlineData("SampleMigrations\\SqlServer\\", "b8b9fcf3-1c1f-8040-8c19-3d87b26dab92", "0003 - Insert Three data.sql")]
    public void Get_migration_mode_run_skip(string filepath, string key, string filename)
    {
        //Arrange
        var row = new FakeMigrationRow(filename, key);
        var ran = new FakeAlreadyRan(row);

        //Act
        var migration = new FakeMigration(Path.Combine(filepath, filename));
        var mode = migration.GetMigrateMode(ran);

        //Assert
        Assert.Equal(MigrateMode.Skip, mode);
    }

    [Theory]
    [InlineData("SampleMigrations\\SqlServer\\", "00000-000", "0003 - Insert Three data.sql")]
    public void Get_migration_mode_run_has_mismatch(string filepath, string key, string filename)
    {
        //Arrange
        var row = new FakeMigrationRow(filename, key);
        var ran = new FakeAlreadyRan(row);

        //Act
        var migration = new FakeMigration(Path.Combine(filepath, filename));
        var mode = migration.GetMigrateMode(ran);

        //Assert
        Assert.Equal(MigrateMode.HashMismatch, mode);
    }
}
