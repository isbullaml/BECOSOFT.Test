using BECOSOFT.Data.Collections;
using BECOSOFT.Data.Models;
using BECOSOFT.Data.Models.Base;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace BECOSOFT.Data.Services.Interfaces {
    public interface IReadonlyTableService<T, in TDefining> : IBaseService
        where T : TableConsumingEntity<TDefining>
        where TDefining : TableDefiningEntity {
        T Get(TDefining definition, int id);
        IPagedList<T> Get(TDefining definition, IEnumerable<int> ids);
        IPagedList<T> GetAll(TDefining definition, PagerData<T> pagerData = null);
        IPagedList<TResult> GetProperties<TResult>(TDefining definition, TResult item, Expression<Func<T, bool>> whereExpression = null, PagerData<T> pagerData = null) where TResult : class;
        IPagedList<T> GetByProperty(TDefining definition, Expression<Func<T, bool>> whereExpression, PagerData<T> pagerData = null);
        TProp GetProperty<TProp>(TDefining definition, int id, Expression<Func<T, TProp>> property) where TProp : IConvertible;
        bool Exists(TDefining definition, T entity);
        bool Exists(TDefining definition, int id);
        bool Exists(TDefining definition, Expression<Func<T, bool>> whereExpression);
    }
}
