using BECOSOFT.Utilities.Converters;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;

namespace BECOSOFT.Data.Migrator {
    public abstract class BaseMigration<T> : IMigration<T> where T : Enum {
        public Type Type => GetType();
        protected virtual bool UseTransaction { get; set; }
        private readonly ILogger _logger;
        private List<int> _dynamicEntityIDs = new List<int>(0);

        public IDbConnection Connection { get; private set; }
        public IDbTransaction Transaction { get; set; }
        public T MigrationType { get; internal set; }
        public int Version { get; internal set; }
        public bool RunningFromLinked { get; private set; }

        public IReadOnlyList<int> DynamicEntityIDs {
            get => _dynamicEntityIDs.AsReadOnly();
            internal set => _dynamicEntityIDs = new List<int>(value);
        }

        public void SetRunningFromLinked(bool fromLinked) {
            RunningFromLinked = fromLinked;
        }

        protected BaseMigration(ILogger logger) {
            _logger = logger;
        }

        protected abstract void Upgrade();

        public abstract TableConsumingMigrationTableInfo GetTableInfo();

        protected void Execute(string sql, int? commandTimeout = null) {
            _logger.Trace(sql);
            using (var command = CreateCommand(sql, commandTimeout)) {
                command.ExecuteNonQuery();
            }
        }

        protected TValue ExecuteWithResult<TValue>(string sql, int? commandTimeout = null) where TValue : IConvertible {
            _logger.Trace(sql);
            using (var command = CreateCommand(sql, commandTimeout)) {
                return command.ExecuteScalar().To<TValue>();
            }
        }

        private IDbCommand CreateCommand(string sql, int? commandTimeout = null) {
            if (Connection == null) {
                throw new InvalidOperationException();
            }
            var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Transaction = Transaction;

            command.CommandTimeout = commandTimeout ?? 600;
            return command;
        }


        public void RunUpgrade(MigrationRunData data) {
            Connection = data.Connection;
            if (UseTransaction) {
                using (Transaction = Connection.BeginTransaction()) {
                    try {
                        Upgrade();
                        Transaction.Commit();
                    } catch (Exception ex) {
                        _logger.Error(ex);
                        Transaction?.Rollback();
                        throw;
                    } finally {
                        Transaction = null;
                    }
                }
            } else {
                Upgrade();
            }
        }

        public void SetVersionInfo(T type, int version) {
            MigrationType = type;
            Version = version;
        }

        internal void AddDynamicVersionEntry(int entityID) {
            if (entityID == 0) { return; }
            _dynamicEntityIDs.Add(entityID);
        }
    }
}