using BECOSOFT.Data.Context;
using BECOSOFT.Data.Converters;
using BECOSOFT.Data.Exceptions;
using BECOSOFT.Data.Helpers;
using BECOSOFT.Data.Models;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Data.Models.QueryData;
using BECOSOFT.Data.Repositories.Interfaces.QueryData;
using BECOSOFT.Data.Services.Interfaces;
using BECOSOFT.Utilities.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BECOSOFT.Data.Repositories.QueryData {
    internal class TriggerQueryRepository : BaseResultRepository<TriggerQueryResult>, ITriggerQueryRepository {
        public TriggerQueryRepository(IDbContextFactory dbContextFactory, IDatabaseCommandFactory databaseCommandFactory) : base(dbContextFactory, databaseCommandFactory) {
        }

        public TriggerEnableResult EnableTrigger<T>(string triggerName, string tablePart = null) where T : IEntity {
            Check.IsValidTableConsuming<T>(tablePart);
            var entityInfo = EntityConverter.GetEntityTypeInfo(typeof(T));
            var tableDefinition = entityInfo.TableDefinition;

            return EnableTrigger(tableDefinition.Schema, tableDefinition.TableName, triggerName, tablePart);
        }

        public TriggerEnableResult EnableTrigger(Schema schema, string tableName, string triggerName, string tablePart = null) {
            var result = new TriggerEnableResult();
            var trigger = GetTrigger(schema, tableName, triggerName, tablePart);
            if (trigger == null) {
                result.DoesNotExist = true;
                return result;
            }
            if (trigger.IsEnabled) {
                result.AlreadyEnabled = true;
                return result;
            }
            var fullTable = TableHelper.GetCombined(schema, tableName, tablePart);
            var cleanTriggerName = TableHelper.Clean(triggerName);
            var query = " ENABLE TRIGGER [{0}] ON {1} ".FormatWith(cleanTriggerName, fullTable);
            var parameterQuery = new ParametrizedQuery(query);
            var command = DatabaseCommandFactory.Custom<TriggerQueryResult>(parameterQuery);
            using (var context = DbContextFactory.CreateBaseResultContext()) {
                context.QueryConvertible<int>(command);
            }
            trigger = GetTrigger(schema, tableName, triggerName, tablePart);
            if (trigger == null) {
                result.DoesNotExist = true;
                return result;
            }
            if (trigger.IsEnabled) {
                result.DidEnable = true;
            }
            return result;
        }

        public TriggerDisableResult DisableTrigger<T>(string triggerName, string tablePart = null) where T : IEntity {
            Check.IsValidTableConsuming<T>(tablePart);
            var entityInfo = EntityConverter.GetEntityTypeInfo(typeof(T));
            var tableDefinition = entityInfo.TableDefinition;

            return DisableTrigger(tableDefinition.Schema, tableDefinition.TableName, triggerName, tablePart);
        }

        public TriggerDisableResult DisableTrigger(Schema schema, string tableName, string triggerName, string tablePart = null) {
            var result = new TriggerDisableResult();
            var trigger = GetTrigger(schema, tableName, triggerName, tablePart);
            if (trigger == null) {
                result.DoesNotExist = true;
                return result;
            }
            if (!trigger.IsEnabled) {
                result.AlreadyDisabled = true;
                return result;
            }
            var fullTable = TableHelper.GetCombined(schema, tableName, tablePart);
            var cleanTriggerName = TableHelper.Clean(triggerName);
            var query = " DISABLE TRIGGER [{0}] ON {1} ".FormatWith(cleanTriggerName, fullTable);
            var parameterQuery = new ParametrizedQuery(query);
            var command = DatabaseCommandFactory.Custom<TriggerQueryResult>(parameterQuery);
            using (var context = DbContextFactory.CreateBaseResultContext()) {
                context.QueryConvertible<int>(command);
            }
            trigger = GetTrigger(schema, tableName, triggerName, tablePart);
            if (trigger == null) {
                result.DoesNotExist = true;
                return result;
            }
            if (!trigger.IsEnabled) {
                result.DidDisable = true;
            }
            return result;
        }

        public TriggerQueryResult GetTrigger<T>(string triggerName, string tablePart = null) where T : IEntity {
            Check.IsValidTableConsuming<T>(tablePart);
            var entityInfo = EntityConverter.GetEntityTypeInfo(typeof(T));
            var tableDefinition = entityInfo.TableDefinition;

            return GetTrigger(tableDefinition.Schema, tableDefinition.TableName, triggerName, tablePart);
        }

        public TriggerQueryResult GetTrigger(Schema schema, string tableName, string triggerName, string tablePart = null) {

            var fullTable = TableHelper.GetCombined(schema, tableName, tablePart);
            var cleanTable = TableHelper.Clean(fullTable);
            var cleanTriggerName = TableHelper.Clean(triggerName);
            var parameterQuery = new ParametrizedQuery();
            parameterQuery.AddParameter("@FullTable", cleanTable);
            parameterQuery.AddParameter("@TriggerName", cleanTriggerName);
            var sb = new StringBuilder();
            sb.Append(" SELECT t.name AS {0}, CASE WHEN t.is_disabled = 1 THEN 0 ELSE 1 END AS {1} ", GetColumn(r => r.Name), GetColumn(r => r.IsEnabled));
            sb.Append(" FROM sys.triggers t ");
            sb.Append(" WHERE t.parent_id = OBJECT_ID(@FullTable) ");
            sb.Append("   AND t.name = @TriggerName ");

            parameterQuery.SetQuery(sb);
            var command = DatabaseCommandFactory.Custom<TriggerQueryResult>(parameterQuery);

            return Execute(Func, () => command.ToHashString());

            TriggerQueryResult Func() {
                using (var context = DbContextFactory.CreateBaseResultContext()) {
                    return context.Query<TriggerQueryResult>(command).FirstOrDefault();
                }
            }
        }

        public List<TriggerQueryResult> GetTriggers<T>(string triggerName, string tablePart = null) where T : IEntity {
            Check.IsValidTableConsuming<T>(tablePart);
            var entityInfo = EntityConverter.GetEntityTypeInfo(typeof(T));
            var tableDefinition = entityInfo.TableDefinition;

            return GetTriggers(tableDefinition.Schema, tableDefinition.TableName, triggerName, tablePart);
        }

        public List<TriggerQueryResult> GetTriggers(Schema schema, string tableName, string triggerName, string tablePart = null) {

            var fullTable = TableHelper.GetCombined(schema, tableName, tablePart);
            var cleanTable = TableHelper.Clean(fullTable);
            var parameterQuery = new ParametrizedQuery();
            parameterQuery.AddParameter("@FullTable", cleanTable);
            var sb = new StringBuilder();
            sb.Append(" SELECT t.Name AS {0}, CASE WHEN t.is_disabled = 1 THEN 0 ELSE 1 END AS {1} ", GetColumn(r => r.Name), GetColumn(r => r.IsEnabled));
            sb.Append(" FROM sys.triggers t ");
            sb.Append(" WHERE t.parent_id = OBJECT_ID(@FullTable) ");

            parameterQuery.SetQuery(sb);
            var command = DatabaseCommandFactory.Custom<TriggerQueryResult>(parameterQuery);

            return Execute(Func, () => command.ToHashString());

            List<TriggerQueryResult> Func() {
                using (var context = DbContextFactory.CreateBaseResultContext()) {
                    return context.Query<TriggerQueryResult>(command).ToList();
                }
            }
        }
    }
}