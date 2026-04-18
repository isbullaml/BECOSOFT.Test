using BECOSOFT.Data.Models;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Utilities.Converters;
using BECOSOFT.Utilities.Extensions.Collections;
using System;
using System.Collections.Generic;

namespace BECOSOFT.Data.Validation {
    public class SaveContainer<TEntity, TDefining> : ISaveContainer<TEntity, TDefining>
        where TDefining : TableDefiningEntity {
        public TDefining Definition { get; }

        public IReadOnlyList<TEntity> Entities { get; }
        public IReadonlyPrimaryKeyContainer PrimaryKeyContainer { get; }
        public ISaveOptions SaveOptions { get; private set; }

        public SaveContainer(TDefining definition, IReadOnlyList<TEntity> entities,
                             IReadonlyPrimaryKeyContainer primaryKeyContainer, ISaveOptions options) {
            Definition = definition;
            Entities = entities;
            PrimaryKeyContainer = primaryKeyContainer;
            SaveOptions = options;
        }

        public SaveContainer(TDefining definition, TEntity entity,
                             IReadonlyPrimaryKeyContainer primaryKeyContainer, ISaveOptions options) {
            Definition = definition;
            Entities = new List<TEntity> { entity };
            PrimaryKeyContainer = primaryKeyContainer;
            SaveOptions = options;
        }

        public SaveContainer(TDefining definition, TEntity entity, ISaveOptions options)
            : this(definition, entity, null, options) {
        }

        public SaveContainer(TDefining definition, IReadOnlyList<TEntity> entities, ISaveOptions options)
            : this(definition, entities, null, options) {
        }

        public TSaveOptions GetSaveOptions<TSaveOptions>() where TSaveOptions : SaveOptions, ISaveOptions {
            if (SaveOptions != null) {
                return (TSaveOptions)SaveOptions;
            }
            var def = TypeActivator<TSaveOptions>.Instance();
            return (TSaveOptions)def.GetDefault();
        }

        public ISaveOptions GetSubSaveOptions(Type subType) {
            return SaveOptions?.GetSubOptions(subType);
        }

        public void SetOptions(ISaveOptions options) {
            if (options == null) { return; }
            SaveOptions = options;
        }

        public ISaveContainer<TNewEntity> ToEntitySubContainer<TNewEntity, TNewOptions>(IReadOnlyList<TNewEntity> entities, TNewOptions options) where TNewOptions : ISaveOptions {
            var container = new SaveContainer<TNewEntity>(entities, PrimaryKeyContainer, options);
            return container;
        }

        public ISaveContainer<TNewEntity, TDefining> ToSubContainer<TNewEntity, TNewOptions>(IReadOnlyList<TNewEntity> entities, TNewOptions options) where TNewOptions : ISaveOptions {
            var container = new SaveContainer<TNewEntity, TDefining>(Definition, entities, PrimaryKeyContainer, options);
            return container;
        }

        public IValidationContainer<TEntity> ToEntityValidationContainer() {
            return new ValidationContainer<TEntity>(Entities.ToSafeList(), PrimaryKeyContainer, SaveOptions);
        }

        public IValidationContainer<TEntity, TDefining> ToValidationContainer() {
            return new ValidationContainer<TEntity, TDefining>(Definition, Entities.ToSafeList(), PrimaryKeyContainer, SaveOptions);
        }
    }

    public class SaveContainer<TEntity> : SaveContainer<TEntity, TableDefiningEntity> {
        public SaveContainer(IReadOnlyList<TEntity> entities, IReadonlyPrimaryKeyContainer primaryKeyContainer, ISaveOptions options) : base(null, entities, primaryKeyContainer, options) {
        }

        public SaveContainer(TEntity entity, IReadonlyPrimaryKeyContainer primaryKeyContainer, ISaveOptions options) : base(null, entity, primaryKeyContainer, options) {
        }

        public SaveContainer(IReadOnlyList<TEntity> entities, ISaveOptions options) : base(null, entities, options) {
        }
    }
}