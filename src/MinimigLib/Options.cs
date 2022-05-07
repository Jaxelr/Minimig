using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Minimig;

public class Options
{
    public string Connection { get; set; }
    public string Database { get; set; }
    public string Server { get; set; }
    public int Timeout { get; set; } = 30;
    public string Folder { get; set; }
    public bool Preview { get; set; }
    public bool Global { get; set; }
    public string Table { get; set; }
    public string Schema { get; set; }
    public TextWriter Output { get; set; }
    public bool Force { get; set; }
    public bool Count { get; set; }
    public DatabaseProvider Provider { get; set; }

    /// <summary>
    /// Determine if the current options satisfy a plausible execution of a migration
    /// </summary>
    public void AssertValid()
    {
        if (!Directory.Exists(GetFolder()))
        {
            throw new Exception($"Invalid folder or possible unscaped \\; current folder argument is \"{Folder}\"");
        }

        if (string.IsNullOrEmpty(Connection) == string.IsNullOrEmpty(Database))
        {
            throw new Exception("Either a connection string or a database must be specified.");
        }

        if (!string.IsNullOrEmpty(Table))
        {
            if (!Regex.IsMatch(Table, "^[a-zA-Z]+$"))
                throw new Exception("Migrations table name can only contain letters A-Z.");
        }
    }

    /// <summary>
    /// Construct a ConnectionString  based on the current options for the provider given.
    /// </summary>
    /// <param name="provider"></param>
    /// <returns>A valid ConnectionString</returns>
    internal string GetConnectionString(DatabaseProvider provider)
    {
        if (!string.IsNullOrEmpty(Connection))
            return Connection;

        if (string.IsNullOrEmpty(Database))
            throw new Exception("No database assign to infer Connection String");

        if (provider is DatabaseProvider.sqlserver)
            return $"Persist Security Info=False;Integrated Security=true;Initial Catalog={Database};server={(string.IsNullOrEmpty(Server) ? "." : Server)}";
        else if (provider is DatabaseProvider.postgresql)
            return $"Server={(string.IsNullOrEmpty(Server) ? "localhost" : Server)};Port=5432;Database={Database};Integrated Security=true;";
        else if (provider is DatabaseProvider.mysql)
            return $"Server={(string.IsNullOrEmpty(Server) ? "localhost" : Server)};Port=3306;Database={Database};IntegratedSecurity=yes;";
        else
            throw new NotImplementedException($"Unsupported DatabaseProvider {Provider}");
    }

    /// <summary>
    /// Get the current migrations table
    /// </summary>
    /// <returns>The current migrations table</returns>
    internal string GetMigrationsTable() => string.IsNullOrEmpty(Table) ? "Migrations" : Table;

    /// <summary>
    /// Get the current migrations schema
    /// </summary>
    /// <returns>The current migrations schema</returns>
    internal string GetMigrationsTableSchema()
    {
        if (string.IsNullOrEmpty(Schema))
        {
            return Provider switch
            {
                DatabaseProvider.postgresql => "public",
                DatabaseProvider.mysql => "mysql",
                _ => "dbo",
            };
        }
        else
        {
            return Schema;
        }
    }

    /// <summary>
    /// Get the current migrations folder (defaults to current directory)
    /// </summary>
    /// <returns>A string with the directory requested</returns>
    internal string GetFolder() => string.IsNullOrEmpty(Folder) ? Directory.GetCurrentDirectory() : Folder;
}
