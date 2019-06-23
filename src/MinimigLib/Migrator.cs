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

        public List<Migration> Migrations { get; }

        private Migrator(Options options)
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

            Migrations = GetAllMigrations(dir, db.CommandSplitter).ToList();

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

        public void Dispose() => db?.Dispose();

        public static string GetVersion()
        {
            var attr = typeof(Migrator).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            return attr.InformationalVersion;
        }

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

        public static MigrationResult RunOutstandingMigrations(Options options)
        {
            using (var migrator = Create(options))
            {
                return migrator.RunOutstandingMigrations();
            }
        }

        // this only exists because you don't expect a constructor to perform I/O, whereas calling Create() implies there might be some work being performed
        private static Migrator Create(Options options) => new Migrator(options);

        private static IEnumerable<Migration> GetAllMigrations(string directory, Regex commandSplitter) =>
            Directory.GetFiles(directory, "*.sql").OrderBy(f => f).Select(f => new Migration(f, commandSplitter));

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
                    var mode = Migrate(m);

                    switch (mode)
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

        private void RenameMigration(Migration migration)
        {
            var existing = alreadyRan.ByHash[migration.Hash];
            Log($"  Filename has changed (\"{existing.Filename}\" in the database, \"{migration.Filename}\" in file system) - updating.");
            Log();

            BeginMigration(useTransaction: true);
            db.RenameMigration(migration);
            EndMigration(migration);
        }

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

        private void Log(string str = "") => output?.WriteLine(str);

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

        private bool ValidateMigrationsSchemaIsAvailable()
        {
            if (db.SchemaMigrationExists())
            {
                return true;
            }

            Log($"No schema found (or no access given) that corresponds to the given input schema {inputSchema}");
            return false;
        }
    }
}
