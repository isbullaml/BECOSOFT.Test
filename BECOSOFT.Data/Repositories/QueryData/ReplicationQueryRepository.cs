using BECOSOFT.Data.Attributes;
using BECOSOFT.Data.Collections;
using BECOSOFT.Data.Context;
using BECOSOFT.Data.Helpers;
using BECOSOFT.Data.Models;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Data.Models.QueryData;
using BECOSOFT.Data.Repositories.Interfaces.QueryData;
using BECOSOFT.Data.Services.Interfaces;
using BECOSOFT.Utilities.Collections;
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Extensions.Collections;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BECOSOFT.Data.Repositories.QueryData {
    internal class ReplicationQueryRepository : BaseResultRepository<ReplicatedTableQueryResult>, IReplicationQueryRepository {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly ITableQueryRepository _tableQueryRepository;
        public ReplicationQueryRepository(IDbContextFactory dbContextFactory,
                                          ITableQueryRepository tableQueryRepository,
                                          IDatabaseCommandFactory databaseCommandFactory)
            : base(dbContextFactory, databaseCommandFactory) {
            _tableQueryRepository = tableQueryRepository;
        }

        public ReplicatedTableResult CheckReplicatedTables(ReplicatedTableParameters parameters) {
            if (!_tableQueryRepository.TableExists(Schema.Dbo, "MSreplication_objects")) {
                return new ReplicatedTableResult {
                    NoReplicationDefined = true,
                };
            }
            List<Type> types;
            var entityType = typeof(IEntity);
            if (parameters.TypesToCheck.HasAny()) {
                types = parameters.TypesToCheck.Where(t => t != null && entityType.IsAssignableFrom(t)).ToDistinctList();
            } else {
                types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetAllLoadableTypes()).Where(t => t != null && entityType.IsAssignableFrom(t)).ToDistinctList();
            }
            var typesToCheck = new Dictionary<Type, (ReplicatedTableAttribute ReplicatedTable, TableAttribute Table, KeyValueList<string, string> TableNames)>();
            foreach (var type in types) {
                var replicatedTableAttribute = type.GetCustomAttribute<ReplicatedTableAttribute>();
                var tableAttribute = type.GetCustomAttribute<TableAttribute>();
                if (tableAttribute == null || replicatedTableAttribute == null) { continue; }
                typesToCheck.Add(type, (replicatedTableAttribute, tableAttribute, new KeyValueList<string, string>(1)));
            }

            if (typesToCheck.IsEmpty()) {
                return new ReplicatedTableResult();
            }

            var parameterQuery = new ParametrizedQuery();

            var fullTableNameMapping = new HashSet<string>();
            foreach (var typeToCheck in typesToCheck) {
                var table = typeToCheck.Value.Table;
                var tableNames = typeToCheck.Value.TableNames;
                var tableDefinition = table.ToDefinition();
                if (typeToCheck.Key.IsSubclassOfRawGeneric(typeof(TableConsumingEntity<>)) 
                    || typeToCheck.Key.IsSubclassOfRawGeneric(typeof(TableConsumingResult<>))) {
                    var baseType = typeToCheck.Key.BaseType;
                    if (baseType == null || baseType.GenericTypeArguments.Length == 0) { continue; }
                    var definingType = baseType.GenericTypeArguments[0];
                    var didFind = false;
                    foreach (var tablePartEntry in parameters.TablePartEntries) {
                        var tablePart = tablePartEntry.Value;
                        if (definingType != tablePartEntry.Key) { continue; }
                        var fullTable = TableHelper.Clean(tableDefinition.GetFullTable(tablePart));
                        tableNames.Add(fullTable, tablePart);
                        fullTableNameMapping.Add(fullTable);
                        didFind = true;
                    }
                    if (!didFind) {
                        Logger.Warn("Did not find table part for {0} (table: {1})", typeToCheck.Key, tableDefinition.FullTableName);
                    }
                } else {
                    var fullTable = TableHelper.Clean(tableDefinition.GetFullTable(null));
                    tableNames.Add(fullTable, null);
                    fullTableNameMapping.Add(fullTable);
                }
            }
            var allTableNames = fullTableNameMapping.ToList();
            var tablesTempTable = parameterQuery.AddTempTable(allTableNames);
            var query = new StringBuilder();

            query.AppendLine(" CREATE TABLE #AllTables(TableName NVARCHAR(500) COLLATE DATABASE_DEFAULT NOT NULL, IsMissing BIT, IsReplicated BIT) ");
            query.AppendLine(" INSERT INTO #AllTables(TableName, IsMissing, IsReplicated) ");
            query.AppendLine(" SELECT t.tempValue AS TableName, NULL AS IsMissing, NULL AS IsReplicated FROM {0} t ", tablesTempTable.TableName);

            query.AppendLine(" UPDATE [at] SET IsMissing = CASE WHEN t.object_id IS NULL THEN 1 ELSE 0 END ");
            query.AppendLine("             , IsReplicated = CASE WHEN r.object_id IS NULL THEN 0 ELSE 1 END ");
            query.AppendLine(" FROM #AllTables [at] ");
            query.AppendLine(" INNER JOIN sys.schemas s on s.name = LEFT([at].TableName, CHARINDEX('.', [at].TableName) - 1) COLLATE DATABASE_DEFAULT ");
            query.AppendLine(" LEFT JOIN sys.tables t on t.name = RIGHT([at].TableName, LEN([at].TableName) - CHARINDEX('.', [at].TableName)) COLLATE DATABASE_DEFAULT AND t.schema_id = s.schema_id ");
            query.AppendLine(" LEFT JOIN (");
            query.AppendLine("     SELECT DISTINCT ot.object_id, ot.schema_id");
            query.AppendLine("     FROM dbo.MSreplication_objects r ");
            query.AppendLine("     LEFT JOIN sys.objects so ON r.object_name = so.name COLLATE DATABASE_DEFAULT AND so.type = 'P' COLLATE DATABASE_DEFAULT ");
            query.AppendLine("     LEFT JOIN sys.sql_dependencies dp ON so.object_id = dp.object_id ");
            query.AppendLine("     LEFT JOIN sys.objects ot ON dp.referenced_major_id = ot.object_id AND r.article = ot.name COLLATE DATABASE_DEFAULT ");
            query.AppendLine(" ) r on r.object_id = t.object_id AND r.schema_id = s.schema_id ");

            query.AppendLine(" SELECT * FROM #AllTables ");

            query.AppendLine(" DROP TABLE #AllTables ");
            parameterQuery.SetQuery(query);

            IPagedList<ReplicatedTableQueryResult> replicatedTableQueryResults;
            var command = DatabaseCommandFactory.Custom<ReplicatedTableQueryResult>(parameterQuery);
            using (var context = DbContextFactory.CreateBaseResultContext()) {
                replicatedTableQueryResults = context.Query<ReplicatedTableQueryResult>(command);
            }
            var queryResultDict = replicatedTableQueryResults.ToDictionary(t => t.TableName);
            var hasRequiredOptionalTables = parameters.RequiredOptionalTables.HasAny();
            var replicatedTableEntries = new List<ReplicatedTableEntry>();
            foreach (var typeToCheck in typesToCheck) {
                var type = typeToCheck.Key;
                var replicatedTable = typeToCheck.Value.ReplicatedTable;
                var table = typeToCheck.Value.Table;
                var tableNames = typeToCheck.Value.TableNames;

                var isRequiredOptional = replicatedTable.IsOptional && hasRequiredOptionalTables && parameters.RequiredOptionalTables.Contains(type);

                var tableEntry = new ReplicatedTableEntry {
                    Type = type,
                    IsOptional = replicatedTable.IsOptional && !isRequiredOptional,
                    TableDefinition = table.ToDefinition(),
                    QueryResults = new List<ReplicatedTableQueryResult>(tableNames.Count),
                };

                foreach (var tableName in tableNames) {
                    var queryResult = queryResultDict.TryGetValueWithDefault(tableName.Key);
                    if (queryResult == null) {
                        queryResult = new ReplicatedTableQueryResult {
                            TableName = tableName.Key,
                            TablePart = tableName.Value,
                            IsMissing = true,
                        };
                    } else {
                        if (queryResult.TablePart.IsNullOrWhiteSpace()) {
                            queryResult.TablePart = tableName.Value;
                        }
                        tableEntry.DidCheck = true;
                    }
                    tableEntry.QueryResults.Add(queryResult);
                }
                replicatedTableEntries.Add(tableEntry);
                _tableQueryRepository.RegisterTableExists(tableEntry);
            }

            return new ReplicatedTableResult(replicatedTableEntries);
        }

        public Dictionary<string, List<ReplicationSubscriberStatus>> GetReplicationSubscriberStatus() {
            var subscribers = InternalGetReplicationSubscriberStatus();
            return subscribers.GroupBy(rss => rss.PublisherDatabase).ToDictionary(g => g.Key, g => g.OrderBy(rss => rss.Subscriber).ToList());
        }

        public List<ReplicationSubscriberStatus> GetReplicationSubscriberStatus(string databaseName) {
            if (databaseName.IsNullOrWhiteSpace()) {
                return new List<ReplicationSubscriberStatus>(0);
            }
            return InternalGetReplicationSubscriberStatus(databaseName);
        }

        private List<ReplicationSubscriberStatus> InternalGetReplicationSubscriberStatus(string databaseName = null) {
            var parameterQuery = new ParametrizedQuery();
            if (!databaseName.IsNullOrWhiteSpace()) {
                parameterQuery.AddParameter("@DatabaseName", databaseName);
            }
            var query = new StringBuilder();
            query.AppendLine(" SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED; ");
            query.AppendLine("  SELECT t.[Status] ");
            query.AppendLine("       , t.[SubscriberDatabase] ");
            query.AppendLine("       , t.[Publication] ");
            query.AppendLine("       , SUBSTRING(t.[Subscriber], 0, LEN(t.[Subscriber]) - (CHARINDEX('-', REVERSE(t.[Subscriber])) - 1)) AS [Subscriber] ");
            query.AppendLine("       , t.[LastSynchronised] ");
            query.AppendLine("       , t.[UndistributedCommands] ");
            query.AppendLine("       , t.[Comments] ");
            query.AppendLine("       , t.[PublisherDatabase] ");
            query.AppendLine("       , t.[Publisher DB] ");
            query.AppendLine("       , t.[SubscriptionType] ");
            query.AppendLine("       , t.[Pub - DB - Publication - SUB - AgentID] ");
            query.AppendLine("  FROM ( ");
            query.AppendLine("  	SELECT mdh.runstatus [Status],  ");
            query.AppendLine("  	mda.subscriber_db [SubscriberDatabase],  ");
            query.AppendLine("  	mda.publication [Publication], ");
            query.AppendLine("  	REPLACE(REPLACE(REPLACE(mda.name, @@SERVERNAME + '-', ''), mda.publisher_db + '-', ''), mda.publication + '-', '') [Subscriber], ");
            query.AppendLine("  	CONVERT(VARCHAR(25),mdh.[time]) [LastSynchronised], ");
            query.AppendLine("  	und.UndelivCmdsInDistDB [UndistributedCommands],  ");
            query.AppendLine("  	mdh.comments [Comments],  ");
            query.AppendLine("  	mda.publisher_db [PublisherDatabase], ");
            query.AppendLine("  	mda.publisher_db+' - '+CAST(mda.publisher_database_id as varchar) [Publisher DB], ");
            query.AppendLine("  	(CASE   ");
            query.AppendLine("  		WHEN mda.subscription_type =  '0' THEN 'Push'  ");
            query.AppendLine("  		WHEN mda.subscription_type =  '1' THEN 'Pull'  ");
            query.AppendLine("  		WHEN mda.subscription_type =  '2' THEN 'Anonymous'  ");
            query.AppendLine("  		ELSE CAST(mda.subscription_type AS VARCHAR) ");
            query.AppendLine("  	END) [SubscriptionType], ");
            query.AppendLine("  	mda.name [Pub - DB - Publication - SUB - AgentID] ");
            query.AppendLine("  	FROM distribution.dbo.MSdistribution_agents mda  ");
            query.AppendLine("  	LEFT JOIN distribution.dbo.MSdistribution_history mdh ON mdh.agent_id = mda.id  ");
            query.AppendLine("  	JOIN  ");
            query.AppendLine("  		(SELECT s.agent_id, MaxAgentValue.[time], SUM(CASE WHEN xact_seqno > MaxAgentValue.maxseq THEN 1 ELSE 0 END) AS UndelivCmdsInDistDB  ");
            query.AppendLine("  		FROM distribution.dbo.MSrepl_commands t (NOLOCK)   ");
            query.AppendLine("  		JOIN distribution.dbo.MSsubscriptions AS s (NOLOCK) ON (t.article_id = s.article_id AND t.publisher_database_id=s.publisher_database_id )  ");
            query.AppendLine("  		JOIN  ");
            query.AppendLine("  			(SELECT hist.agent_id, MAX(hist.[time]) AS [time], h.maxseq   ");
            query.AppendLine("  			FROM distribution.dbo.MSdistribution_history hist (NOLOCK)  ");
            query.AppendLine("  			JOIN (SELECT agent_id,ISNULL(MAX(xact_seqno),0x0) AS maxseq  ");
            query.AppendLine("  			FROM distribution.dbo.MSdistribution_history (NOLOCK)   ");
            query.AppendLine("  			GROUP BY agent_id) AS h   ");
            query.AppendLine("  			ON (hist.agent_id=h.agent_id AND h.maxseq=hist.xact_seqno)  ");
            query.AppendLine("  			GROUP BY hist.agent_id, h.maxseq  ");
            query.AppendLine("  			) AS MaxAgentValue  ");
            query.AppendLine("  		ON MaxAgentValue.agent_id = s.agent_id  ");
            query.AppendLine("  		GROUP BY s.agent_id, MaxAgentValue.[time]  ");
            query.AppendLine("  		) und  ");
            query.AppendLine("  	ON mda.id = und.agent_id AND und.[time] = mdh.[time]  ");
            query.AppendLine("  	WHERE mda.subscriber_db<>'virtual' ");
            if (!databaseName.IsNullOrWhiteSpace()) {
                query.AppendLine("        AND mda.publisher_db = @DatabaseName ");
            }
            query.AppendLine("  ) t ");
            query.AppendLine("  ORDER BY t.Subscriber ");

            parameterQuery.SetQuery(query);
            parameterQuery.Timeout = 600;
            var command = DatabaseCommandFactory.Custom(parameterQuery);
            using (var context = GetContext()) {
                var subscribers = context.Query<ReplicationSubscriberStatus>(command);
                return subscribers.Items;
            }
        }
    }
}
