# Minimig

Minimig is a forward-only, database migrator for SQL Server, MySql and Postgres.

## Build Status

| Appveyor  | Branch | Coverage |
| :---:     | :---: | :--: |
| [![Build status][build-master-img]][build-master] | master | [![CodeCov][codecov-master-img]][codecov-master] |

## Packages

Package | NuGet (Stable) | MyGet (Prerelease)
| :--- | :---: | :---: |
| Minimig | [![NuGet][nuget-mig-img]][nuget-mig] | [![MyGet][myget-mig-img]][myget-mig] |

## Installation

Easiest installation is via dotnet global tools running `dotnet tool install --global Minimig` from your terminal.

To install the prerelease version (from myget), you can add the option `--add-source https://www.myget.org/F/minimig/api/v3/index.json`

## Usage

### Creating Migrations

A migration is just plain T-SQL saved in a .sql file. Individual commands are separated with the `GO` keyword for sql server, just like when using [SSMS](https://msdn.microsoft.com/en-us/library/mt238290.aspx). 

> In the case of mysql and postgres, the separator used is the semicolon(;) since `GO` is not supported on those databases

For example:

```sql
CREATE TABLE One
(
  Id int not null identity(1,1),
  Name nvarchar(50) not null,
  
  constraint PK_One primary key clustered (Id)
)
GO

INSERT INTO One (Name) VALUES ('Person')
GO
```

> Migrations are executed as a transaction by default, which allows them to be rolled back if any command fails. You can disable this transaction for a specific migration by beginning the file with `-- no transaction --`.

We recommend prefixing migration file names with a zero-padded number so that the migrations are listed in chronological order. For example, a directory of migrations might look like:

``` cmd
0001 - Add Users table.sql
0002 - Add Posts.sql
0003 - Insert default users.sql
0004 - Add auth columns to Users.sql
0005 - Drop tables One To Three.sql
...
```

### Running Migrations

#### Command Line

To run migrations the typical usage is:

``` cmd
mig --folder="c:\path\to\migrations" --connection="Persist Security Info=False;Integrated Security=true;Initial Catalog=MyDatabase;server=localhost"
```
> By default the provider used is Sql Server

If you use integrated auth, you can use the `--database` and `--server` arguments instead of supplying a connection string (server defaults to "localhost").

``` cmd
mig --folder="c:\\path\\to\\migrations" --database=MyLocalDatabase
```

Use `mig --help` to show the complete set of options:

``` cmd
Usage: Minimig [OPTIONS]+
  Runs all *.sql files in the directory --dir=<directory>.
  The databse connection can be specified using a full connection string with --connection,
  or Minimig can generate an integrated auth connection string using the --database and
  optional --server arguments.

  -h, --help                 Shows this help message.
  -c, --connection=VALUE     A connection string (can be Postgresql or 
                               SqlServer or MySql). For integrated auth, you 
                               can use --database and --server instead.
  -d, --database=VALUE       Generates an integrated auth connection string 
                               for the specified database.
  -s, --server=VALUE         Generates an integrated auth connection string 
                               with the specified server (default: localhost).
  -f, --folder=VALUE         The folder containing your .sql migration files 
                               (defaults to current working directory).
      --timeout=VALUE        Command timeout duration in seconds (default: 30)
      --preview              Run outstanding migrations, but roll them back.
  -p, --provider=VALUE       Use a specific database provider options: 
                               sqlserver (default), postgres, mysql
      --global               Run all outstanding migrations in a single 
                               transaction, if possible.
      --table=VALUE          Name of the table used to track migrations 
                               (default: Migrations)
      --schema=VALUE         Name of the schema to be used to track 
                               migrations (default: dbo for sqlserver, public 
                               for postgres, mysql for mysql)
      --force                Will rerun modified migrations.
      --version              Print version number.
      --count                Print the number of outstanding migrations.
```

### Reverting Migrations

Minimig has no such concept. It is a forward-only tool. This is done to keep a minimalistic approach.

### Uninstallation

For uninstallation execute `dotnet tool uninstall --global Minimig` from terminal.

## Credit

This library started as a fork from Mayflower.net which is [inactive](https://github.com/bretcope/Mayflower.NET)

## License

Minimig is available under the [MIT License](https://github.com/Jaxelr/Minimig/blob/master/LICENSE)

## Logo license

Minimig's icon was created by [Freepik](https://www.freepik.com/) under [Creative Commons license 3.0](http://creativecommons.org/licenses/by/3.0/)

[build-master-img]: https://ci.appveyor.com/api/projects/status/t7e2n08lgqb4jvui/branch/master?svg=true
[build-master]: https://ci.appveyor.com/project/Jaxelr/minimig/branch/master
[nuget-mig-img]: https://img.shields.io/nuget/v/Minimig.svg
[nuget-mig]: https://www.nuget.org/packages/Minimig
[myget-mig-img]: https://img.shields.io/myget/minimig/v/Minimig.svg
[myget-mig]: https://www.myget.org/feed/minimig/package/nuget/Minimig
[codecov-master-img]: https://codecov.io/gh/Jaxelr/Minimig/branch/master/graph/badge.svg
[codecov-master]: https://codecov.io/gh/Jaxelr/Minimig/branch/master
