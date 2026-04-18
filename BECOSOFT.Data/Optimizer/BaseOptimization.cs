using BECOSOFT.Data.Parsers;
using BECOSOFT.Utilities.Converters;
using BECOSOFT.Utilities.Extensions.Collections;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;

namespace BECOSOFT.Data.Optimizer {
    public abstract class BaseOptimization<T> : IOptimization<T> where T : Enum {
        private readonly ILogger _logger;
        public T OptimizationType { get; }

        public IDbConnection Connection { get; private set; }
        public IDbTransaction Transaction { get; set; }
        public bool LogIndexStatistics { get; set; }
        protected internal BaseOptimization(T optimizationType, ILogger logger) {
            OptimizationType = optimizationType;
            _logger = logger;
        }

        protected abstract void Optimize();

        public void RunOptimization(IDbConnection connection, bool logIndexStatistics) {
            Connection = connection;
            LogIndexStatistics = logIndexStatistics;
            Optimize();
        }

        protected void Execute(string sql, int? commandTimeout = null) {
            _logger.Trace(sql);
            using (var command = CreateCommand(sql, commandTimeout)) {
                using (var reader = command.ExecuteReader()) {
                    if (!LogIndexStatistics) {
                        return;
                    }

                    do {
                        while (reader.Read()) {
                            var indexName = reader["IndexName"].To<string>();
                            var averageFragmentation = reader["AverageFragmentation"].To<decimal>();
                            var pageCount = reader["PageCount"].To<long>();
                            var minimumReorganize = reader["MinimumReorganize"].To<int>();
                            var minimumRebuild = reader["MinimumRebuild"].To<int>();
                            var minimumPageCount = reader["MinimumPageCount"].To<int>();

                            _logger.Info($"Optimizing index '{indexName}': Avg. Fragmentation {averageFragmentation} (Reorganize from {minimumReorganize}, Rebuild from {minimumRebuild}) - Page Count {pageCount} (Min {minimumPageCount})");
                        }
                    } while (reader.NextResult());
                }
            }
        }

        protected IDbCommand CreateCommand(string sql, int? commandTimeout = null) {
            if (Connection == null) {
                throw new InvalidOperationException();
            }
            var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Transaction = Transaction;

            command.CommandTimeout = commandTimeout ?? 600;
            return command;
        }

        protected List<TConvertible> SelectConvertible<TConvertible>(string sql) where TConvertible : IConvertible {
            _logger.Trace(sql);
            using (var command = CreateCommand(sql)) {
                using (var reader = command.ExecuteReader()) {
                    var parser = new ConvertibleParser<TConvertible>();
                    return parser.SelectConvertible(reader).ToSafeList();
                }
            }
        }
    }
}