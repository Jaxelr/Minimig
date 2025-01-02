using System;
using System.Runtime.CompilerServices;
using CommandLine.Options;

namespace Minimig;

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
        var cmd = TryParseArgs(args, out Options options);

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
                    int count = Migrator.GetOutstandingMigrationsCount(options);
                    Console.WriteLine($"{count} outstanding migrations");
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
    }

    /// <summary>
    /// Parse optional parameters passed from the console and return the action to perform
    /// </summary>
    /// <param name="args">Command line arguments array</param>
    /// <param name="options">Minimig custom option arguments</param>
    /// <returns>A Command enum that indicates which action will be performed</returns>
    private static Command TryParseArgs(string[] args, out Options options)
    {
        bool showHelp = false;
        bool showVersion = false;
        bool getCount = false;
        Options optionsTmp = options = new Options();

        var optionSet = new OptionSet()
        {
            { "h|help", "Shows this help message.", v => showHelp= v != null },
            {"c|connection=", "A connection string (can be PostgreSQL or SQLServer or MySQL). For integrated auth, you can use --database and --server instead.", v => optionsTmp.ConnectionString = v },
            {"d|database=", "Generates an integrated auth connection string for the specified database.", v => optionsTmp.Database = v },
            {"s|server=", "Generates an integrated auth connection string with the specified server (default: localhost).", v => optionsTmp.Server = v },
            {"f|folder=", "The folder containing your .sql migration files (defaults to current working directory).", v => optionsTmp.MigrationsFolder = v },
            {"timeout=", "Command timeout duration in seconds (default: 30).", v => optionsTmp.CommandTimeout = int.Parse(v) },
            {"preview", "Run outstanding migrations, but roll them back.", v => optionsTmp.IsPreview = v != null },
            {"p|provider=", "Use a specific database provider options: sqlserver (default), postgresql, mysql.", v => optionsTmp.Provider = Options.MapDatabaseProvider(v) },
            {"global", "Run all outstanding migrations in a single transaction, if possible.", v => optionsTmp.UseGlobalTransaction = v != null },
            {"table=", "Name of the table used to track migrations (default: Migrations).", v => optionsTmp.MigrationsTable = v },
            {"schema=", "Name of the schema to be used to track migrations (default: dbo for sqlserver, public for postgresql, mysql for mysql).", v => optionsTmp.MigrationsTableSchema = v },
            {"force", "Will rerun modified migrations.", v => optionsTmp.Force = v != null },
            {"version", "Print version number.", v => showVersion = v != null },
            { "count", "Print the number of outstanding migrations.", v => getCount = v != null },
        };

        try
        {
            optionSet.Parse(args);
            options.Output = Console.Out;

            if (!showHelp && !showVersion)
                optionsTmp.AssertValid();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine();
            showHelp = true;
        }

        if (showVersion)
        {
            Console.WriteLine($"Minimig --version {Migrator.GetVersion()}");
            Console.WriteLine();
            return Command.None;
        }

        if (showHelp)
        {
            ShowHelpMessage(optionSet);
            return Command.None;
        }

        return getCount ? Command.GetCount : Command.RunMigrations;
    }

    /// <summary>
    /// Show help message on the console
    /// </summary>
    /// <param name="optionSet">An OptionSet object instance that maps to the command line</param>
    private static void ShowHelpMessage(OptionSet optionSet)
    {
        Console.WriteLine("Usage: Minimig [OPTIONS]+");
        Console.WriteLine("  Runs all *.sql files in the directory --f=<folder> specified.");
        Console.WriteLine("  The databse connection can be specified using the following combinations:");
        Console.WriteLine("  -A connection string with the --connection along a --provider");
        Console.WriteLine("      if no provider is specified, the program defaults to sqlserver.");
        Console.WriteLine("  -Minimig can generate an integrated auth connection string using ");
        Console.WriteLine("      the --database and optional --server arguments, along a --provider.");
        Console.WriteLine();
        optionSet.WriteOptionDescriptions(Console.Out);
    }
}
