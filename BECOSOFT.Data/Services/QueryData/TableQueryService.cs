using BECOSOFT.Data.Models;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Data.Repositories.Interfaces.QueryData;
using BECOSOFT.Data.Services.Interfaces.QueryData;
using BECOSOFT.Utilities.Collections;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace BECOSOFT.Data.Services.QueryData {
    public sealed class TableQueryService : ITableQueryService {
        private readonly ITableQueryRepository _repository;

        internal TableQueryService(ITableQueryRepository repository) {
            _repository = repository;
        }

        public bool TableExists<T>(string tablePart = null) where T : IEntity {
            return _repository.TableExists<T>(tablePart);
        }

        public bool TableExists(Schema schema, string tableName, string tablePart = null) {
            return _repository.TableExists(schema, tableName, tablePart);
        }

        public bool ViewExists<T>(string tablePart = null) where T : IEntity {
            return _repository.ViewExists<T>(tablePart);
        }

        public bool ViewExists(Schema schema, string tableName, string tablePart = null) {
            return _repository.ViewExists(schema, tableName, tablePart);
        }

        public bool ColumnExists<T, TProp>(Expression<Func<T, TProp>> columnSelector, string tablePart = null) where T : IEntity {
            return _repository.ColumnExists(columnSelector, tablePart);
        }

        public bool ColumnExists<T>(string column, string tablePart = null) where T : IEntity {
            return _repository.ColumnExists<T>(column, tablePart);
        }

        public bool ColumnExists(Schema schema, string tableName, string column, string tablePart = null) {
            return _repository.ColumnExists(schema, tableName, column, tablePart);
        }

        public bool HasRows<T>(string tablePart = null) where T : IEntity {
            return _repository.HasRows<T>(tablePart);
        }

        public bool HasRows(Schema schema, string tableName, string tablePart = null) {
            return _repository.HasRows(schema, tableName, tablePart);
        }

        public Dictionary<TypeTablePartDefinition, bool> HasRows(KeyValueList<Type, string> typeTablePartValues) {
            return _repository.HasRows(typeTablePartValues);
        }

        public bool HasIdentity<T>(string tablePart = null) where T : IEntity {
            return _repository.HasIdentity<T>(tablePart);
        }

        public bool HasIdentity(Schema schema, string tableName, string tablePart = null) {
            return _repository.HasIdentity(schema, tableName, tablePart);
        }
    }
}
