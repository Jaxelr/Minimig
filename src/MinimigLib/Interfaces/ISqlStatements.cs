using System.Text.RegularExpressions;

namespace Minimig
{
    internal interface ISqlStatements
    {
        /// <summary>
        /// The regular expression used to break up multiple commands within a single migration.
        /// </summary>
        Regex CommandSplitter { get; }

        /// <summary>
        /// Should return a single integer. 1 indicating the table exists. 0 indicating it does not.
        /// </summary>
        string DoesMigrationsTableExist { get; }

        /// <summary>
        /// Creates the migrations table. Does not take any parameters.
        /// </summary>
        string CreateMigrationsTable { get; }

        /// <summary>
        /// Changes the Filename of a migration. Requires @Filename and @Hash parameters.
        /// </summary>
        string RenameMigration { get; }

        /// <summary>
        /// Updates the hash of a migration by Filename. Requires @Filename, @Hash, @ExecutionDate, and @Duration parameters.
        /// </summary>
        string UpdateMigrationHash { get; }

        /// <summary>
        /// Inserts a new migration record. Requires @Filename, @Hash, @ExecutionDate, and @Duration parameters.
        /// </summary>
        string InsertMigration { get; }

        /// <summary>
        /// Selects all previously run migrations, ordered by ExecutedDate. Does not take any parameters.
        /// </summary>
        string GetAlreadyRan { get; }
    }
}