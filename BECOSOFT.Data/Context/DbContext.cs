using BECOSOFT.Data.Extensions;
using BECOSOFT.Data.Models;
using BECOSOFT.Data.Query;
using NLog;
using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;

namespace BECOSOFT.Data.Context {

    internal abstract class DbContext : IDbContext {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly IBulkCopyHelper _bulkCopyHelper;

        /// <summary>
        /// Connection to the database
        /// </summary>
        public IDbConnection Connection { get; }

        protected DbContext(IDbConnection dbConnection, IBulkCopyHelper bulkCopyHelper) {
            Connection = dbConnection;
            _bulkCopyHelper = bulkCopyHelper;
        }

        public int ExecuteNonQuery(DatabaseCommand databaseCommand) {
            SqlCommand command = null;
            try {
                command = SetupCommand(databaseCommand);
                LogQuery(databaseCommand);
                HandleBulkCopyTempTables(databaseCommand, command.Connection);
                var result = command.ExecuteNonQuery();
                return result;
            } catch (Exception e) {
                LogQueryForException(e, databaseCommand);
                throw;
            } finally {
                CleanupCommand(databaseCommand, command);
            }
        }

        public DbDataReader ExecuteReader(DatabaseCommand databaseCommand) {
            SqlCommand command = null;
            try {
                command = SetupCommand(databaseCommand);
                LogQuery(databaseCommand);
                HandleBulkCopyTempTables(databaseCommand, command.Connection);
                var result = command.ExecuteReader();
                return result;
            } catch (Exception e) {
                LogQueryForException(e, databaseCommand);
                throw;
            } finally {
                CleanupCommand(databaseCommand, command);
            }
        }

        public DbDataReader ExecuteReader(DatabaseCommand databaseCommand, CommandBehavior behavior) {
            SqlCommand command = null;
            try {
                command = SetupCommand(databaseCommand);
                LogQuery(databaseCommand);
                HandleBulkCopyTempTables(databaseCommand, command.Connection);
                var result = command.ExecuteReader(behavior);
                return result;
            } catch (Exception e) {
                LogQueryForException(e, databaseCommand);
                throw;
            } finally {
                CleanupCommand(databaseCommand, command);
            }
        }

        public object ExecuteScalar(DatabaseCommand databaseCommand) {
            SqlCommand command = null;
            try {
                command = SetupCommand(databaseCommand);
                LogQuery(databaseCommand);
                HandleBulkCopyTempTables(databaseCommand, command.Connection);
                var result = command.ExecuteScalar();
                return result;
            } catch (Exception e) {
                LogQueryForException(e, databaseCommand);
                throw;
            } finally {
                CleanupCommand(databaseCommand, command);
            }
        }

        private void HandleBulkCopyTempTables(DatabaseCommand databaseCommand, SqlConnection connection) {
            _bulkCopyHelper.Handle(databaseCommand.BulkCopyTempTables, connection, ExecuteNonQuery);
        }

        private SqlCommand SetupCommand(DatabaseCommand databaseCommand) {
            var command = (SqlCommand)Connection.CreateCommand();
            command.Connection.FireInfoMessageEventOnUserErrors = databaseCommand.FireInfoMessageEventOnUserErrors;
            command.Connection.InfoMessage += databaseCommand.InfoMessageHandler;
            command.Transaction = databaseCommand.Transaction;
            command.CommandText = databaseCommand.CommandText;
            command.CommandTimeout = databaseCommand.CommandTimeout ?? DatabaseCommand.DefaultQueryTimeout;
            command.CommandType = databaseCommand.CommandType;
            command.UpdatedRowSource = databaseCommand.UpdatedRowSource;
            var clonedParameters = databaseCommand.Parameters
                                                  .Select(p => p.GetClone())
                                                  .ToArray();
            command.Parameters.AddRange(clonedParameters);
            return command;
        }

        private void CleanupCommand(DatabaseCommand databaseCommand, SqlCommand command) {
            if (command == null) { return;}
            command.Connection.InfoMessage -= databaseCommand.InfoMessageHandler;
            command.Dispose();
        }

        /// <summary>
        /// Logs a query to the <see cref="LogLevel.Trace"/> log, if it is enabled
        /// </summary>
        private void LogQuery(DatabaseCommand command) {
            if (!Logger.IsTraceEnabled) {
                return;
            }
            Logger.Trace(command.GetParsedCommandTextStringForLogging());
        }
        
        private void LogQueryForException(Exception e, DatabaseCommand command) {
            Logger.Error("Exception occured while running the following query: \n{0}\n\nException: \n{1}", command.GetParsedCommandTextStringForLogging(), e);
        }

        private void Dispose(bool disposing) {
            if (disposing) {
                Connection?.Dispose();
            }
        }

        /// <inheritdoc />
        public void Dispose() {
            Dispose(true);
        }
    }
}
