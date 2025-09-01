using System;
using MinimigTests.Fakes;

namespace MinimigTests.Unit;

public class ExceptionTests
{
    [Theory]
    [InlineData("SampleMigrations\\SqlServer\\0001 - Add One and Two tables.sql")]
    public void FakeMigration_ShouldHaveMigrationChangedException(string filePath)
    {
        //Arrange
        const string modified = "has been modified since it was run";
        var mig = new FakeMigration(filePath);

        //Act
        var exception = new MigrationChangedException(mig);

        //Assert
        Assert.Contains(modified, exception.Message);
        Assert.Contains(mig.Filename, exception.Message);
    }

    [Fact]
    public void DefaultConstructor_ShouldHaveMessage()
    {
        // Arrange
        const string message = "Exception of type 'Minimig.MigrationChangedException' was thrown.";

        // Act
        var exception = new MigrationChangedException();

        // Assert
        Assert.Equal(message, exception.Message);
    }

    [Theory]
    [InlineData("Custom error message")]
    public void Constructor_WithMessage_ShouldSetMessage(string message)
    {
        // Act
        var exception = new MigrationChangedException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }

    [Theory]
    [InlineData("Custom error message", "Inner exception message")]
    public void Constructor_WithMessageAndInnerException_ShouldSetMessageAndInnerException(string message, string innerMessage)
    {
        // Arrange
        var innerException = new Exception(innerMessage);

        // Act
        var exception = new MigrationChangedException(message, innerException);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Equal(innerException, exception.InnerException);
    }
}
