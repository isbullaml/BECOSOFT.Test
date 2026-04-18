using BECOSOFT.Utilities.Exceptions;
using NLog;
using System;
using System.Data;
using System.Data.SqlClient;

namespace BECOSOFT.Data.Context {
    /// <summary>
    /// Connection factory for SQL
    /// </summary>
    internal class SqlDbConnectionFactory : IDbConnectionFactory {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        public string Connection { get; }

        internal SqlDbConnectionFactory(string connection) {
            Connection = connection;
        }

        /// <inheritdoc />
        public IDbConnection CreateConnection() {
            try {
                var connection = new SqlConnection(Connection);
                connection.Open();
                return connection;
            } catch (InvalidOperationException ex) {
                Logger.Error(ex, Resources.Error_Connection_Invalid);
                throw new DbConnectionException(Resources.Error_Connection_Invalid, ex);
            } catch (SqlException ex) {
                Logger.Error(ex, Resources.Error_Connection);
                throw new DbConnectionException(Resources.Error_Connection, ex);
            }
        }
    }
}