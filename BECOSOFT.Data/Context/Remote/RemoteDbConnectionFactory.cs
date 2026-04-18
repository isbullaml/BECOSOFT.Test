using BECOSOFT.Data.Models;
using BECOSOFT.Utilities.Exceptions;
using NLog;
using System;
using System.Data;
using System.Data.SqlClient;

namespace BECOSOFT.Data.Context.Remote {
    /// <summary>
    /// Factory for BaseDbConnections
    /// This is used for executing own SQL-scripts on remote servers
    /// </summary>
    internal class RemoteDbConnectionFactory : IRemoteDbConnectionFactory {
        /// <summary>
        /// The logger
        /// </summary>
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Creates a connection
        /// </summary>
        /// <param name="sqlInfo">The SQL-Info object</param>
        /// <returns>The connection</returns>
        public IDbConnection CreateConnection(SqlInfo sqlInfo) {
            try {
                var connectionString = GetConnectionString(sqlInfo);
                var connection = new SqlConnection(connectionString);
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

        /// <summary>
        /// Parses an SQL-info object into a connectionstring
        /// </summary>
        /// <param name="sqlInfo">The SQL-Info object</param>
        /// <returns>The connectionstring</returns>
        private static string GetConnectionString(SqlInfo sqlInfo) {
            var builder = new SqlConnectionStringBuilder {
                DataSource = sqlInfo.DataSource,
                Password = sqlInfo.Password,
                UserID = sqlInfo.UserID,
                ConnectTimeout = sqlInfo.ConnectTimeout,
                AsynchronousProcessing = sqlInfo.AsynchronousProcessing,
                Encrypt = sqlInfo.Encrypt,
                IntegratedSecurity = sqlInfo.IntegratedSecurity,
                TrustServerCertificate = sqlInfo.TrustServerCertificate
            };
            return builder.ToString();
        }
    }
}