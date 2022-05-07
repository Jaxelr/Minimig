using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;
using Npgsql;

namespace Minimig;

internal class ConnectionContext : IDisposable
{
    internal readonly IDbConnection Connection;
    private IDbTransaction transaction;
    private readonly ISqlStatements sql;
    private readonly int timeout;

    internal bool IsPreview { get; }
    internal DatabaseProvider Provider { get; }
    internal string Database { get; }
    internal List<string> FilesInCurrentTransaction { get; } = [];

    internal Regex CommandSplitter => sql.CommandSplitter;
    internal bool HasPendingTransaction => transaction != null;

    /// <summary>
    /// Generate a new Connection Context with the options provided
    /// </summary>
    /// <param name="options">Minimig custom option arguments</param>
    internal ConnectionContext(Options options)
    {
        timeout = options.Timeout;
        IsPreview = options.Preview;
        Provider = options.Provider;

        string connStr = options.GetConnectionString(Provider);

        switch (Provider)
        {
            case DatabaseProvider.sqlserver:
                sql = new SqlServerStatements(options.GetMigrationsTable(), options.GetMigrationsTableSchema());
                Connection = new SqlConnection(connStr);
                Database = new SqlConnectionStringBuilder(connStr).InitialCatalog;
                break;

            case DatabaseProvider.postgresql:
                sql = new PostgreSqlStatements(options.GetMigrationsTable(), options.GetMigrationsTableSchema());
                Connection = new NpgsqlConnection(connStr);
                Database = new NpgsqlConnectionStringBuilder(connStr).Database;
                break;
            case DatabaseProvider.mysql:
                sql = new MySqlStatements(options.GetMigrationsTable(), options.GetMigrationsTableSchema());
                Connection = new MySqlConnection(connStr);
                Database = new MySqlConnectionStringBuilder(connStr).Database;
                break;
        }

        if (string.IsNullOrEmpty(Database))
            throw new Exception("No database was set in the connection string.");
    }

    public void Dispose()
    {
        Connection?.Dispose();
        transaction?.Dispose();
    }

    /// <summary>
    /// Open database connection
    /// </summary>
    internal void Open() => Connection.Open();

    /// <summary>
    /// Begin transaction processing
    /// </summary>
    internal void BeginTransaction() => transaction = Connection.BeginTransaction();

    /// <summary>
    /// Complete transaction processing
    /// </summary>
    internal void Commit()
    {
        if (IsPreview)
            transaction.Rollback();
        else if (transaction is IDbTransaction)
            transaction.Commit();
        else
            throw new InvalidOperationException("Cannot Commit a transaction without beginning");

        transaction = null;
        FilesInCurrentTransaction.Clear();
    }

    /// <summary>
    /// Rollback transaction processing
    /// </summary>
    internal void Rollback()
    {
        transaction.Rollback();
        transaction = null;
        FilesInCurrentTransaction.Clear();
    }

    /// <summary>
    /// Verify if the existing migration table exists
    /// </summary>
    /// <returns>A bool indicating if it exists or not</returns>
    internal bool MigrationTableExists()
    {
        object result = Connection
                .NewCommand(sql.DoesMigrationsTableExist)
                .ExecuteScalar();

        //Depending on the provider the result might be type long instead of int
        if (result is long longScalar)
            return longScalar == 1;

        return (int) result == 1;
    }

    /// <summary>
    /// Verify if the existing migration schema exists
    /// </summary>
    /// <returns>A bool indicating if it exists or not</returns>
    internal bool SchemaMigrationExists()
    {
        object result = Connection
                .NewCommand(sql.DoesSchemaMigrationExist)
                .ExecuteScalar();

        //Depending on the provider the result might be type long instead of int
        if (result is long longScalar)
            return longScalar == 1;

        return (int) result == 1;
    }

    /// <summary>
    /// Create migration table on the server
    /// </summary>
    internal void CreateMigrationsTable()
    {
        var cmd = Connection.NewCommand(sql.CreateMigrationsTable);
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Drop migration table on the server
    /// </summary>
    internal void DropMigrationsTable()
    {
        var cmd = Connection.NewCommand(sql.DropMigrationsTable);
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Get existing migrations from the server
    /// </summary>
    /// <returns>An AlreadyRan object with the existing migrations</returns>
    internal AlreadyRan GetAlreadyRan()
    {
        var results = new List<MigrationRow>();
        var cmd = Connection.NewCommand(sql.GetAlreadyRan);

        using (var rdr = cmd.ExecuteReader())
        {
            while (rdr.Read())
            {
                var row = new MigrationRow
                {
                    Id = rdr.GetInt32(0),
                    Filename = rdr.GetString(1),
                    Hash = rdr.GetString(2),
                    ExecutionDate = rdr.GetDateTime(3),
                    Duration = rdr.GetInt32(4)
                };

                results.Add(row);
            }
        }

        return new AlreadyRan(results);
    }

    /// <summary>
    /// Execute a sql command based on the string provided
    /// </summary>
    /// <param name="sql">Sql script to execute.</param>
    /// <returns>An integer indicating the result of the command</returns>
    public int ExecuteCommand(string sql)
    {
        var cmd = Connection.NewCommand(sql, transaction, timeout);
        return cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Inserts the migration record into the migration table
    /// </summary>
    /// <param name="row">A migration row description</param>
    internal void InsertMigrationRecord(MigrationRow row)
    {
        var cmd = Connection.NewCommand(sql.InsertMigration, transaction);

        cmd.AddParameter("Filename", row.Filename);
        cmd.AddParameter("Hash", row.Hash);
        cmd.AddParameter("ExecutionDate", row.ExecutionDate);
        cmd.AddParameter("Duration", row.Duration);

        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Update the hash of the migration record
    /// </summary>
    /// <param name="row">A migration row description</param>
    internal void UpdateMigrationRecordHash(MigrationRow row)
    {
        var cmd = Connection.NewCommand(sql.UpdateMigrationHash, transaction);

        cmd.AddParameter("Hash", row.Hash);
        cmd.AddParameter("ExecutionDate", row.ExecutionDate);
        cmd.AddParameter("Duration", row.Duration);
        cmd.AddParameter("Filename", row.Filename);

        int affected = cmd.ExecuteNonQuery();

        if (affected != 1)
            throw new Exception($"Failure updating the migration record. {affected} rows affected. Expected 1.");
    }

    /// <summary>
    /// Rename an existing migration
    /// </summary>
    /// <param name="migration">A migration object</param>
    internal void RenameMigration(Migration migration)
    {
        var cmd = Connection.NewCommand(sql.RenameMigration, transaction);

        cmd.AddParameter("Filename", migration.Filename);
        cmd.AddParameter("Hash", migration.Hash);

        int affected = cmd.ExecuteNonQuery();

        if (affected != 1)
            throw new Exception($"Failure renaming the migration record. {affected} rows affected. Expected 1.");
    }
}
