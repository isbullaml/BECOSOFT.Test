using BECOSOFT.Data.Attributes;
using BECOSOFT.Data.Collections;
using BECOSOFT.Data.Models;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Data.Parsers;
using BECOSOFT.Data.Query;
using BECOSOFT.Data.Services.Interfaces;
using BECOSOFT.Utilities.Converters;
using BECOSOFT.Utilities.Extensions.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Data.Repositories.Interfaces;

namespace BECOSOFT.Data.Context {
    /// <inheritdoc cref="IBaseEntityDbContext" />
    internal sealed class BaseEntityDbContext : DbContext, IBaseEntityDbContext {
        private readonly IDatabaseCommandFactory _databaseCommandFactory;
        private readonly IOfflineTableExistsRepository _tableExistsRepository;

        public BaseEntityDbContext(IDbConnection dbConnection,
                                   IBulkCopyHelper bulkCopyHelper,
                                   IDatabaseCommandFactory databaseCommandFactory,
                                   IOfflineTableExistsRepository tableExistsRepository)
            : base(dbConnection, bulkCopyHelper) {
            _databaseCommandFactory = databaseCommandFactory;
            _tableExistsRepository = tableExistsRepository;
        }

        /// <inheritdoc />
        public IPagedList<T> QueryConvertible<T>(DatabaseCommand command) where T : IConvertible {
            using (var reader = ExecuteReader(command)) {
                var parser = new ConvertibleParser<T>();
                return parser.SelectConvertible(reader);
            }
        }

        /// <inheritdoc />
        public IPagedList<T> Query<T>(DatabaseCommand command) where T : BaseEntity {
            using (var reader = ExecuteReader(command)) {
                var parser = CreateParser<T>();
                return parser.Select(reader, command.TablePart);
            }
        }

        /// <inheritdoc />
        public T Get<T>(DatabaseCommand command) where T : BaseEntity {
            using (var reader = ExecuteReader(command)) {
                var parser = CreateParser<T>();
                return parser.Single(reader, command.TablePart);
            }
        }

        /// <inheritdoc />
        public IPagedList<TResult> GetProperties<T, TResult>(DatabaseCommand command) where T : BaseEntity where TResult : class {
            using (var reader = ExecuteReader(command)) {
                var parser = CreateParser<T>();
                return parser.SelectCustom<TResult>(reader);
            }
        }

        /// <inheritdoc />
        public IPagedList<object> GetPropertiesNonGeneric<T>(DatabaseCommand command, Type resultType) where T : BaseEntity {
            var t = GetType();
            var method = t.GetMethods().First(m => m.Name == "GetProperties" && m.GetGenericArguments().Length == 2);
            var nonGeneric = method.MakeGenericMethod(typeof(T), resultType);
            return new PagedList<object>((nonGeneric.Invoke(this, new object[] { command }) as IEnumerable)?.Cast<object>());
        }

        /// <inheritdoc />
        public void Insert<T>(DatabaseCommandBuilder<T> commandBuilder) where T : BaseEntity {
            var parser = new BaseResultParser<InsertInformation>(_tableExistsRepository);
            var builder = commandBuilder.QueryBuilder;
            var entityList = new List<T>(commandBuilder.Entities.ToSafeList());
            while (entityList.HasAny()) {
                var command = _databaseCommandFactory.Build(builder);
                var entityRange = PrepareEntityData(commandBuilder, entityList, command);
                var numberPerBatch = entityRange.Count;
                using (var reader = ExecuteReader(command)) {
                    var newIdentities = parser.Select(reader, command.TablePart);
                    foreach (var newIdentity in newIdentities) {
                        var entity = entityRange[newIdentity.EntityIndex];
                        entity.Id = newIdentity.ID;
                        entity.IsDirty = false;
                    }
                }
                entityList = entityList.GetRange(numberPerBatch, entityList.Count - numberPerBatch);
            }
        }

        /// <inheritdoc />
        public void Update<T>(DatabaseCommandBuilder<T> commandBuilder) where T : BaseEntity {
            var builder = commandBuilder.QueryBuilder;
            var entityList = new List<T>(commandBuilder.Entities.ToSafeList());
            while (entityList.HasAny()) {
                var command = _databaseCommandFactory.Build(builder);
                var entityRange = PrepareEntityData(commandBuilder, entityList, command);
                var numberPerBatch = entityRange.Count;
                if (!command.CommandText.IsNullOrWhiteSpace()) {
                    ExecuteNonQuery(command);
                }
                foreach (var entity in entityRange) {
                    entity.IsDirty = false;
                }
                entityList = entityList.GetRange(numberPerBatch, entityList.Count - numberPerBatch);
            }
        }

        private static List<T> PrepareEntityData<T>(DatabaseCommandBuilder<T> commandBuilder,
                                                    List<T> entityList, DatabaseCommand command) where T : BaseEntity {
            var builder = commandBuilder.QueryBuilder;
            var amountPerBatchResult = builder.CalculateAmountPerBatch(entityList);
            var numberPerBatch = amountPerBatchResult.Item1;
            var valuesPerType = amountPerBatchResult.Item2;
            var entityRange = entityList.GetRange(0, numberPerBatch);
            command.CommandTimeout = commandBuilder.Timeout;
            builder.SetParameters(command, Tuple.Create(valuesPerType, (IEnumerable)entityRange));
            return entityRange;
        }

        /// <inheritdoc />
        public void UpdateProperty(DatabaseCommand command) {
            ExecuteNonQuery(command);
        }

        public bool Delete(DatabaseCommand command) {
            ExecuteNonQuery(command);
            return true;
        }

        public bool Exists(DatabaseCommand command) {
            var count = ExecuteScalar(command).To<int>();
            return count != 0;
        }

        public void Execute(DatabaseCommand command) {
            ExecuteNonQuery(command);
        }

        private BaseEntityParser<T> CreateParser<T>() where T : BaseEntity => new BaseEntityParser<T>(_tableExistsRepository);
    }

    [ResultTable]
    internal class InsertInformation : BaseResult {
        [Column]
        public int EntityIndex { get; set; }

        [Column]
        public int ID { get; set; }
    }
}