using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

[assembly: InternalsVisibleTo("MinimigTests")]

namespace Minimig
{
    public class Migrator : IDisposable
    {
        private readonly ConnectionContext db;
        private bool tableExists;
        private readonly string inputSchema;
        private readonly bool isPreview;
        private readonly bool useGlobalTransaction;
        private readonly TextWriter output;
        private readonly bool force;

        private readonly AlreadyRan alreadyRan;

        public IEnumerable<Migration> Migrations { get; }

        /// <summary>
        /// Create an instance of the migrator object after validation are completed
        /// </summary>
        /// <param name="options"></param>
        internal Migrator(Options options)
        {
            output = options.Output;
            inputSchema = options.MigrationsTableSchema;

            options.AssertValid();

            isPreview = options.IsPreview;
            useGlobalTransaction = options.UseGlobalTransaction || isPreview; // always run preview in a global transaction so previous migrations are seen
            force = options.Force;

            string dir = options.GetFolder();

            Log("Minimig Migrator");
            Log($"    Directory:        {dir}");
            Log($"    Provider:         {options.Provider}");

            db = new ConnectionContext(options);

            Migrations = GetAllMigrations(dir, db.CommandSplitter);

            Log($"    Database:         {db.Database}");

            db.Open();

            Log($"    Transaction Mode: {(useGlobalTransaction ? "Global" : "Individual")}");

            if (!ValidateMigrationsSchemaIsAvailable())
            {
                return;
            }

            EnsureMigrationsTableExists();

            alreadyRan = tableExists ? db.GetAlreadyRan() : new AlreadyRan(Enumerable.Empty<MigrationRow>());
            Log($"    Prior Migrations: {alreadyRan.Count}");
            if (alreadyRan.Count > 0)
            {
                var last = alreadyRan.Last;
                Log($"    Last Migration:   \"{last.Filename}\" on {last.ExecutionDate:u}");
            }

            Log();

            if (isPreview && !options.UseGlobalTransaction)
            {
                Log("Using global transaction mode because of preview mode");
                Log();
            }
        }

        /// <summary>
        /// Dispose of the database connection context
        /// </summary>
        public void Dispose() => db?.Dispose();

        /// <summary>
        /// Get minimig version
        /// </summary>
        /// <returns>A string detailing the version of the console</returns>
        public static string GetVersion()
        {
            var attr = typeof(Migrator).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            return attr.InformationalVersion;
        }

        /// <summary>
        /// Get the amount of migrations left to process
        /// </summary>
        /// <param name="options"></param>
        /// <returns>An int with the total of migrations left</returns>
        public static int GetOutstandingMigrationsCount(Options options)
        {
            using (var migrator = Create(options))
            {
                int count = 0;
                foreach (var m in migrator.Migrations)
                {
                    if (m.GetMigrateMode(migrator.alreadyRan) != MigrateMode.Skip)
                        count++;
                }

                return count;
            }
        }

        /// <summary>
        /// Execute the migrations left to process
        /// </summary>
        /// <param name="options"></param>
        /// <returns>An object defining the migration result</returns>
        public static MigrationResult RunOutstandingMigrations(Options options)
        {
            using (var migrator = Create(options))
            {
                return migrator.RunOutstandingMigrations();
            }
        }

        /// <summary>
        /// Create the migrator object instance
        /// </summary>
        /// <param name="options"></param>
        /// <returns>An instance of the migrator class</returns>
        // this only exists because you don't expect a constructor to perform I/O, whereas calling Create() implies there might be some work being performed
        private static Migrator Create(Options options) => new Migrator(options);

        /// <summary>
        /// Get all the migrations available in a directory specified
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="commandSplitter"></param>
        /// <returns>An generic Enumerable of Migrations</returns>
        private static IEnumerable<Migration> GetAllMigrations(string directory, Regex commandSplitter) =>
            Directory.GetFiles(directory, "*.sql")
                     .OrderBy(f => f)
                     .Select(f => new Migration(f, commandSplitter));

        /// <summary>
        /// Execute the migrations left
        /// </summary>
        /// <returns>An object defining the migration result</returns>
        private MigrationResult RunOutstandingMigrations()
        {
            Log("Running migrations" + (isPreview ? " (preview mode)" : ""));
            Log();

            var result = new MigrationResult();

            Migration current = null;
            try
            {
                foreach (var m in Migrations)
                {
                    current = m;
                    result.Attempted++;
                    switch (Migrate(m))
                    {
                        case MigrateMode.Skip:
                            result.Skipped++;
                            break;

                        case MigrateMode.Run:
                            result.Ran++;
                            break;

                        case MigrateMode.HashMismatch:
                            result.Forced++;
                            break;

                        case MigrateMode.Rename:
                            result.Renamed++;
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                if (db.HasPendingTransaction)
                    db.Commit();

                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Exception = ex;

                MigrationFailed(current, ex);
            }

            Log($"Attempted {result.Attempted} migrations.");
            if (result.Ran > 0)
                Log($"  Ran:     {result.Ran}");
            if (result.Forced > 0)
                Log($"  Forced:  {result.Forced}");
            if (result.Skipped > 0)
                Log($"  Skipped: {result.Skipped}");
            if (result.Renamed > 0)
                Log($"  Renamed: {result.Renamed}");

            Log();
            Log((result.Success ? "SUCCESS" : "FAIL") + (isPreview ? " (preview mode)" : ""));

            return result;
        }

        /// <summary>
        /// Calculate what must be done with the migration
        /// </summary>
        /// <param name="migration">A migration execution object</param>
        /// <returns>An enum defining what to do with the migration</returns>
        private MigrateMode Migrate(Migration migration)
        {
            var mode = migration.GetMigrateMode(alreadyRan);

            if (mode == MigrateMode.Skip)
                return mode;

            if (mode == MigrateMode.Rename)
            {
                RenameMigration(migration);
                return mode;
            }

            if (mode == MigrateMode.HashMismatch && !force)
            {
                throw new MigrationChangedException(migration);
            }

            if (!migration.UseTransaction && isPreview)
            {
                Log($"  Skipping \"{migration.Filename}\". It cannot be run in preview mode because the no-transaction header is set.");
                Log();
                return MigrateMode.Skip;
            }

            RunMigrationCommands(migration, mode);
            return mode;
        }

        /// <summary>
        /// Rename the migration
        /// </summary>
        /// <param name="migration">A migration execution object</param>
        private void RenameMigration(Migration migration)
        {
            var existing = alreadyRan.ByHash[migration.Hash];
            Log($"  Filename has changed (\"{existing.Filename}\" in the database, \"{migration.Filename}\" in file system) - updating.");
            Log();

            BeginMigration(useTransaction: true);
            db.RenameMigration(migration);
            EndMigration(migration);
        }

        /// <summary>
        /// Run the migrations commands as based on the migration mode
        /// </summary>
        /// <param name="migration">A migration execution object</param>
        /// <param name="mode">An enum defining the migration mode</param>
        private void RunMigrationCommands(Migration migration, MigrateMode mode)
        {
            BeginMigration(migration.UseTransaction);

            if (mode == MigrateMode.Run)
                Log($"  Running \"{migration.Filename}\" {(migration.UseTransaction ? "" : " (NO TRANSACTION)")}");
            else if (mode == MigrateMode.HashMismatch)
                Log($"  {migration.Filename} has been modified since it was run. It is being run again because --force was used.");
            else
                throw new Exception($"Minimig bug: RunMigrationCommands called with mode: {mode}");

            var sw = new Stopwatch();
            sw.Start();
            foreach (string cmd in migration.SqlCommands)
            {
                int result = db.ExecuteCommand(cmd);
                Log("    Result: " + (result == -1 ? "No Rows Affected" : result + " rows"));
            }
            sw.Stop();

            if (!isPreview || tableExists)
            {
                var recordRow = new MigrationRow()
                {
                    Filename = migration.Filename,
                    Hash = migration.Hash,
                    ExecutionDate = DateTime.UtcNow,
                    Duration = (int) sw.ElapsedMilliseconds,
                };

                if (mode == MigrateMode.Run)
                    db.InsertMigrationRecord(recordRow);
                else
                    db.UpdateMigrationRecordHash(recordRow);
            }

            Log();
            EndMigration(migration);
        }

        /// <summary>
        /// Start the migration
        /// </summary>
        /// <param name="useTransaction">flag to determine if a transaciton is required or not</param>
        private void BeginMigration(bool useTransaction)
        {
            if (useTransaction)
            {
                if (!db.HasPendingTransaction)
                {
                    db.BeginTransaction();
                }
            }
            else
            {
                if (db.HasPendingTransaction)
                {
                    Log("  Breaking up Global Transaction");

                    if (db.FilesInCurrentTransaction.Count > 0)
                        Log("    Committing all migrations which have run up to this point...");
                }
            }
        }

        /// <summary>
        /// Finish the migration
        /// </summary>
        /// <param name="migration">A migration execution object</param>
        private void EndMigration(Migration migration)
        {
            if (db.HasPendingTransaction)
            {
                if (useGlobalTransaction)
                    db.FilesInCurrentTransaction.Add(migration.Filename);
                else
                    db.Commit();
            }
        }

        /// <summary>
        /// Handle a failed migration
        /// </summary>
        /// <param name="migration">A migration execution object</param>
        /// <param name="ex">An exception indicating what error ocurred</param>
        private void MigrationFailed(Migration migration, Exception ex)
        {
            Log();
            if (migration == null)
            {
                Log("ERROR:");
            }
            else
            {
                if (!db.HasPendingTransaction && !migration.UseTransaction)
                    Log($"FAILED WITHOUT A TRANSACTION: {migration.Filename}");
                else
                    Log($"FAILED: {migration.Filename}");
            }

            Log(ex.Message);
            Log();

            if (db.HasPendingTransaction)
            {
                if (db.FilesInCurrentTransaction.Count > 0)
                {
                    Log(" Rolling back prior migrations:");
                    foreach (string f in db.FilesInCurrentTransaction)
                    {
                        Log("    " + f);
                    }
                }

                db.Rollback();
            }
        }

        /// <summary>
        /// Validate that the migration table exists prior to processing, if it doesnt, it creates it
        /// </summary>
        private void EnsureMigrationsTableExists()
        {
            if (db.MigrationTableExists())
            {
                tableExists = true;
                return;
            }

            if (isPreview)
            {
                // don't want to create the table if this is just a preview
                tableExists = false;
                return;
            }

            db.CreateMigrationsTable();
            tableExists = true;
        }

        /// <summary>
        /// Validate that the migration schema exists prior to processing
        /// </summary>
        /// <returns>A boolean value indicating if the schema exists</returns>
        private bool ValidateMigrationsSchemaIsAvailable()
        {
            if (db.SchemaMigrationExists())
            {
                return true;
            }

            Log($"No schema found (or no access given) that corresponds to the given input schema {inputSchema}");
            return false;
        }

        /// <summary>
        /// Write to the console the message sent
        /// </summary>
        /// <param name="str">A string to write into the console, defaults to empty string</param>
        private void Log(string str = "") => output?.WriteLine(str);
    }
}
