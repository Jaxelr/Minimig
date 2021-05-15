using System.Data;

namespace Minimig
{
    internal static class SqlExtensions
    {
        /// <summary>
        /// Abstraction of the logic to create a command to a connection
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns>Returns and IDbCommand initialized with the parameters provided</returns>
        internal static IDbCommand NewCommand(this IDbConnection conn, string sql, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.Transaction = transaction;

            if (commandTimeout.HasValue)
                cmd.CommandTimeout = commandTimeout.Value;

            return cmd;
        }

        /// <summary>
        /// Abstraction of the logic to create an IDbDataParameter and attach it to the IDbCommand
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        internal static void AddParameter(this IDbCommand cmd, string name, object value)
        {
            var param = cmd.CreateParameter();
            param.ParameterName = name;
            param.Value = value;
            cmd.Parameters.Add(param);
        }
    }
}
