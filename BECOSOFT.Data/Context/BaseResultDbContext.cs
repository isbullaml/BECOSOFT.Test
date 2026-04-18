using BECOSOFT.Data.Collections;
using BECOSOFT.Data.Models;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Data.Parsers;
using BECOSOFT.Data.Query;
using BECOSOFT.Data.Repositories.Interfaces;
using System;
using System.Data;

namespace BECOSOFT.Data.Context {
    /// <inheritdoc cref="IBaseResultDbContext" />
    internal sealed class BaseResultDbContext : DbContext, IBaseResultDbContext {
        private readonly IOfflineTableExistsRepository _tableExistsRepository;

        public BaseResultDbContext(IDbConnection dbConnection,
                                   IBulkCopyHelper bulkCopyHelper,
                                   IOfflineTableExistsRepository tableExistsRepository)
            : base(dbConnection, bulkCopyHelper) {
            _tableExistsRepository = tableExistsRepository;
        }

        /// <inheritdoc />
        public IPagedList<T> Query<T>(DatabaseCommand command) where T : BaseResult {
            using (var reader = ExecuteReader(command)) {
                var parser = CreateParser<T>();
                return parser.Select(reader, command.TablePart);
            }
        }

        /// <inheritdoc />
        public IPagedList<T> QueryConvertible<T>(DatabaseCommand command) where T : IConvertible {
            using (var reader = ExecuteReader(command)) {
                var parser = new ConvertibleParser<T>();
                return parser.SelectConvertible(reader);
            }
        }

        /// <inheritdoc />
        public IPagedList<TResult> GetProperties<T, TResult>(DatabaseCommand command) where T : BaseResult where TResult : class {
            using (var reader = ExecuteReader(command)) {
                var parser = CreateParser<T>();
                return parser.SelectCustom<TResult>(reader);
            }
        }

        private BaseResultParser<T> CreateParser<T>() where T : BaseResult => new BaseResultParser<T>(_tableExistsRepository);
    }
}