using BECOSOFT.Data.Collections;
using BECOSOFT.Data.Models;
using BECOSOFT.Data.Models.Base;
using System;
using System.Linq.Expressions;

namespace BECOSOFT.Data.Services.Interfaces {

    public interface IBaseResultService<T> : IBaseService where T : BaseResult {
        IPagedList<T> GetAll(PagerData<T> pagerData = null);
        IPagedList<T> GetByProperty(Expression<Func<T, bool>> whereExpression, PagerData<T> pagerData = null);
        IPagedList<TResult> GetProperties<TResult>(TResult item, Expression<Func<T, bool>> whereExpression = null, PagerData<T> pagerData = null) where TResult : class;
    }
}
