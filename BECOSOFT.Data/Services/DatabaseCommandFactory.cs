using BECOSOFT.Data.Converters;
using BECOSOFT.Data.Models;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Data.Models.QueryObjects;
using BECOSOFT.Data.Query;
using BECOSOFT.Utilities.Extensions.Collections;
using BECOSOFT.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using BECOSOFT.Data.Helpers;
using BECOSOFT.Data.Services.Interfaces;

namespace BECOSOFT.Data.Services {
    internal class DatabaseCommandFactory : IDatabaseCommandFactory {
        private readonly IQueryBuilderFactory _queryBuilderFactory;

        internal DatabaseCommandFactory(IQueryBuilderFactory queryBuilderFactory) {
            _queryBuilderFactory = queryBuilderFactory;
        }

        public DatabaseCommand Create() {
            return new DatabaseCommand();
        }

        public DatabaseCommand Build(BaseQueryBuilder builder) {
            var command = Create();
            builder.PrepareCommand(command);
            command.IsPrepared = true;
            return command;
        }

        public DatabaseCommand GetCommand<T>(DatabaseCommandBuilder<T> commandBuilder) {
            return Build(commandBuilder.QueryBuilder);
        }

        public DatabaseCommand Select<T>(QueryExpressionQueryObject queryObject) {
            var expression = (queryObject.QueryExpression ?? new QueryExpression<T>()).ToExpression();
            var queryInfo = new QueryInfo {
                TypeInfo = EntityConverter.GetEntityTypeInfo(typeof(T)),
                Expression = expression,
                TablePart = queryObject.TablePart,
                IsDistinct = queryObject.Distinct,
            };
            var builder = _queryBuilderFactory.GetBuilder(QueryType.Select, queryInfo);
            var command = Build(builder);
            command.CommandTimeout = queryObject.Timeout;
            return command;
        }

        /// <summary>
        /// Execute an expression that will be mapped to a child of BaseEntity
        /// Properties that are not present in the query will be ignored, and thus have an optional value
        /// </summary>
        /// <typeparam name="T">Child class of BaseEntity</typeparam>
        /// <param name="expression">The expression</param> 
        /// <param name="tablePart">The tablepart on which the query is executed</param>    
        /// <param name="distinct">Specifies if the results should be distinct</param>
        /// <param name="timeout">The timeout for the query</param>
        /// <returns>A pagedlist of T</returns>
        public DatabaseCommand Select<T>(Expression expression, string tablePart, bool distinct, int timeout = 60) {
            var queryInfo = new QueryInfo {
                TypeInfo = EntityConverter.GetEntityTypeInfo(typeof(T)),
                Expression = expression,
                TablePart = tablePart,
                IsDistinct = distinct
            };
            var builder = _queryBuilderFactory.GetBuilder(QueryType.Select, queryInfo);
            var command = Build(builder);
            command.CommandTimeout = timeout;
            return command;
        }

        /// <summary>
        /// Retrieves an entity T
        /// </summary>
        /// <typeparam name="T">The type of entity</typeparam>
        /// <param name="id">The id of the entity</param>
        /// <param name="tablePart">The tablepart to search in</param>
        /// <param name="timeout">The timeout for the query</param>
        /// <returns>Retrieved entity</returns>
        public DatabaseCommand Select<T>(long id, string tablePart, int timeout = 60) {
            var queryInfo = new QueryInfo {
                TypeInfo = EntityConverter.GetEntityTypeInfo(typeof(T)),
                Entity = id,
                TablePart = tablePart,
                SelectedProperties = new List<EntityPropertyInfo>(0)
            };
            var builder = _queryBuilderFactory.GetBuilder(QueryType.Select, queryInfo);
            var command = Build(builder);
            command.CommandTimeout = timeout;
            return command;
        }

        /// <summary>
        /// Only select properties from an entity
        /// </summary>
        /// <typeparam name="T">The base-entity type</typeparam>
        /// <param name="entityTypeInfo">The type-info of the original base-entity</param>
        /// <param name="resultType">The anonymous type</param>
        /// <param name="queryObject">The query object containing the where and pagination</param>
        /// <param name="tablePart">The tablepart</param>
        /// <param name="timeout">The timeout for the query</param>
        /// <returns>A list of the found anonymous entities</returns>
        public DatabaseCommand Select<T>(EntityTypeInfo entityTypeInfo, Type resultType, QueryExpression<T> queryObject,
                                         string tablePart, int timeout = 60) {
            var selectProperties = GetSelectProperties(resultType, entityTypeInfo, tablePart);
            var queryInfo = new QueryInfo {
                TypeInfo = EntityConverter.GetEntityTypeInfo(typeof(T)),
                Expression = queryObject?.ToExpression(),
                TablePart = tablePart,
                IsDistinct = true,
                SelectedProperties = selectProperties?.ToList() ?? new List<EntityPropertyInfo>(0)
            };
            var builder = _queryBuilderFactory.GetBuilder(QueryType.Select, queryInfo);
            var command = Build(builder);
            command.CommandTimeout = timeout;
            return command;
        }

        /// <summary>
        /// Execute an expression that will be mapped to a child of T
        /// Properties that are not present in the query will be ignored, and thus have an optional value
        /// </summary>
        /// <param name="query">The query</param> 
        /// <returns>A pagedlist of T</returns>
        public DatabaseCommand Custom(ParametrizedQuery query) {
            var queryInfo = query.ToQueryInfo();
            var builder = _queryBuilderFactory.GetBuilder(QueryType.Custom, queryInfo);
            var command = Build(builder);
            command.CommandTimeout = query.Timeout;
            return command;
        }

        /// <summary>
        /// Execute a query that will be mapped to a child of BaseEntity
        /// Properties that are not present in the query will be ignored, and thus have an optional value
        /// </summary>
        /// <typeparam name="T">Child class of BaseResult</typeparam>
        /// <param name="query">The query</param> 
        /// <returns>A pagedlist of T</returns>
        public DatabaseCommand Custom<T>(ParametrizedQuery query) {
            var queryInfo = query.ToQueryInfo();
            queryInfo.TypeInfo = EntityConverter.GetEntityTypeInfo(typeof(T));
            var builder = _queryBuilderFactory.GetBuilder(QueryType.Custom, queryInfo);
            var command = Build(builder);
            command.CommandTimeout = query.Timeout;
            return command;
        }

        /// <summary>
        /// Insert a list of entities
        /// </summary>
        /// <typeparam name="T">The type of entity</typeparam>
        /// <param name="entities">The list of entities to insert</param>
        /// <param name="tablePart">The tablepart to insert in</param>
        /// <param name="timeout">The timeout for the query</param>
        public DatabaseCommandBuilder<T> Insert<T>(IEnumerable<T> entities, string tablePart, int timeout = 60) {
            var builder = _queryBuilderFactory.GetBuilder(QueryType.Insert, new QueryInfo {
                TypeInfo = EntityConverter.GetEntityTypeInfo(typeof(T)),
                TablePart = tablePart,
            });
            return new DatabaseCommandBuilder<T> {
                QueryBuilder = builder,
                Entities = entities,
                Timeout = timeout,
            };
        }

        /// <summary>
        /// Update a list of entities
        /// </summary>
        /// <typeparam name="T">The type of entity</typeparam>
        /// <param name="entities">The list of entities to update</param>
        /// <param name="selectedProperties">The properties to use in the query, NULL means all properties</param>
        /// <param name="tablePart">The tablepart to update in</param>
        /// <param name="timeout">The timeout for the query</param>
        public DatabaseCommandBuilder<T> Update<T>(IEnumerable<T> entities, IEnumerable<EntityPropertyInfo> selectedProperties,
                                                   string tablePart, int timeout = 60) {
            var queryInfo = new QueryInfo {
                TypeInfo = EntityConverter.GetEntityTypeInfo(typeof(T)),
                TablePart = tablePart,
                SelectedProperties = selectedProperties?.ToList() ?? new List<EntityPropertyInfo>(),
            };
            var builder = _queryBuilderFactory.GetBuilder(QueryType.Update, queryInfo);
            return new DatabaseCommandBuilder<T> {
                QueryBuilder = builder,
                Entities = entities,
                Timeout = timeout,
            };
        }

        /// <summary>
        /// Update a single property on a single entity based on the primary key of that entity
        /// </summary>
        /// <typeparam name="T">The type of entity</typeparam>
        /// <typeparam name="TProp">The type of the property to update</typeparam>
        /// <param name="id">The id of the entity</param>
        /// <param name="property">The info about the property</param>
        /// <param name="value">The value to set</param>
        /// <param name="tablePart">The tablepart to update in</param>
        /// <param name="timeout">The timeout for the query</param>
        public DatabaseCommand Update<T, TProp>(int id, EntityPropertyInfo property, TProp value, string tablePart, int timeout = 60) {
            var queryInfo = new QueryInfo {
                TypeInfo = EntityConverter.GetEntityTypeInfo(typeof(T)),
                ColumnName = property.ColumnName,
                EntityID = id,
                Value = value,
                TablePart = tablePart,
            };
            var builder = _queryBuilderFactory.GetBuilder(QueryType.UpdateProperty, queryInfo);
            var command = Build(builder);
            command.CommandTimeout = timeout;
            return command;
        }

        /// <summary>
        /// Update a single property on a list of entities based on the primary key of that entity
        /// </summary>
        /// <typeparam name="T">The type of entity</typeparam>
        /// <typeparam name="TProp">The type of the property to update</typeparam>
        /// <param name="ids">The ids of the entities</param>
        /// <param name="property">The info about the property</param>
        /// <param name="value">The value to set</param>
        /// <param name="tablePart">The tablepart to update in</param>
        /// <param name="timeout">The timeout for the query</param>
        public DatabaseCommand Update<T, TProp>(IEnumerable<int> ids, EntityPropertyInfo property, TProp value, string tablePart, int timeout = 60) where T : BaseEntity {
            Expression<Func<T, bool>> expression = ent => ids.Contains(ent.Id);
            var queryInfo = new QueryInfo {
                TypeInfo = EntityConverter.GetEntityTypeInfo(typeof(T)),
                ColumnName = property.ColumnName,
                Expression = expression,
                Value = value,
            };
            var builder = _queryBuilderFactory.GetBuilder(QueryType.UpdateProperty, queryInfo);
            var command = Build(builder);
            command.CommandTimeout = timeout;
            return command;
        }

        /// <summary>
        /// Delete entities based on an expression
        /// </summary>
        /// <typeparam name="T">The type of entity</typeparam>
        /// <param name="expression">The expression which determinates the entities to delete</param>
        /// <param name="tablePart">The tablepart to delete in</param>
        /// <param name="timeout">The timeout for the query</param>
        /// <returns>Value indicating whether the delete-action was successful</returns>
        public DatabaseCommand Delete<T>(Expression<Func<T, bool>> expression, string tablePart, int timeout = 60) {
            var queryInfo = new QueryInfo {
                TypeInfo = EntityConverter.GetEntityTypeInfo(typeof(T)),
                Expression = expression,
                TablePart = tablePart,
            };
            var builder = _queryBuilderFactory.GetBuilder(QueryType.Delete, queryInfo);
            var command = Build(builder);
            command.CommandTimeout = timeout;
            return command;
        }

        /// <summary>
        /// Delete entities based on a property and it's value
        /// </summary>
        /// <typeparam name="T">The type of entity</typeparam>
        /// <param name="property">The property to check</param>
        /// <param name="value">The value on which to delete</param>
        /// <param name="tablePart">The tablepart to delete in</param>
        /// <param name="timeout">The timeout for the query</param>
        /// <returns>Value indicating whether the delete-action was successful</returns>
        public DatabaseCommand Delete<T>(string property, object value, string tablePart, int timeout = 60) {
            var queryInfo = new QueryInfo {
                TypeInfo = EntityConverter.GetEntityTypeInfo(typeof(T)),
                Entity = value,
                ColumnName = property,
                TablePart = tablePart,
            };
            var builder = _queryBuilderFactory.GetBuilder(QueryType.Delete, queryInfo);
            var command = Build(builder);
            command.CommandTimeout = timeout;
            return command;
        }


        /// <summary>
        /// Delete an entity based on the entity
        /// </summary>
        /// <typeparam name="T">The type of entity</typeparam>
        /// <param name="entity">The entity to delete</param>
        /// <param name="tablePart">The tablepart to delete in</param>
        /// <param name="timeout">The timeout for the query</param>
        /// <returns>Value indicating whether the delete-action was successful</returns>
        public DatabaseCommand Delete<T>(object entity, string tablePart, int timeout = 60) {
            var queryInfo = new QueryInfo {
                TypeInfo = EntityConverter.GetEntityTypeInfo(typeof(T)),
                Entity = entity,
                TablePart = tablePart,
                SelectedProperties = new List<EntityPropertyInfo>(0)
            };
            var builder = _queryBuilderFactory.GetBuilder(QueryType.Delete, queryInfo);
            var command = Build(builder);
            command.CommandTimeout = timeout;
            return command;
        }

        /// <summary>
        /// Check if an entity exists based on it's id
        /// </summary>
        /// <typeparam name="T">The type of entity</typeparam>
        /// <param name="id">The id to check</param>
        /// <param name="tablePart">The tablepart to check in</param>
        /// <param name="timeout">The timeout for the query</param>
        /// <returns>Value indicating whether an entity exists with the id</returns>
        public DatabaseCommand Exists<T>(long id, string tablePart, int timeout = 60) where T : BaseEntity {
            var expressionBuilder = new ExistsExpressionBuilder<T>(id);
            return Exists(expressionBuilder.ToExpression(), tablePart, timeout);
        }

        /// <summary>
        /// Check if an entity exists based on an expression
        /// </summary>
        /// <typeparam name="T">The type of entity</typeparam>
        /// <param name="where">The WHERE-expression</param>
        /// <param name="tablePart">The tablepart to check in</param>
        /// <param name="timeout">The timeout for the query</param>
        /// <returns>Value indicating whether an entity exists based on the expression</returns>
        public DatabaseCommand Exists<T>(Expression<Func<T, bool>> where, string tablePart, int timeout = 60) {
            var queryInfo = new QueryInfo {
                TypeInfo = EntityConverter.GetEntityTypeInfo(typeof(T)),
                Expression = where,
                TablePart = tablePart,
            };
            var builder = _queryBuilderFactory.GetBuilder(QueryType.Exists, queryInfo);
            var command = Build(builder);
            command.CommandTimeout = timeout;
            return command;
        }

        /// <summary>
        /// Executes a query without returning a result.
        /// </summary>
        /// <param name="query">The query</param>
        /// <param name="timeout">The timeout for the query</param>
        public DatabaseCommand Execute(ParametrizedQuery query, int timeout = 60) {
            var builder = _queryBuilderFactory.GetBuilder(QueryType.Custom, query.ToQueryInfo());
            var command = Build(builder);
            command.CommandTimeout = timeout;
            return command;
        }

        private static List<EntityPropertyInfo> GetSelectProperties(Type resultType, EntityTypeInfo entityTypeInfo, string tablePart) {
            var anonymousProperties = resultType.GetProperties();
            var matchingProperties = new List<EntityPropertyInfo>();

            foreach (var anonymousProperty in anonymousProperties) {
                var anonymousPropertyName = anonymousProperty.Name.ToLowerInvariant();
                foreach (var property in entityTypeInfo.Properties) {
                    if (property.IsBaseChild) {
                        var baseChildPropertyInfo = EntityConverter.GetEntityTypeInfo(property.PropertyType);
                        var propertyInfo = baseChildPropertyInfo.GetPropertyInfo(anonymousPropertyName, tablePart);

                        if (propertyInfo != null) {
                            matchingProperties.Add(propertyInfo);
                        }
                    } else if (property.PropertyName.EqualsIgnoreCase(anonymousProperty.Name) || property.ColumnName.EqualsIgnoreCase(anonymousPropertyName)) {
                        matchingProperties.Add(property);
                    }
                }
            }

            if (matchingProperties.IsEmpty()) {
                throw new Exception("No matching property found on the entity.");
            }

            var primaryKeyInfo = entityTypeInfo.PrimaryKeyInfo ?? entityTypeInfo.TablePrimaryKeyInfo;
            if (!matchingProperties.Any(p => primaryKeyInfo.ColumnName.Equals(p.ColumnName))) {
                matchingProperties.Add(primaryKeyInfo);
            }

            return matchingProperties;
        }
    }
}
