using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using Npgsql;

namespace Minimig
{
    public enum DatabaseProvider
    {
        sqlserver = 0,
        postgres = 1,
        mysql = 2
    }

    internal class ConnectionContext : IDisposable
    {
        internal readonly IDbConnection Connection;
        private IDbTransaction transaction;
        private readonly ISqlStatements sql;
        private readonly int timeout;

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
                case DatabaseProvider.sqlserver:
                    sql = new SqlServerStatements(options.GetMigrationsTable(), options.GetMigrationsTableSchema());
                    Connection = new SqlConnection(connStr);
                    Database = new SqlConnectionStringBuilder(connStr).InitialCatalog;
                    break;

                case DatabaseProvider.postgres:
                    sql = new PostgreSqlStatements(options.GetMigrationsTable(), options.GetMigrationsTableSchema());
                    Connection = new NpgsqlConnection(connStr);
                    Database = new NpgsqlConnectionStringBuilder(connStr).Database;
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

        internal void Open() => Connection.Open();

        internal void BeginTransaction() => transaction = Connection.BeginTransaction();

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

        internal void Rollback()
        {
            transaction.Rollback();
            transaction = null;
            FilesInCurrentTransaction.Clear();
        }

        internal bool MigrationTableExists()
        {
            object result = Connection
                    .NewCommand(sql.DoesMigrationsTableExist)
                    .ExecuteScalar();

            //Depending on the provider the result might be type long instead of int
            if (result is long longScalar)
            {
                return longScalar == 1;
            }

            return (int) result == 1;
        }

        internal bool SchemaMigrationExists()
        {
            object result = Connection
                    .NewCommand(sql.DoesSchemaMigrationExist)
                    .ExecuteScalar();

            //Depending on the provider the result might be type long instead of int
            if (result is long longScalar)
            {
                return longScalar == 1;
            }

            return (int) result == 1;
        }

        internal void CreateMigrationsTable()
        {
            var cmd = Connection.NewCommand(sql.CreateMigrationsTable);
            cmd.ExecuteNonQuery();
        }

        internal void DropMigrationsTable()
        {
            var cmd = Connection.NewCommand(sql.DropMigrationsTable);
            cmd.ExecuteNonQuery();
        }

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

        public int ExecuteCommand(string sql)
        {
            var cmd = Connection.NewCommand(sql, transaction, timeout);
            return cmd.ExecuteNonQuery();
        }

        internal void InsertMigrationRecord(MigrationRow row)
        {
            var cmd = Connection.NewCommand(sql.InsertMigration, transaction);

            cmd.AddParameter("Filename", row.Filename);
            cmd.AddParameter("Hash", row.Hash);
            cmd.AddParameter("ExecutionDate", row.ExecutionDate);
            cmd.AddParameter("Duration", row.Duration);

            cmd.ExecuteNonQuery();
        }

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
}
