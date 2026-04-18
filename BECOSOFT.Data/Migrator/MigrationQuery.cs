using BECOSOFT.Data.Context;
using BECOSOFT.Data.Converters;
using BECOSOFT.Data.Exceptions;
using BECOSOFT.Data.Helpers;
using BECOSOFT.Data.Models;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Utilities.Collections;
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Extensions.Collections;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace BECOSOFT.Data.Migrator {
    public static class MigrationQuery {
        public static string AddTable<T>(string definition, string tablePart = null) where T : IEntity {
            Check.IsValidTableConsuming<T>(tablePart);
            var entityTypeInfo = EntityConverter.GetEntityTypeInfo(typeof(T));
            return AddTable(entityTypeInfo.TableDefinition.Schema, entityTypeInfo.TableDefinition.TableName, definition, tablePart);
        }

        public static string AddTable(Schema schema, string table, string definition, string tablePart = null) {
            var sqlSchema = schema.ToSql();
            table = TableHelper.Clean(string.Format(table, tablePart));
            if (table.Contains("{0}")) {
                throw new ArgumentException(Resources.Error_MissingTablePart);
            }
            return $@"
IF NOT (EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '{sqlSchema}' AND TABLE_NAME = '{table}'))
BEGIN
    {definition}
END
";
        }

        public static string AddView<T>(string definition, string tablePart = null) where T : IEntity {
            Check.IsValidTableConsuming<T>(tablePart);
            var entityTypeInfo = EntityConverter.GetEntityTypeInfo(typeof(T));
            return AddView(entityTypeInfo.TableDefinition.Schema, entityTypeInfo.TableDefinition.TableName, definition, tablePart);
        }

        public static string AddView(Schema schema, string view, string definition, string tablePart = null) {
            var sqlSchema = schema.ToSql();
            view = TableHelper.Clean(string.Format(view, tablePart));
            if (view.Contains("{0}")) {
                throw new ArgumentException(Resources.Error_MissingTablePart);
            }
            var definitionBuilder = new StringBuilder(definition);
            definitionBuilder.Replace("CREATE VIEW", "VIEW").Replace("create view", "VIEW");
            definitionBuilder.Replace("ALTER VIEW", "VIEW").Replace("alter view", "VIEW");
            definitionBuilder.Replace("'", "''");
            var correctedDefinition = definitionBuilder.ToString();

            return $@"
IF (EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.VIEWS WHERE TABLE_SCHEMA = '{sqlSchema}' AND TABLE_NAME = '{view}'))
BEGIN
    EXEC ('ALTER {correctedDefinition}')
END
ELSE
BEGIN
    EXEC ('CREATE {correctedDefinition}')
END
";
        }

        public static string AddColumn<T>(Expression<Func<T, object>> prop, string definition, string tablePart = null, string extraQuery = null) {
            var column = Entity.GetColumn(prop);
            return AddColumn<T>(column, definition, tablePart, extraQuery);
        }

        public static string AddColumn<T>(string column, string definition, string tablePart = null, string extraQuery = null) {
            Check.IsValidTableConsuming<T>(tablePart);
            var entityTypeInfo = EntityConverter.GetEntityTypeInfo(typeof(T));
            return AddColumn(entityTypeInfo.TableDefinition.Schema, entityTypeInfo.TableDefinition.TableName, column, definition, tablePart, extraQuery);
        }

        public static string AddColumn(Schema schema, string table, string column, string definition, string tablePart = null, string extraQuery = null) {
            var fullTable = TableHelper.GetCombined(schema, table, tablePart);
            if (fullTable.Contains("{0}")) {
                throw new ArgumentException(Resources.Error_MissingTablePart);
            }
            var cleanedColumn = ColumnHelper.Clean(column);
            return $@"
IF OBJECTPROPERTY(OBJECT_ID(N'{fullTable}'), 'IsTable') = 1 AND NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'{cleanedColumn}' AND Object_ID = Object_ID(N'{fullTable}'))
BEGIN
    ALTER TABLE {fullTable} ADD [{cleanedColumn}] {definition}

    {extraQuery}
END
";
        }

        public static string RenameColumn<T>(string column, string newName, string tablePart = null) {
            Check.IsValidTableConsuming<T>(tablePart);
            var entityTypeInfo = EntityConverter.GetEntityTypeInfo(typeof(T));
            return RenameColumn(entityTypeInfo.TableDefinition.Schema, entityTypeInfo.TableDefinition.TableName, column, newName, tablePart);
        }

        public static string RenameColumn(Schema schema, string table, string column, string newName, string tablePart = null) {
            var fullTable = TableHelper.GetCombined(schema, table, tablePart);
            if (fullTable.Contains("{0}")) {
                throw new ArgumentException(Resources.Error_MissingTablePart);
            }
            var cleanedColumn = ColumnHelper.Clean(column);
            var cleanedNewColumn = ColumnHelper.Clean(newName);
            return $@"
IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'{cleanedColumn}' AND Object_ID = Object_ID(N'{fullTable}'))
    AND NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'{cleanedNewColumn}' AND Object_ID = Object_ID(N'{fullTable}'))
BEGIN
    EXEC sp_rename '{fullTable}.{column}', '{cleanedNewColumn}', 'COLUMN';
END
";
        }

        public static string AlterColumn<T>(string column, AlterColumnParameters alterColumnParameters, string tablePart = null) {
            Check.IsValidTableConsuming<T>(tablePart);
            var entityTypeInfo = EntityConverter.GetEntityTypeInfo(typeof(T));
            return AlterColumn(entityTypeInfo.TableDefinition.Schema, entityTypeInfo.TableDefinition.TableName, column, alterColumnParameters, tablePart);
        }

        public static string AlterColumn(Schema schema, string table, string column, AlterColumnParameters alterColumnParameters, string tablePart = null) {
            var fullTable = TableHelper.GetCombined(schema, table, tablePart);
            if (tablePart != null) { table = table.FormatWith(tablePart); }
            if (fullTable.Contains("{0}")) {
                throw new ArgumentException(Resources.Error_MissingTablePart);
            }

            var cleanedColumn = ColumnHelper.Clean(column);
            var checkPart = $"SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = '{schema}' AND TABLE_NAME = '{table}' AND COLUMN_NAME = '{cleanedColumn}'";
            var whereParts = new List<string> {
                $"DATA_TYPE <> '{alterColumnParameters.Type.ToString()}'",
            };

            if (alterColumnParameters.Length != 0) {
                if (alterColumnParameters.Length == -1) {
                    whereParts.Add("(ISNULL(CHARACTER_MAXIMUM_LENGTH, 0) <> -1)");
                } else {
                    whereParts.Add($"(ISNULL(CHARACTER_MAXIMUM_LENGTH, 0) <> -1 AND ISNULL(CHARACTER_MAXIMUM_LENGTH, 0) < {alterColumnParameters.Length})");
                }
            }

            if (alterColumnParameters.Precision != 0) {
                if (alterColumnParameters.Type == SqlDbType.Decimal) {
                    whereParts.Add($"NUMERIC_PRECISION < {alterColumnParameters.Precision}");
                } else {
                    whereParts.Add($"DATETIME_PRECISION < {alterColumnParameters.Precision}");
                }
            }
            whereParts.Add($"IS_NULLABLE <> {(alterColumnParameters.Nullable ? "'YES'" : "'NO'")}");
            var extraChecks = string.Join(" OR ", whereParts);
            checkPart = $"{checkPart} AND ({extraChecks})";
            var definition = alterColumnParameters.ToString();

            var changeNullToNotNull = "";
            var defaultConstraintPart = "";
            if (!alterColumnParameters.Nullable && !alterColumnParameters.DefaultValue.IsNullOrWhiteSpace()) {
                changeNullToNotNull = $@"
    UPDATE {fullTable} SET {column} = {alterColumnParameters.DefaultValue} WHERE {column} IS NULL
";
                defaultConstraintPart = $@"
    ELSE
    BEGIN
        EXEC('ALTER TABLE {fullTable} ADD CONSTRAINT [DF_{cleanedColumn}] DEFAULT {alterColumnParameters.DefaultValue} FOR {column}');
    END
";
            }

            return $@"
IF EXISTS({checkPart})
BEGIN
    DECLARE @Constraint_Name NVARCHAR(MAX);
    DECLARE @Constraint_Definition NVARCHAR(MAX);
    SELECT
        TOP 1 @Constraint_Name = d.Name, @Constraint_Definition = d.Definition
    FROM
        sys.all_columns c
    INNER JOIN sys.tables t ON c.object_id = t.object_id
    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
    INNER JOIN sys.default_constraints d ON c.default_object_id = d.object_id
    WHERE
        s.name = '{schema.ToSql()}' AND
        t.name = '{table}' AND
        c.name = '{cleanedColumn}' AND
        d.Type = 'D'

    IF @Constraint_Name IS NOT NULL AND @Constraint_Definition IS NOT NULL
    BEGIN
        EXEC('ALTER TABLE {fullTable} DROP CONSTRAINT [' + @Constraint_Name + ']');
    END
{changeNullToNotNull}
    ALTER TABLE {fullTable} ALTER COLUMN {column} {definition}

    IF @Constraint_Name IS NOT NULL AND @Constraint_Definition IS NOT NULL
    BEGIN
        EXEC('ALTER TABLE {fullTable} ADD CONSTRAINT [' + @Constraint_Name + '] DEFAULT (' + @Constraint_Definition + ') FOR {column}');
    END
{defaultConstraintPart}
END
";
        }

        public static string DropColumn<T>(string column, string tablePart = null) {
            Check.IsValidTableConsuming<T>(tablePart);
            var entityTypeInfo = EntityConverter.GetEntityTypeInfo(typeof(T));
            return DropColumn(entityTypeInfo.TableDefinition.Schema, entityTypeInfo.TableDefinition.TableName, column, tablePart);
        }

        public static string DropColumn(Schema schema, string table, string column, string tablePart = null) {
            var fullTable = TableHelper.GetCombined(schema, table, tablePart);
            if (fullTable.Contains("{0}")) {
                throw new ArgumentException(Resources.Error_MissingTablePart);
            }
            var cleanedColumn = ColumnHelper.Clean(column);
            return $@"
IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'{cleanedColumn}' AND Object_ID = Object_ID(N'{fullTable}'))
BEGIN
    ALTER TABLE {fullTable} DROP COLUMN [{column}]
END
";
        }

        public static string AddIndex(IndexParameters parameters) {
            return AddIndexAndOptimize(parameters);
        }

        public static string AddIndexAndOptimize(IndexParameters parameters, IndexOptimizeParameters? optimizeParameters = null) {
            if (parameters.IndexedColumns.IsEmpty()) {
                throw new ArgumentException("No index columns defined");
            }
            parameters.FillFactor = optimizeParameters?.FillFactor ?? 0;

            var definition = GetIndexQuery(parameters).SqlEscape();
            var optimizePart = GetOptimizePart(parameters, optimizeParameters);

            var indexPart = string.Join(", ", parameters.IndexedColumns.Select(i => i.GetIndexPart()));
            var includedPart = string.Join(", ", parameters.IncludedColumns.Select(i => i.GetIndexPart()));

            var indexName = parameters.GetIndexName();
            var queryBuilder = new StringBuilder();
            queryBuilder.AppendLine($"DECLARE @AverageFragmentation_{indexName} AS FLOAT = 0");
            queryBuilder.AppendLine($"DECLARE @PageCount_{indexName} AS BIGINT = 0");

            if (parameters.IncludedColumns.HasAny() && parameters.DropWhenIncludedDoesNotMatch) {
                queryBuilder.AppendLine($@"
SELECT i.index_id, t.object_id
INTO #indexes
FROM sys.indexes i
INNER JOIN sys.tables t on t.object_id = i.object_id
INNER JOIN sys.schemas s on s.schema_id = t.schema_id
WHERE t.name = '{parameters.CleanedTable}' and s.name = '{parameters.Schema.ToSql()}' AND i.type <> 1

SELECT i.name
INTO #indexesToDrop
FROM sys.indexes i
INNER JOIN #indexes t ON t.index_id = i.index_id AND t.object_id = i.object_id
WHERE (
	SELECT COUNT(*) AS cnt 
	FROM (
		SELECT c.name
		FROM sys.index_columns ic
		INNER JOIN sys.columns c on c.column_id = ic.column_id and c.object_id = i.object_id
		WHERE ic.index_id = i.index_id and ic.object_id = i.object_id and ic.is_included_column = 0
		EXCEPT 
		SELECT idx.col
		FROM (VALUES {string.Join(",", parameters.IndexedColumns.Select(ic => $"('{ic.ColumnName}')"))}) idx(col)
	) i
) = 0 /* = 0 because we want to match on the exact same indexed columns */
 AND  (
	SELECT COUNT(*) AS cnt 
	FROM (
		SELECT incl.col
		FROM (VALUES {string.Join(",", parameters.IncludedColumns.Select(ic => $"('{ic.ColumnName}')"))}) incl(col)
		EXCEPT 
		SELECT c.name
		FROM sys.index_columns ic
		INNER JOIN sys.columns c on c.column_id = ic.column_id and c.object_id = i.object_id
		WHERE ic.index_id = i.index_id and ic.object_id = i.object_id and ic.is_included_column = 1
	) i
) <> 0 /* <> 0 because if it is not equal to 0, there are included columns missing */

IF EXISTS(SELECT 1 FROM #indexesToDrop)
BEGIN
	DECLARE @dropQ NVARCHAR(MAX) = ''
	SELECT @dropQ = @dropQ + ' DROP INDEX ' + name + ' ON {parameters.FullTable}'
	FROM #indexesToDrop 
    EXEC (@dropQ)
END
DROP TABLE #indexes
DROP TABLE #indexesToDrop
");
            }

            queryBuilder.AppendLine($@"
DECLARE @TableObjectID_{indexName} INT
SELECT @TableObjectID_{indexName} = o.object_id
FROM sys.objects o WHERE o.type = N'U' AND o.schema_id = SCHEMA_ID(N'{parameters.Schema.ToSql()}') AND o.name = N'{parameters.CleanedTable}'

IF @TableObjectID_{indexName} IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = @TableObjectID_{indexName} AND name = N'{indexName}')
    BEGIN
        DECLARE @ExistingIndex{indexName} NVARCHAR(255)
        SELECT @ExistingIndex{indexName} = ind.[name] 
        FROM sys.indexes ind
        INNER JOIN sys.tables t ON ind.[object_id] = t.[object_id] AND t.[type_desc] <> 'BASE_TABLE' AND ISNULL([is_ms_shipped], 0) = 0
        WHERE t.[object_id] = @TableObjectID_{indexName} AND
            LTRIM(RTRIM(STUFF((SELECT DISTINCT ', ' + LTRIM(RTRIM(col.[name] + ' ' + CASE WHEN ic.[is_descending_key] = 0 THEN '' ELSE 'DESC' END))
            FROM sys.index_columns ic
            INNER JOIN sys.columns col ON col.[column_id] = ic.[column_id] AND col.[object_id] = ic.[object_id]
            WHERE ind.[object_id] = ic.[object_id] AND ind.[index_id] = ic.[index_id] AND ic.[is_included_column] = 0
		    FOR XML PATH ('')), 1, 1, ''))) = '{indexPart}' AND
            LTRIM(RTRIM(STUFF((SELECT DISTINCT ', ' + LTRIM(RTRIM(col.[name] + ' ' + CASE WHEN ic.[is_descending_key] = 0 THEN '' ELSE 'DESC' END))
            FROM sys.index_columns ic
            INNER JOIN sys.columns col ON col.[column_id] = ic.[column_id] AND col.[object_id] = ic.[object_id]
            WHERE ind.[object_id] = ic.[object_id] AND ind.[index_id] = ic.[index_id] AND ic.[is_included_column] = 1
		    FOR XML PATH ('')), 1, 1, ''))) = '{includedPart}'

        IF @ExistingIndex{indexName} IS NULL
        BEGIN 
            EXEC ('{definition}')
        END
        ELSE
        BEGIN
            DECLARE @FullIndex{indexName} AS SYSNAME =  CAST('{parameters.FullTable}.' + @ExistingIndex{indexName} AS SYSNAME)
            EXEC sp_rename @FullIndex{indexName}, N'{indexName}', N'INDEX';
        END
    END
    {optimizePart}
END

SELECT
    N'{indexName}' AS IndexName,
    @AverageFragmentation_{indexName} AS AverageFragmentation,
    @PageCount_{indexName} AS PageCount,
    {optimizeParameters?.MinimumReorganize ?? 0} AS MinimumReorganize,
    {optimizeParameters?.MinimumRebuild ?? 0} AS MinimumRebuild,
    {optimizeParameters?.MinimumPageCount ?? 0} AS MinimumPageCount
");
            var query = queryBuilder.ToString();
            return query;
        }

        private static string GetOptimizePart(IndexParameters parameters, IndexOptimizeParameters? optimizeParameters) {
            var optimizePart = "";
            if (optimizeParameters.HasValue) {
                var indexParam = optimizeParameters.Value;
                var indexName = parameters.GetIndexName();
                var updateStatisticsPart = indexParam.UpdateStatistics ? $"UPDATE STATISTICS {parameters.FullTable} [{indexName}]" : "";
                optimizePart = $@"
ELSE
BEGIN
    DECLARE @IndexCommandString{indexName} NVARCHAR(MAX) = ''

    SELECT TOP 1 @IndexCommandString{indexName} = 
	    CASE
		    WHEN a.avg_fragmentation_in_percent BETWEEN {indexParam.MinimumReorganize} AND {indexParam.MinimumRebuild} 
                THEN 'ALTER INDEX [' + b.name + '] ON ' + SCHEMA_NAME(t.schema_id) + '.[' + t.name + '] REORGANIZE ;'
		    WHEN a.avg_fragmentation_in_percent > {indexParam.MinimumRebuild} 
                THEN 'ALTER INDEX [' + b.name + '] ON ' + SCHEMA_NAME(t.schema_id) + '.[' + t.name + '] REBUILD WITH (FILLFACTOR = {indexParam.FillFactor}) ;'
	    END,
        @AverageFragmentation_{indexName} = a.avg_fragmentation_in_percent,
        @PageCount_{indexName} = a.page_count
    FROM sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, NULL) AS a
    INNER JOIN sys.indexes AS b ON a.object_id = b.object_id AND a.index_id = b.index_id 
    INNER JOIN sys.tables t ON b.[object_id] = t.[object_id]
    WHERE b.name = '{indexName}' AND b.object_id = @TableObjectID_{indexName}
	    AND a.index_type_desc <> 'HEAP'

    IF @IndexCommandString{indexName} is not NULL AND @IndexCommandString{indexName} <> '' 
           AND (
                (@AverageFragmentation_{indexName} > {indexParam.MinimumReorganize} AND @PageCount_{indexName} > {indexParam.MinimumPageCount})
                OR @AverageFragmentation_{indexName} > {indexParam.MinimumRebuild}
           )
    BEGIN
        EXEC (@IndexCommandString{indexName})
        {updateStatisticsPart}
    END
END
";
            }
            return optimizePart;
        }

        public static string AddUniqueIndex(IndexParameters parameters) {
            parameters.IsUnique = true;

            var definition = GetIndexQuery(parameters);
            return $@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'{parameters.FullTable}') AND name = N'{parameters.GetIndexName()}')
BEGIN
    {definition}
END
";
        }

        public static string DropForeignKey<T>(string index, string tablePart = null) {
            Check.IsValidTableConsuming<T>(tablePart);
            var entityTypeInfo = EntityConverter.GetEntityTypeInfo(typeof(T));
            return DropForeignKey(entityTypeInfo.TableDefinition.Schema, entityTypeInfo.TableDefinition.TableName, index, tablePart);
        }

        public static string DropForeignKey(Schema schema, string table, string index, string tablePart = null) {
            var fullTable = TableHelper.GetCombined(schema, table, tablePart);
            if (fullTable.Contains("{0}")) {
                throw new ArgumentException(Resources.Error_MissingTablePart);
            }
            var fullIndex = TableHelper.GetCombined(schema, index);
            return $@"
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'{fullIndex}') AND parent_object_id = OBJECT_ID(N'{fullTable}'))
BEGIN
    ALTER TABLE {fullTable} DROP CONSTRAINT {index}
END
";
        }

        public static string DropIndex<T>(string index, string tablePart = null) {
            Check.IsValidTableConsuming<T>(tablePart);
            var entityTypeInfo = EntityConverter.GetEntityTypeInfo(typeof(T));
            return DropIndex(entityTypeInfo.TableDefinition.Schema, entityTypeInfo.TableDefinition.TableName, index, tablePart);
        }

        public static string DropIndex(Schema schema, string table, string index, string tablePart = null) {
            var fullTable = TableHelper.GetCombined(schema, table, tablePart);
            if (fullTable.Contains("{0}")) {
                throw new ArgumentException(Resources.Error_MissingTablePart);
            }
            fullTable = TableHelper.Clean(fullTable);
            var fullIndex = TableHelper.Clean(index);
            return $@"
IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'{fullTable}') AND name = N'{fullIndex}')
BEGIN
    DROP INDEX {index} ON {fullTable}
END
";
        }

        public static string AddForeignKey<T>(string index, string definition, string tablePart = null) {
            Check.IsValidTableConsuming<T>(tablePart);
            var entityTypeInfo = EntityConverter.GetEntityTypeInfo(typeof(T));
            return AddForeignKey(entityTypeInfo.TableDefinition.Schema, entityTypeInfo.TableDefinition.TableName, index, definition, tablePart);
        }

        public static string AddForeignKey(Schema schema, string table, string index, string definition, string tablePart = null) {
            var fullTable = TableHelper.GetCombined(schema, table, tablePart);
            if (fullTable.Contains("{0}")) {
                throw new ArgumentException(Resources.Error_MissingTablePart);
            }
            var fullIndex = TableHelper.GetCombined(schema, index);
            return $@"
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'{fullIndex}') AND parent_object_id = OBJECT_ID(N'{fullTable}'))
BEGIN
    {definition}
END
";
        }

        /// <summary>
        /// Generates the correct CREATE or ALTER statement for the stored procedure, depending on its existence.
        /// </summary>
        /// <param name="storedProcedure">Stored procedure information</param>
        /// <returns></returns>
        public static string CreateOrAlterStoredProcedure(StoredProcedure storedProcedure) {
            if (storedProcedure.Procedure.IsNullOrWhiteSpace()) { return string.Empty; }
            var fullProcedure = TableHelper.GetCombined(storedProcedure.Schema, storedProcedure.Name);
            var definitionBuilder = new StringBuilder(storedProcedure.Procedure);
            definitionBuilder.Replace("CREATE PROCEDURE", "PROCEDURE").Replace("create procedure", "PROCEDURE");
            definitionBuilder.Replace("ALTER PROCEDURE", "PROCEDURE").Replace("alter procedure", "PROCEDURE");
            definitionBuilder.Replace("'", "''");
            var correctedDefinition = definitionBuilder.ToString();
            if (correctedDefinition.IsNullOrWhiteSpace()) {
                return "";
            }

            return $@"
IF EXISTS(SELECT 1 FROM sysobjects WHERE id = object_id(N'{fullProcedure}') and OBJECTPROPERTY(id, N'IsProcedure') = 1 )
BEGIN
    EXEC ('ALTER {correctedDefinition}')
END
ELSE
BEGIN
    EXEC ('CREATE {correctedDefinition}')
END
";
        }

        /// <summary>
        /// Generates the correct DROP statement for the stored procedure.
        /// </summary>
        /// <param name="storedProcedure">Stored procedure information</param>
        /// <returns></returns>
        public static string DropStoredProcedure(StoredProcedure storedProcedure) {
            var fullProcedure = TableHelper.GetCombined(storedProcedure.Schema, storedProcedure.Name);
            return $@"
IF EXISTS(SELECT 1 FROM sysobjects WHERE id = object_id(N'{fullProcedure}') and OBJECTPROPERTY(id, N'IsProcedure') = 1 )
BEGIN
    EXEC ('DROP PROCEDURE {fullProcedure}')
END
";
        }

        public static string CreateOrAlterTrigger(Trigger trigger, string tablePart = null, KeyValueList<string, object> replaceValues = null) {
            var triggerDefinition = trigger.GetDefinition(tablePart);
            if (replaceValues.HasAny()) {
                foreach (var replaceValue in replaceValues) {
                    triggerDefinition = triggerDefinition.Replace(replaceValue.Key, replaceValue.Value.ToString());
                }
            }
            var definitionBuilder = new StringBuilder(triggerDefinition);
            definitionBuilder.Replace("ALTER TRIGGER", "CREATE TRIGGER").Replace("alter trigger", "CREATE TRIGGER");
            definitionBuilder.Replace("'", "''");
            var createDefinition = definitionBuilder.ToString();
            definitionBuilder.Clear();
            definitionBuilder.Append(triggerDefinition);
            definitionBuilder.Replace("CREATE TRIGGER", "ALTER TRIGGER").Replace("create trigger", "ALTER TRIGGER");
            definitionBuilder.Replace("'", "''");
            var alterDefinition = definitionBuilder.ToString();
            var cleanedName = TableHelper.Clean(trigger.GetName(tablePart));
            return $@"
DECLARE @shouldRetry bit = 1
IF NOT EXISTS(SELECT 1 FROM sys.objects WHERE name = '{cleanedName}' AND type = 'TR')
BEGIN
    BEGIN TRY
        EXECUTE dbo.sp_executesql @statement = N'{createDefinition}'
        SET @shouldRetry = 0
    END TRY
    BEGIN CATCH
        WAITFOR DELAY '00:00:05';
    END CATCH

    IF @shouldRetry = 1
    BEGIN
        BEGIN TRY
            EXECUTE dbo.sp_executesql @statement = N'{createDefinition}'
            SET @shouldRetry = 0
        END TRY
        BEGIN CATCH
            WAITFOR DELAY '00:00:10';
        END CATCH
    END

    IF @shouldRetry = 1
    BEGIN
        EXECUTE dbo.sp_executesql @statement = N'{createDefinition}'
    END
END
ELSE
BEGIN
    BEGIN TRY
        EXECUTE dbo.sp_executesql @statement = N'{alterDefinition}'
        SET @shouldRetry = 0
    END TRY
    BEGIN CATCH
        WAITFOR DELAY '00:00:05';
    END CATCH

    IF @shouldRetry = 1
    BEGIN
        BEGIN TRY
            EXECUTE dbo.sp_executesql @statement = N'{alterDefinition}'
            SET @shouldRetry = 0
        END TRY
        BEGIN CATCH
            WAITFOR DELAY '00:00:10';
        END CATCH
    END

    IF @shouldRetry = 1
    BEGIN
        EXECUTE dbo.sp_executesql @statement = N'{alterDefinition}'
    END
END
";
        }

        public static string DropTrigger(Schema schema, string trigger) {
            var fullTrigger = TableHelper.GetCombined(schema, trigger);
            var cleanedName = TableHelper.Clean(trigger);
            return $@"
IF EXISTS(SELECT 1 FROM sys.objects WHERE name = '{cleanedName}' AND type = 'TR')
BEGIN
    DROP TRIGGER {fullTrigger}
END
";
        }

        public static string AddFunction(Schema schema, string function, string definition) {
            var fullFunction = TableHelper.GetCombined(schema, function);
            return $@"
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'{fullFunction}') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
    EXECUTE dbo.sp_executesql @statement = N'{definition}'
END
";
        }

        public static string AddSchema(Schema schema) {
            if (schema == Schema.Unknown) {
                throw new UnknownSchemaException();
            }

            var sqlSchema = schema.ToSql();
            return $@"
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = '{sqlSchema}')
BEGIN
    EXEC('CREATE SCHEMA [{sqlSchema}] AUTHORIZATION dbo');
END
";
        }

        public static string FillTable<T>(string data, string tablePart = null) where T : IEntity {
            Check.IsValidTableConsuming<T>(tablePart);
            var entityTypeInfo = EntityConverter.GetEntityTypeInfo(typeof(T));
            return FillTable(entityTypeInfo.TableDefinition.Schema, entityTypeInfo.TableDefinition.TableName, data, tablePart);
        }

        public static string FillTable(Schema schema, string table, string data, string tablePart = null) {
            table = TableHelper.Clean(string.Format(table, tablePart));
            var fullTable = TableHelper.GetCombined(schema, table);
            if (fullTable.Contains("{0}")) {
                throw new ArgumentException(Resources.Error_MissingTablePart);
            }
            return $@"
IF NOT EXISTS (SELECT * FROM {fullTable})
BEGIN
    {data}
END
";
        }

        public static string TableHasData<T>(string tablePart = null) where T : IEntity {
            Check.IsValidTableConsuming<T>(tablePart);
            var entityTypeInfo = EntityConverter.GetEntityTypeInfo(typeof(T));
            return TableHasData(entityTypeInfo.TableDefinition.Schema, entityTypeInfo.TableDefinition.TableName, tablePart);
        }

        public static string TableHasData(Schema schema, string table, string tablePart = null) {
            var sqlSchema = schema.ToSql();
            table = TableHelper.Clean(string.Format(table, tablePart));
            var fullTable = TableHelper.GetCombined(schema, table);
            if (fullTable.Contains("{0}")) {
                throw new ArgumentException(Resources.Error_MissingTablePart);
            }
            return $@"
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '{sqlSchema}' AND TABLE_NAME = '{table}')
BEGIN
        EXEC ('
            IF EXISTS (SELECT * FROM {fullTable})
                SELECT 1
            ELSE 
                SELECT 0
        ')
END
ELSE
    SELECT 0
";
        }

        public static string TableExists<T>(string tablePart = null) where T : IEntity {
            Check.IsValidTableConsuming<T>(tablePart);
            var entityTypeInfo = EntityConverter.GetEntityTypeInfo(typeof(T));
            return TableHasData(entityTypeInfo.TableDefinition.Schema, entityTypeInfo.TableDefinition.TableName, tablePart);
        }

        public static string TableExists(Schema schema, string table, string tablePart = null) {
            var sqlSchema = schema.ToSql();
            table = TableHelper.Clean(string.Format(table, tablePart));
            if (table.Contains("{0}")) {
                throw new ArgumentException(Resources.Error_MissingTablePart);
            }
            return $@"
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '{sqlSchema}' AND TABLE_NAME = '{table}')
    SELECT 1
ELSE 
    SELECT 0
";
        }

        /// <summary>
        /// Generates the query to drop the table specified by the provided <see cref="BaseResult"/> type.
        /// The query first checks if the table exists and then attempts to drop the table only if there is no data present.
        /// </summary>
        /// <typeparam name="T">Type for which the table should be dropped</typeparam>
        /// <param name="tablePart">Optional table part for dynamic tables</param>
        /// <returns></returns>
        public static string DropTable<T>(string tablePart = null) where T : IEntity {
            Check.IsValidTableConsuming<T>(tablePart);
            var entityTypeInfo = EntityConverter.GetEntityTypeInfo(typeof(T));
            return DropTable(entityTypeInfo.TableDefinition.Schema, entityTypeInfo.TableDefinition.TableName, tablePart);
        }

        /// <summary>
        /// Generates the query to drop the table.
        /// The query first checks if the table exists and then attempts to drop the table only if there is no data present.
        /// </summary>
        /// <param name="schema">Schema of the table to drop</param>
        /// <param name="table">Table name of the table to drop.</param>
        /// <param name="tablePart">Optional table part for dynamic tables</param>
        /// <returns></returns>
        public static string DropTable(Schema schema, string table, string tablePart = null) {
            var sqlSchema = schema.ToSql();
            table = TableHelper.Clean(string.Format(table, tablePart));
            var fullTable = TableHelper.GetCombined(schema, table);
            if (fullTable.Contains("{0}")) {
                throw new ArgumentException(Resources.Error_MissingTablePart);
            }
            return $@"
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '{sqlSchema}' AND TABLE_NAME = '{table}')
BEGIN
    EXEC ('
        IF NOT EXISTS (SELECT * FROM {fullTable})
        BEGIN
            DROP TABLE {fullTable}
        END
    ')
END

";
        }

        public static string AddUniqueConstraint<T>(string name, string indexedColumns, string tablePart = null) where T : IEntity {
            Check.IsValidTableConsuming<T>(tablePart);
            var entityTypeInfo = EntityConverter.GetEntityTypeInfo(typeof(T));
            return AddUniqueConstraint(entityTypeInfo.TableDefinition.Schema, entityTypeInfo.TableDefinition.TableName,
                                       name, indexedColumns, tablePart);
        }

        public static string AddUniqueConstraint(Schema schema, string table, string name, string indexedColumns, string tablePart = null) {
            var fullTable = TableHelper.GetCombined(schema, table, tablePart);
            if (fullTable.Contains("{0}")) {
                throw new ArgumentException(Resources.Error_MissingTablePart);
            }
            var cleanedName = TableHelper.Clean(name);
            return $@"
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'{fullTable}') AND name = N'{name}')
BEGIN
ALTER TABLE {fullTable} ADD CONSTRAINT [{cleanedName}] UNIQUE NONCLUSTERED 
(
	{indexedColumns}
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
END
";
        }

        private static string GetIndexQuery(IndexParameters parameters) {
            var includedPart = parameters.IncludedColumns.IsEmpty() ? "" : $"INCLUDE ( {string.Join(", ", parameters.IncludedColumns.Select(i => i.GetIndexPart()))} )";
            var uniquePart = parameters.IsUnique ? "UNIQUE " : "";
            var fillFactorPart = parameters.FillFactor != 0 ? $", FILLFACTOR = {parameters.FillFactor}" : "";
            var filterPart = parameters.FilterClause.HasNonWhiteSpaceValue() ? parameters.FilterClause.Trim() : "";
            return $@"
CREATE {uniquePart}NONCLUSTERED INDEX [{parameters.GetIndexName()}] ON {parameters.FullTable}
(
	{string.Join(", ", parameters.IndexedColumns.Select(i => i.GetIndexPart()))}
)
{includedPart}
{filterPart}
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON{fillFactorPart}) ON [PRIMARY]
";
        }

        public static string AddFullTextCatalog(FullTextCatalog catalog) {
            var name = catalog.ToString();
            return $@"
IF NOT EXISTS(SELECT 1 FROM sys.fulltext_catalogs WHERE name = N'{name}') 
BEGIN
    CREATE FULLTEXT CATALOG [{name}] WITH ACCENT_SENSITIVITY = OFF
END
";
        }

        public static string AddFullTextIndex<T>(FullTextCatalog catalog, string primaryKeyName, List<string> columns) where T : IEntity {
            var entityTypeInfo = EntityConverter.GetEntityTypeInfo(typeof(T));
            return AddFullTextIndex(catalog, entityTypeInfo.TableDefinition.Schema, entityTypeInfo.TableDefinition.TableName, primaryKeyName, columns);
        }

        public static string AddFullTextIndex(FullTextCatalog catalog, Schema schema, string table, string primaryKeyName, List<string> columns) {
            var fullTable = TableHelper.GetCombined(schema, table);
            var name = catalog.ToString();
            var cleanedPrimaryKeyName = TableHelper.Clean(primaryKeyName);
            var columnList = columns.Select(c => $"[{TableHelper.Clean(c)}] LANGUAGE 'Dutch'");
            var columnString = string.Join(",\n", columnList);
            return $@"
IF NOT EXISTS(SELECT 1 FROM sys.tables t
INNER JOIN sys.fulltext_indexes fi ON t.[object_id] = fi.[object_id]
INNER JOIN sys.fulltext_index_columns ic ON ic.[object_id] = t.[object_id]
	where t.[object_id] = object_ID('{fullTable}')) 
BEGIN
    CREATE FULLTEXT INDEX ON {fullTable}(
    {columnString})
    KEY INDEX [{cleanedPrimaryKeyName}]
    ON ([{name}], FILEGROUP [PRIMARY])
    WITH (CHANGE_TRACKING = AUTO, STOPLIST = OFF)
END
";
        }

        public static string AddFullTextColumn<T>(FullTextCatalog catalog, string column) where T : IEntity {
            var entityTypeInfo = EntityConverter.GetEntityTypeInfo(typeof(T));
            return AddFullTextColumn(catalog, entityTypeInfo.TableDefinition.Schema, entityTypeInfo.TableDefinition.TableName, column);
        }

        public static string AddFullTextColumn(FullTextCatalog catalog, Schema schema, string table, string column) {
            var fullTable = TableHelper.GetCombined(schema, table);
            if (fullTable.Contains("{0}")) {
                throw new ArgumentException(Resources.Error_MissingTablePart);
            }
            var name = catalog.ToString();
            var cleanColumnName = TableHelper.Clean(column);
            var columnString = $"[{cleanColumnName}] LANGUAGE 'Dutch'";
            return $@"
IF EXISTS(SELECT 1 FROM sys.tables t
INNER JOIN sys.fulltext_indexes fi ON t.[object_id] = fi.[object_id]
	where t.[object_id] = object_ID('{fullTable}')) 
    AND NOT EXISTS(SELECT 1 FROM sys.tables t
INNER JOIN sys.fulltext_indexes fi ON t.[object_id] = fi.[object_id]
inner join sys.fulltext_catalogs ftc on ftc.fulltext_catalog_id = fi.fulltext_catalog_id
INNER JOIN sys.fulltext_index_columns ic ON ic.[object_id] = t.[object_id]
INNER JOIN sys.columns c on c.column_id = ic.column_id and c.object_id = t.object_id
	where t.[object_id] = object_ID('{fullTable}') AND ftc.name = N'{name}'
	and c.name = N'{cleanColumnName}' )
BEGIN
    ALTER FULLTEXT INDEX ON {fullTable} ADD ({columnString})
END
";
        }

        public static string AddCheckConstraint<T>(string name, string check, string tablePart = null) {
            Check.IsValidTableConsuming<T>(tablePart);
            var entityTypeInfo = EntityConverter.GetEntityTypeInfo(typeof(T));
            return AddCheckConstraint(entityTypeInfo.TableDefinition.Schema, entityTypeInfo.TableDefinition.TableName, name, check, tablePart);
        }

        public static string AddCheckConstraint(Schema schema, string table, string name, string check, string tablePart = null) {
            var fullTable = TableHelper.GetCombined(schema, table, tablePart);
            if (fullTable.Contains("{0}")) {
                throw new ArgumentException(Resources.Error_MissingTablePart);
            }
            return $@"
IF NOT EXISTS(SELECT 1 FROM sys.objects WHERE type_desc LIKE '%CONSTRAINT' AND parent_object_id = OBJECT_ID(N'{fullTable}')  AND OBJECT_NAME(OBJECT_ID) = N'{name}')
BEGIN
    ALTER TABLE {fullTable} ADD CONSTRAINT {name} CHECK ({check})
END
";
        }

        public static string AddDefaultConstraint<T>(string column, string defaultConstraint, string tablePart = null) {
            Check.IsValidTableConsuming<T>(tablePart);
            var entityTypeInfo = EntityConverter.GetEntityTypeInfo(typeof(T));
            return AddDefaultConstraint(entityTypeInfo.TableDefinition.Schema, entityTypeInfo.TableDefinition.TableName, column, defaultConstraint, tablePart);
        }

        public static string AddDefaultConstraint(Schema schema, string table, string column, string defaultConstraint, string tablePart = null) {
            var cleanedColumn = TableHelper.Clean(column);
            var sqlSchema = schema.ToSql();
            table = TableHelper.Clean(string.Format(table, tablePart));
            var fullTable = TableHelper.GetCombined(schema, table, tablePart);
            if (fullTable.Contains("{0}")) {
                throw new ArgumentException(Resources.Error_MissingTablePart);
            }
            return $@"
IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'{cleanedColumn}' AND Object_ID = Object_ID(N'{fullTable}')) 
   AND (SELECT Column_Default FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = '{sqlSchema}' AND TABLE_NAME = '{table}' AND COLUMN_NAME = '{cleanedColumn}') IS NULL
BEGIN
    ALTER TABLE {fullTable} ADD DEFAULT ({defaultConstraint}) FOR {cleanedColumn}
END
";
        }
    }
}