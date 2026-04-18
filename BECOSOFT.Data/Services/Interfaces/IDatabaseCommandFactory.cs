using BECOSOFT.Data.Converters;
using BECOSOFT.Data.Models;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Data.Models.QueryObjects;
using BECOSOFT.Data.Query;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace BECOSOFT.Data.Services.Interfaces {
    public interface IDatabaseCommandFactory : IBaseService {
        DatabaseCommand Create();
        DatabaseCommand Build(BaseQueryBuilder builder);
        DatabaseCommand GetCommand<T>(DatabaseCommandBuilder<T> commandBuilder);
        DatabaseCommand Select<T>(QueryExpressionQueryObject queryObject);
        DatabaseCommand Select<T>(Expression expression, string tablePart, bool distinct, int timeout = 60);
        DatabaseCommand Select<T>(long id, string tablePart, int timeout = 60);

        DatabaseCommand Select<T>(EntityTypeInfo entityTypeInfo, Type resultType, QueryExpression<T> queryObject,
                                  string tablePart, int timeout = 60);

        DatabaseCommand Custom(ParametrizedQuery query);
        DatabaseCommand Custom<T>(ParametrizedQuery query);
        DatabaseCommandBuilder<T> Insert<T>(IEnumerable<T> entities, string tablePart, int timeout = 60);

        DatabaseCommandBuilder<T> Update<T>(IEnumerable<T> entities, IEnumerable<EntityPropertyInfo> selectedProperties,
                                            string tablePart, int timeout = 60);

        DatabaseCommand Update<T, TProp>(int id, EntityPropertyInfo property, TProp value, string tablePart, int timeout = 60);
        DatabaseCommand Update<T, TProp>(IEnumerable<int> ids, EntityPropertyInfo property, TProp value, string tablePart, int timeout = 60) where T : BaseEntity;
        DatabaseCommand Delete<T>(Expression<Func<T, bool>> expression, string tablePart, int timeout = 60);
        DatabaseCommand Delete<T>(string property, object value, string tablePart, int timeout = 60);
        DatabaseCommand Delete<T>(object entity, string tablePart, int timeout = 60);
        DatabaseCommand Exists<T>(long id, string tablePart, int timeout = 60) where T : BaseEntity;
        DatabaseCommand Exists<T>(Expression<Func<T, bool>> where, string tablePart, int timeout = 60);
        DatabaseCommand Execute(ParametrizedQuery query, int timeout = 60);
    }
}