using System;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Runtime.CompilerServices;

namespace Minimig
{
    internal static class Program
    {
        private enum Command
        {
            None,
            RunMigrations,
            GetCount,
        }

        private static void Main(string[] args) => Run(args);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void Run(string[] args)
        {
            var root = new RootCommand()
            {
                new Option<string>(["--connection", "-c"], @"A connection string (can be PostgreSQL or SQLServer or MySQL).
                                                                    For integrated auth, you can use --database and --server instead."),
                new Option<string>(["--database", "-d"], "Generates an integrated auth connection string for the specified database."),
                new Option<string>(["--server", "-s"], "Generates an integrated auth connection string with the specified server (default: localhost)."),
                new Option<string>(["--folder", "-f"], "The folder containing your .sql migration files (defaults to current working directory)."),
                new Option<int>(["--timeout", "-t"], "Command timeout duration in seconds (default: 30)."),
                new Option<bool>("--preview", "Preview the migration without running it."),
                new Option<DatabaseProvider>(["--provider", "-p"], "Use a specific database provider options: sqlserver (default), postgresql, mysql."),
                new Option<bool>("--global", "Run all outstanding migrations in a single transaction, if possible."),
                new Option<string>("--table", "The table name to use for the migrations (default: migrations)."),
                new Option<string>("--schema", "The schema name to use for the migrations (default: dbo for sqlserver, public for postgresql, mysql for mysql)."),
                new Option<bool>("--force", "Will rerun modified migrations."),
                new Option<bool>("--count", "Counts the number of outstanding migrations."),
            };

            root.Handler = CommandHandler.Create((Options options) =>
            {
                Command cmd = options.Count ? Command.GetCount : Command.RunMigrations;

                try
                {
                    switch (cmd)
                    {
                        case Command.RunMigrations:
                            var result = Migrator.RunOutstandingMigrations(options);
                            if (!result.Success)
                                Environment.Exit(1);
                            break;

                        case Command.GetCount:
                            int total = Migrator.GetOutstandingMigrationsCount(options);
                            Console.WriteLine($"{total} outstanding migrations");
                            Console.WriteLine();
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine();
                    Environment.Exit(1);
                }

                Environment.Exit(0);
            });

            root.Invoke(args);

        }
    }
}
