using BECOSOFT.Data.Exceptions;
using BECOSOFT.Data.Models;
using BECOSOFT.Data.Query;
using BECOSOFT.Data.Services.Interfaces;
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Helpers;
using NLog;
using System;

namespace BECOSOFT.Data.Services
{
    // ReSharper disable once UnusedMember.Global

    /// <inheritdoc cref="IQueryBuilderFactory"/>
    internal class QueryBuilderFactory : IQueryBuilderFactory {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly IFactoryService<QueryType, IQueryBuilderFactoryService> _queryBuilderFactoryService;

        public QueryBuilderFactory(IFactoryService<QueryType, IQueryBuilderFactoryService> queryBuilderFactoryService) {
            _queryBuilderFactoryService = queryBuilderFactoryService;
        }


        public BaseQueryBuilder GetBuilder(QueryType type, QueryInfo info) {
            if (!type.IsDefined()) {
                throw new UnknownQueryTypeException($"{type} is unknown");
            }
            if (ShouldCheckTypeInfo(type) && info.TypeInfo == null) {
                throw new ArgumentException("{0} missing".FormatWith(nameof(info.TypeInfo)), nameof(info));
            }
            try {
                var queryBuilder = _queryBuilderFactoryService.GetInstance(type);
                return queryBuilder.Initialize(info);
            } catch (Exception ex) {
                Logger.Error(ex);
                throw new UnknownQueryTypeException($"Exception for {type}", ex);
            }
        }

        private static bool ShouldCheckTypeInfo(QueryType type) {
            return type != QueryType.Custom;
        }
    }
}