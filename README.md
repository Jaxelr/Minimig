# Minimig

*** This library is a fork from Mayflower.net which seems to be [inactive](https://github.com/bretcope/Mayflower.NET)

Minimig is a simple, forward-only, db migrator for SQL Server.

## Usage

### Creating Migrations

A migration is just plain T-SQL saved in a .sql file. Individual commands are separated with the `GO` keyword, just like when using [SSMS](https://msdn.microsoft.com/en-us/library/mt238290.aspx). For example:

```sql
CREATE TABLE One
(
  Id int not null identity(1,1),
  Name nvarchar(50) not null,
  
  constraint PK_One primary key clustered (Id)
)
GO

INSERT INTO One (Name) VALUES ('Wystan')
GO
```

> Migrations are run in a transaction by default, which allows them to be rolled back if any command fails. You can disable this transaction for a specific migration by beginning the file with `-- no transaction --`.

We recommend prefixing migration file names with a zero-padded number so that the migrations are listed in chronological order. For example, a directory of migrations might look like:

``` cmd
0001 - Add Users table.sql
0002 - Add Posts.sql
0003 - Insert default users.sql
0004 - Add auth columns to Users.sql
...
```

### Running Migrations

#### Command Line

The easiest way to run migrations is with `mig` command. You obtain it from the Downloads section of [GitHub releases](https://github.com/jaxelr/Minimig/releases) or installable via [nuget](https://www.nuget.org/packages/Minimig/). It requires .NET Framework 4.5.2 or above.

Typical usage is simply:

``` cmd
mig --folder="c:\path\to\migrations" --connection="Persist Security Info=False;Integrated Security=true;Initial Catalog=MyDatabase;server=localhost"
```

If you use integrated auth, you can use the `--database` and `--server` arguments instead of supplying a connection string (server defaults to "localhost").

``` cmd
mig --folder="c:\path\to\migrations" --database=MyLocalDatabase
```

Use `mig --help` to show the complete set of options:

``` cmd
Usage: mig [OPTIONS]+
  Runs all *.sql files in the directory --dir=<directory>.
  The databse connection can be specified using a full connection string
  with --connection, or Minimig can generate an integrated auth connection
  string using the --database and optional --server arguments.

  -h, --help                 Shows this help message.
  -c, --connection=VALUE     A SQL Server connection string. For integrated
                               auth, you can use --database and --server
                               instead.
  -d, --database=VALUE       Generates an integrated auth connection string
                               for the specified database.
  -s, --server=VALUE         Generates an integrated auth connection string
                               with the specified server (default: localhost).
  -f, --folder=VALUE         The folder containing your .sql migration files
                               (defaults to current working directory).
      --timeout=VALUE        Command timeout duration in seconds (default: 30)
      --preview              Run outstanding migrations, but roll them back.
      --global               Run all outstanding migrations in a single
                               transaction, if possible.
      --table=VALUE          Name of the table used to track migrations
                               (default: Migrations)
      --force                Will rerun modified migrations.
      --version              Print version number.
      --count                Print the number of outstanding migrations.
```

#### Programmatic

If you'd prefer, Minimig can be called via code. Minimig.dll is included in the [nuget package](https://www.nuget.org/packages/Minimig/).

```csharp
var options = new Options
{
    Database = "MyLocalDatabase",
    MigrationsFolder = @"c:\path\to\migrations",
    Output = Console.Out,
};

var result = Migrator.RunOutstandingMigrations(options);
// result.Success indicates success or failure
```

The `Options` class has equivalent properties to most of the command line options.

### Reverting Migrations

Many migration systems have a notion of reversing a migration or "downgrading" in some sense. Mayflower has no such concept. If you want to reverse the effects of one migration, then you write a new migration to do so. Mayflower lives in a forward-only world.

## License

Minimig is available under the [MIT License](https://github.com/bretcope/Mayflower.NET/blob/master/LICENSE.MIT).