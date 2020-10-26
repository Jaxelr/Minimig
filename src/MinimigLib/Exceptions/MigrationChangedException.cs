using System;

namespace Minimig
{
#pragma warning disable RCS1194 // Implement exception constructors.

    public class MigrationChangedException : Exception
#pragma warning restore RCS1194 // Implement exception constructors.
    {
        internal MigrationChangedException(Migration migration) : base($"{migration.Filename} has been modified since it was run. Use --force to re-run it.")
        {
        }
    }
}
