using System;

namespace Minimig;

public class MigrationChangedException : Exception
{
    internal MigrationChangedException(Migration migration) : base($"{migration.Filename} has been modified since it was run. Use --force to re-run it.")
    {
    }

    internal MigrationChangedException()
    {
    }

    internal MigrationChangedException(string message) : base(message)
    {
    }

    internal MigrationChangedException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
