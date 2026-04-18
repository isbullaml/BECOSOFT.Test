using BECOSOFT.Data.Context;
using BECOSOFT.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace BECOSOFT.Data.Migrator {
    internal class MssqlDatabaseProvider<T> : DatabaseProvider<T> where T : Enum {
        private readonly IAdvisoryLockProvider _advisoryLockProvider;

        internal MssqlDatabaseProvider(IDbConnectionFactory connectionFactory,
                                       IBaseMigrationInformationProvider<T> informationProvider,
                                       IAdvisoryLockProvider advisoryLockProvider)
            : base(connectionFactory, informationProvider) {
            _advisoryLockProvider = advisoryLockProvider;
            InformationProvider = informationProvider;
            MaxDescriptionLength = 255;
        }

        public override IDbConnection Begin() {
            return _advisoryLockProvider.Begin();
        }

        public override void End() {
            _advisoryLockProvider.End();
        }

        public override int GetCurrentVersion(T type) {
            return GetCurrentVersion(type, Connection, null);
        }

        public override Dictionary<int, int> GetCurrentDynamicVersions(T type, TableConsumingMigrationTableInfo tableInfo) {
            return GetCurrentDynamicVersions(type, Connection, null, tableInfo);
        }

        public override void UpdateVersion(int oldVersion, int newVersion, string newDescription, T type) {
            UpdateVersion(oldVersion, newVersion, newDescription, type, Connection, null);
        }

        public override void UpdateDynamicVersion(int newVersion, T type, IReadOnlyList<int> entityIDs) {
            UpdateVersion(newVersion, type, entityIDs, Connection, null);
        }

        public override int EnsurePrerequisitesCreatedAndGetCurrentVersion(T type) {
            try {
                _advisoryLockProvider.AcquireAdvisoryLock();
                return EnsurePrerequisitesCreatedAndGetCurrentVersion(type, Connection, null);
            } finally {
                _advisoryLockProvider.ReleaseAdvisoryLock();
            }
        }

        protected override string CreateTableAndGetCurrentVersionSql() {
            var query = new StringBuilder();
            var schemaName = InformationProvider.SchemaName;
            var tableName = InformationProvider.TableName;
            var tableNameDynamic = InformationProvider.TableNameDynamic;
            query.AppendLine(" IF NOT EXISTS (select * from sys.schemas WHERE name ='{0}')", schemaName);
            query.AppendLine(" BEGIN ");
            query.AppendLine("     EXECUTE ('CREATE SCHEMA [{0}] AUTHORIZATION dbo');", schemaName);
            query.AppendLine(" END ");
            query.AppendLine(" IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('[{0}].[{1}]') AND type in (N'U')) ", schemaName, tableName);
            query.AppendLine(" BEGIN ");
            query.AppendLine("     EXECUTE (' CREATE TABLE [{0}].[{1}]( ", schemaName, tableName);
            query.AppendLine("         [Id] [int] IDENTITY(1,1) PRIMARY KEY NOT NULL, ");
            query.AppendLine("         [Type] [int] NOT NULL, ");
            query.AppendLine("         [Version] [bigint] NOT NULL, ");
            query.AppendLine("         [AppliedOn] [datetime] NOT NULL, ");
            query.AppendLine("         [Description] [nvarchar]({0}) NOT NULL ", MaxDescriptionLength);
            query.AppendLine("     ) ");
            query.AppendLine("     '); ");
            query.AppendLine(" END; ");
            query.AppendLine("  ");
            query.AppendLine(" IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('[{0}].[{1}]') AND type in (N'U')) ", schemaName, tableNameDynamic);
            query.AppendLine(" BEGIN ");
            query.AppendLine("     EXECUTE (' CREATE TABLE [{0}].[{1}]( ", schemaName, tableNameDynamic);
            query.AppendLine("         [EntityID] [int] NOT NULL, ");
            query.AppendLine(" 	    [MigrationType] [int] NOT NULL, ");
            query.AppendLine(" 	    [Version] [int] NOT NULL, ");
            query.AppendLine("         [AppliedOn] [datetime] NOT NULL, ");
            query.AppendLine("     ) ");
            query.AppendLine("     '); ");
            query.AppendLine("     IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'{0}.{1}') AND name = N'EntityID_MigrationType') ", schemaName, tableNameDynamic);
            query.AppendLine("     BEGIN ");
            query.AppendLine("         EXECUTE ('  ");
            query.AppendLine("         CREATE NONCLUSTERED INDEX [EntityID_MigrationType] ON {0}.{1} (EntityID, MigrationType) ", schemaName, tableNameDynamic);
            query.AppendLine("         WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY] ");
            query.AppendLine("         '); ");
            query.AppendLine("     END; ");
            query.AppendLine(" END; ");

            query.AppendLine(" DECLARE @VersionQuery NVARCHAR(MAX) ");
            query.AppendLine(" SET @VersionQuery = N' ");
            query.AppendLine("      SELECT TOP 1 v.[Version] ");
            query.AppendLine("      FROM [{0}].[{1}] v ", schemaName, tableName);
            query.AppendLine("      WHERE v.[Type] = @Type ");
            query.AppendLine("      ORDER BY v.[Id] DESC");
            query.AppendLine(" ;'");
            query.AppendLine(" EXEC sp_executeSQL @VersionQuery, N'@Type INT', @Type; ");

            return query.ToString();
        }

        protected override string GetCurrentDynamicVersionSql(TableConsumingMigrationTableInfo tableInfo) {
            var schemaName = InformationProvider.SchemaName;
            var tableName = InformationProvider.TableNameDynamic;
            var primaryKeyColumn = $"[{tableInfo.PrimaryKey.EscapedColumnName}]";
            var entityTableName = tableInfo.Table.FullTableName;
            return $@"
SELECT t.{primaryKeyColumn} AS EntityID, MAX(d.Version) AS [MaxVersion] 
FROM {entityTableName} t
LEFT JOIN [{schemaName}].[{tableName}] d ON d.EntityID = t.{primaryKeyColumn} AND d.[MigrationType] = @Type 
GROUP BY t.{primaryKeyColumn};
";
        }

        protected override string GetSetVersionSql() {
            var schemaName = InformationProvider.SchemaName;
            var tableName = InformationProvider.TableName;
            return $@"INSERT INTO [{schemaName}].[{tableName}] ([Type], [Version], [AppliedOn], [Description]) VALUES (@Type, @Version, GETDATE(), @Description);";
        }

        protected override string GetSetDynamicVersionSql() {
            var schemaName = InformationProvider.SchemaName;
            var tableName = InformationProvider.TableNameDynamic;
            return $@"INSERT INTO [{schemaName}].[{tableName}] ([EntityID], [MigrationType], [Version], [AppliedOn]) VALUES ({{0}}, @MigrationType, @Version, GETDATE());";
        }
    }
}