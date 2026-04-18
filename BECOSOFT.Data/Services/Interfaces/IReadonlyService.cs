using BECOSOFT.Data.Collections;
using BECOSOFT.Data.Models;
using BECOSOFT.Data.Models.Base;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace BECOSOFT.Data.Services.Interfaces {
    public interface IReadonlyService<T> : IBaseService where T : BaseEntity {
        T Get(int id);
        IPagedList<T> Get(IEnumerable<int> ids);
        IPagedList<T> GetAll(PagerData<T> pagerData = null);
        IPagedList<T> GetByProperty(Expression<Func<T, bool>> whereExpression, PagerData<T> pagerData = null);
        IPagedList<TResult> GetProperties<TResult>(TResult item, Expression<Func<T, bool>> whereExpression = null, PagerData<T> pagerData = null) where TResult : class;
        TProp GetProperty<TProp>(int id, Expression<Func<T, TProp>> property) where TProp : IConvertible;
        bool Exists(T entity);
        bool Exists(int id);
        bool Exists(Expression<Func<T, bool>> whereExpression);
    }
}
