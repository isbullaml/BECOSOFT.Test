using BECOSOFT.Data.Models;
using BECOSOFT.Data.Models.Base;
using System;
using System.Collections.Generic;

namespace BECOSOFT.Data.Validation {
    public interface ISaveContainer<out TEntity, out TDefining> : ISaveContainer<TEntity>
        where TDefining : TableDefiningEntity {
        TDefining Definition { get; }

        ISaveContainer<TNewEntity, TDefining> ToSubContainer<TNewEntity, TNewOptions>(IReadOnlyList<TNewEntity> entities, TNewOptions options) where TNewOptions : ISaveOptions;
        IValidationContainer<TEntity, TDefining> ToValidationContainer();
    }

    public interface ISaveContainer<out TEntity> {
        IReadOnlyList<TEntity> Entities { get; }
        IReadonlyPrimaryKeyContainer PrimaryKeyContainer { get; }
        ISaveOptions SaveOptions { get; }
        TSaveOptions GetSaveOptions<TSaveOptions>() where TSaveOptions : SaveOptions, ISaveOptions;
        ISaveOptions GetSubSaveOptions(Type subType);
        void SetOptions(ISaveOptions options);
        IValidationContainer<TEntity> ToEntityValidationContainer();
        ISaveContainer<TNewEntity> ToEntitySubContainer<TNewEntity, TNewOptions>(IReadOnlyList<TNewEntity> entities, TNewOptions options) where TNewOptions : ISaveOptions;
    }
}