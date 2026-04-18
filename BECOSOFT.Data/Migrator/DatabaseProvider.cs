using BECOSOFT.Data.Context;
using BECOSOFT.Utilities.Converters;
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Extensions.Collections;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace BECOSOFT.Data.Migrator {
    internal abstract class DatabaseProvider<T> : IDatabaseProvider<T> where T : Enum {
        private readonly IDbConnectionFactory _connectionFactory;
        protected IBaseMigrationInformationProvider<T> InformationProvider;
        private IDbConnection _connection;

        protected IDbConnection Connection {
            get {
                SetConnection();
                return _connection;
            }
        }

        public TimeSpan LockTimeout { get; set; } = TimeSpan.FromSeconds(600);
        protected int MaxDescriptionLength { get; set; }

        protected DatabaseProvider(IDbConnectionFactory connectionFactory, 
                                   IBaseMigrationInformationProvider<T> informationProvider) {
            _connectionFactory = connectionFactory;
            InformationProvider = informationProvider;
        }

        private void SetConnection() {
            if (_connection == null) {
                _connection = _connectionFactory.CreateConnection();
            }
            if (_connection.State != ConnectionState.Open) {
                _connection.Open();
            }
        }

        public abstract IDbConnection Begin();
        public abstract void End();
        public abstract int GetCurrentVersion(T type);
        public abstract Dictionary<int, int> GetCurrentDynamicVersions(T type, TableConsumingMigrationTableInfo tableInfo);
        public abstract void UpdateVersion(int oldVersion, int newVersion, string newDescription, T type);
        public abstract void UpdateDynamicVersion(int newVersion, T type, IReadOnlyList<int> entityIDs);
        public abstract int EnsurePrerequisitesCreatedAndGetCurrentVersion(T type);

        protected virtual int EnsurePrerequisitesCreatedAndGetCurrentVersion(T type, IDbConnection connection,
                                                                             DbTransaction transaction) {
            return GetCurrentVersion(type, connection, transaction);
        }

        protected virtual int GetCurrentVersion(T type, IDbConnection connection, DbTransaction transaction) {
            var version = 0;
            using (var command = connection.CreateCommand()) {
                command.CommandText = CreateTableAndGetCurrentVersionSql();

                var typeParam = command.CreateParameter();
                typeParam.ParameterName = "Type";
                typeParam.Value = type;
                command.Parameters.Add(typeParam);

                command.Transaction = transaction;
                var result = command.ExecuteScalar();

                if (result == null) {
                    return version;
                }
                try {
                    version = result.To<int>();
                } catch {
                    throw new MigrationException("Database Provider returns a value for the current version which isn't an integer");
                }
            }

            return version;
        }

        protected virtual Dictionary<int, int> GetCurrentDynamicVersions(T type, IDbConnection connection, DbTransaction transaction, TableConsumingMigrationTableInfo tableInfo) {
            using (var command = connection.CreateCommand()) {
                command.CommandText = GetCurrentDynamicVersionSql(tableInfo);

                var typeParam = command.CreateParameter();
                typeParam.ParameterName = "Type";
                typeParam.Value = type;
                command.Parameters.Add(typeParam);

                command.Transaction = transaction;
                var result = new Dictionary<int, int>();
                using (var reader = command.ExecuteReader()) {

                    while (reader.Read()) {
                        var entityID = reader.GetValue(0).To<int>();
                        int version;
                        try {
                            version = reader.GetValue(1).To<int>();
                        } catch {
                            throw new MigrationException("Database Provider returns a value for the current version which isn't a long");
                        }
                        result[entityID] = version;
                    }
                }
                return result;
            }
        }



        protected virtual void UpdateVersion(int oldVersion, int newVersion, string newDescription, T type,
                                             IDbConnection connection, DbTransaction transaction) {
            if (!InformationProvider.ShouldRegisterVersion(type)) { return; }
            if (MaxDescriptionLength > 0) {
                newDescription = newDescription.Truncate(MaxDescriptionLength);
            }

            using (var command = connection.CreateCommand()) {
                command.Transaction = transaction;
                command.CommandText = GetSetVersionSql();

                var typeParam = command.CreateParameter();
                typeParam.ParameterName = "Type";
                typeParam.Value = type;
                command.Parameters.Add(typeParam);

                var versionParam = command.CreateParameter();
                versionParam.ParameterName = "Version";
                versionParam.Value = newVersion;
                command.Parameters.Add(versionParam);

                var oldVersionParam = command.CreateParameter();
                oldVersionParam.ParameterName = "OldVersion";
                oldVersionParam.Value = oldVersion;
                command.Parameters.Add(oldVersionParam);

                var nameParam = command.CreateParameter();
                nameParam.ParameterName = "Description";
                nameParam.Value = newDescription;
                command.Parameters.Add(nameParam);

                command.ExecuteNonQuery();
            }
        }

        protected virtual void UpdateVersion(int newVersion, T type, IReadOnlyList<int> entityIDs,
                                             IDbConnection connection, DbTransaction transaction) {
            if (!InformationProvider.ShouldRegisterVersion(type)) { return; }
            if (entityIDs.IsEmpty()) { return; }
            using (var command = connection.CreateCommand()) {
                command.Transaction = transaction;
                var query = GetSetDynamicVersionSql();
                var queryBuilder = new StringBuilder();

                var typeParam = command.CreateParameter();
                typeParam.ParameterName = "MigrationType";
                typeParam.Value = type;
                command.Parameters.Add(typeParam);

                var versionParam = command.CreateParameter();
                versionParam.ParameterName = "Version";
                versionParam.Value = newVersion;
                command.Parameters.Add(versionParam);

                foreach (var entityID in entityIDs) {
                    // query contains {0} format specifier
                    queryBuilder.AppendLine(query, entityID);
                }
                command.CommandText = queryBuilder.ToString();

                command.ExecuteNonQuery();
            }
        }

        protected abstract string CreateTableAndGetCurrentVersionSql();
        protected abstract string GetCurrentDynamicVersionSql(TableConsumingMigrationTableInfo tableInfo);
        protected abstract string GetSetVersionSql();
        protected abstract string GetSetDynamicVersionSql();
    }
}