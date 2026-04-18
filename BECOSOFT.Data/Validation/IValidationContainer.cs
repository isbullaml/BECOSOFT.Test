using BECOSOFT.Data.Models;
using BECOSOFT.Data.Models.Base;
using System.Collections.Generic;

namespace BECOSOFT.Data.Validation {
    public interface IValidationContainer<out TEntity> {
        IReadOnlyList<TEntity> Entities { get; }
        IReadOnlyList<IErrorList> ErrorList { get; }
        IReadonlyPrimaryKeyContainer PrimaryKeyContainer { get; }
        IValidationContainer<TEntity, TableDefiningEntity> AsTableDefining();
        ISaveOptions Options { get; }
        TSaveOptions GetSaveOptions<TSaveOptions>() where TSaveOptions : SaveOptions, ISaveOptions;
        IValidationContainer<TNewEntity> ToEntitySubContainer<TNewEntity, TNewOptions>(IReadOnlyList<TNewEntity> entities, TNewOptions options) where TNewOptions : ISaveOptions;
    }

    public interface IValidationContainer<out TEntity, out TDefining> : IValidationContainer<TEntity>
        where TDefining : TableDefiningEntity {
        TDefining Definition { get; }
        IValidationContainer<TNewEntity, TDefining> ToSubContainer<TNewEntity, TNewOptions>(IReadOnlyList<TNewEntity> entities, TNewOptions options) where TNewOptions : ISaveOptions;
    }
}