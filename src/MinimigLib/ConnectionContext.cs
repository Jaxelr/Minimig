using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace Minimig
{
    public enum DatabaseProvider
    {
        SqlServer = 0,
        Postgres = 1
    }

    class ConnectionContext : IDisposable
    {
        readonly IDbConnection connection;
        IDbTransaction transaction;
        readonly ISqlStatements sql;
        readonly int timeout;

        internal bool IsPreview { get; }
        internal DatabaseProvider Provider { get; }
        internal string Database { get; }
        internal List<string> FilesInCurrentTransaction { get; } = new List<string>();
        
        internal Regex CommandSplitter => sql.CommandSplitter;
        internal bool HasPendingTransaction => transaction != null;

        internal ConnectionContext(Options options)
        {
            timeout = options.CommandTimeout;
            IsPreview = options.IsPreview;
            Provider = options.Provider;

            string connStr = options.GetConnectionString(Provider);

            switch (Provider)
            {
                case DatabaseProvider.SqlServer:
                    sql = new SqlServerStatements(options.GetMigrationsTable(), options.GetMigrationsTableSchema());
                    connection = new SqlConnection(connStr);
                    Database = new SqlConnectionStringBuilder(connStr).InitialCatalog;
                    break;
                default:
                    throw new NotImplementedException($"Unsupported DatabaseProvider {options.Provider}");
            }

            if (string.IsNullOrEmpty(Database))
                throw new Exception("No database was set in the connection string.");
        }

        public void Dispose() => connection?.Dispose();

        internal void Open() => connection.Open();

        internal void BeginTransaction() => transaction = connection.BeginTransaction();

        internal void Commit()
        {
            if (IsPreview)
                transaction.Rollback();
            else
                transaction.Commit();

            transaction = null;
            FilesInCurrentTransaction.Clear();
        }

        internal void Rollback()
        {
            transaction.Rollback();
            transaction = null;
            FilesInCurrentTransaction.Clear();
        }

        internal bool MigrationTableExists()
        {
            var cmd = connection.NewCommand(sql.DoesMigrationsTableExist);
            return (int)cmd.ExecuteScalar() == 1;
        }

        internal bool SchemaMigrationTableExists()
        {
            var cmd = connection.NewCommand(sql.DoesMigrationsTableExist);
            return (int)cmd.ExecuteScalar() == 1;
        }

        internal void CreateMigrationsTable()
        {
            var cmd = connection.NewCommand(sql.CreateMigrationsTable);
            cmd.ExecuteNonQuery();
        }

        internal AlreadyRan GetAlreadyRan()
        {
            var results = new List<MigrationRow>();
            var cmd = connection.NewCommand(sql.GetAlreadyRan);

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

        public int ExecuteCommand(string sql)
        {
            var cmd = connection.NewCommand(sql, transaction, timeout);
            return cmd.ExecuteNonQuery();

        }

        internal void InsertMigrationRecord(MigrationRow row)
        {
            var cmd = connection.NewCommand(sql.InsertMigration, transaction);

            cmd.AddParameter("Filename", row.Filename);
            cmd.AddParameter("Hash", row.Hash);
            cmd.AddParameter("ExecutionDate", row.ExecutionDate);
            cmd.AddParameter("Duration", row.Duration);

            cmd.ExecuteNonQuery();
        }

        internal void UpdateMigrationRecordHash(MigrationRow row)
        {
            var cmd = connection.NewCommand(sql.UpdateMigrationHash, transaction);

            cmd.AddParameter("Hash", row.Hash);
            cmd.AddParameter("ExecutionDate", row.ExecutionDate);
            cmd.AddParameter("Duration", row.Duration);
            cmd.AddParameter("Filename", row.Filename);

            int affected = cmd.ExecuteNonQuery();
            if (affected != 1)
                throw new Exception($"Failure updating the migration record. {affected} rows affected. Expected 1.");
        }

        internal void RenameMigration(Migration migration)
        {
            var cmd = connection.NewCommand(sql.RenameMigration, transaction);

            cmd.AddParameter("Filename", migration.Filename);
            cmd.AddParameter("Hash", migration.Hash);

            int affected = cmd.ExecuteNonQuery();
            if (affected != 1)
                throw new Exception($"Failure renaming the migration record. {affected} rows affected. Expected 1.");
        }
    }
}
