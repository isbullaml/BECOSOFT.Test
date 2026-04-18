using BECOSOFT.Data.Models;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Utilities.Collections;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace BECOSOFT.Data.Services.Interfaces.QueryData {
    public interface ITableQueryService : IBaseService {
        bool TableExists<T>(string tablePart = null) where T : IEntity;
        bool TableExists(Schema schema, string tableName, string tablePart = null);
        bool ViewExists<T>(string tablePart = null) where T : IEntity;
        bool ViewExists(Schema schema, string tableName, string tablePart = null);
        bool ColumnExists<T, TProp>(Expression<Func<T, TProp>> columnSelector, string tablePart = null) where T : IEntity;
        bool ColumnExists<T>(string column, string tablePart = null) where T : IEntity;
        bool ColumnExists(Schema schema, string tableName, string column, string tablePart = null);
        bool HasRows<T>(string tablePart = null) where T: IEntity;
        bool HasRows(Schema schema, string tableName, string tablePart = null);
        Dictionary<TypeTablePartDefinition, bool> HasRows(KeyValueList<Type, string> typeTablePartValues);
        bool HasIdentity<T>(string tablePart = null) where T: IEntity;
        bool HasIdentity(Schema schema, string tableName, string tablePart = null);
    }
}