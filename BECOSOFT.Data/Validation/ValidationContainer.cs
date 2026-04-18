using BECOSOFT.Data.Models;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Utilities.Converters;
using BECOSOFT.Utilities.Extensions.Collections;
using System.Collections.Generic;

namespace BECOSOFT.Data.Validation {
    public class ValidationContainer<TEntity, TDefining> : IValidationContainer<TEntity, TDefining>
        where TDefining : TableDefiningEntity {
        private readonly List<ErrorList> _errorList;

        public TDefining Definition { get; }

        public IReadOnlyList<TEntity> Entities { get; }

        public IReadOnlyList<IErrorList> ErrorList => _errorList;
        public IReadonlyPrimaryKeyContainer PrimaryKeyContainer { get; }
        public ISaveOptions Options { get; }

        public ValidationContainer(TDefining definition, IReadOnlyList<TEntity> entities,
                                   IReadonlyPrimaryKeyContainer primaryKeyContainer, ISaveOptions options) {
            Definition = definition;
            Entities = entities;
            PrimaryKeyContainer = primaryKeyContainer;
            _errorList = new List<ErrorList>(entities.Count);
            for (var i = 0; i < entities.Count; i++) {
                _errorList.Add(new ErrorList());
            }
            Options = options;
        }

        public IValidationContainer<TEntity, TableDefiningEntity> AsTableDefining() {
            return this;
        }

        public TSaveOptions GetSaveOptions<TSaveOptions>() where TSaveOptions : SaveOptions, ISaveOptions {
            if (Options != null) {
                return (TSaveOptions)Options;
            }
            var def = TypeActivator<TSaveOptions>.Instance();
            return (TSaveOptions)def.GetDefault();
        }

        public IValidationContainer<TNewEntity> ToEntitySubContainer<TNewEntity, TNewOptions>(IReadOnlyList<TNewEntity> entities, TNewOptions options) where TNewOptions : ISaveOptions {
            return new ValidationContainer<TNewEntity>(entities.ToSafeList(), PrimaryKeyContainer, options);
        }

        public IValidationContainer<TNewEntity, TDefining> ToSubContainer<TNewEntity, TNewOptions>(IReadOnlyList<TNewEntity> entities, 
                                                                                                   TNewOptions options)
            where TNewOptions : ISaveOptions {
            return new ValidationContainer<TNewEntity, TDefining>(Definition, entities.ToSafeList(), PrimaryKeyContainer, options);
        }
    }

    public sealed class ValidationContainer<TEntity> : ValidationContainer<TEntity, TableDefiningEntity> {
        public ValidationContainer(IReadOnlyList<TEntity> entities,
                                   IReadonlyPrimaryKeyContainer primaryKeyContainer, ISaveOptions options = null)
            : base(null, entities, primaryKeyContainer, options) {
        }

        public ValidationContainer(TEntity entity,
                                   IReadonlyPrimaryKeyContainer primaryKeyContainer, ISaveOptions options = null)
            : base(null, new List<TEntity> { entity }, primaryKeyContainer, options) {
        }
    }
}