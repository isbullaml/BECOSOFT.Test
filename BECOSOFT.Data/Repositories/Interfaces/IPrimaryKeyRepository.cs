using BECOSOFT.Data.Models;
using System;
using System.Collections.Generic;

namespace BECOSOFT.Data.Repositories.Interfaces {
    public interface IPrimaryKeyRepository : IBaseRepository {
        HashSet<int> GetIDs(Type type, HashSet<int> ids, string tablePart = null);
        HashSet<int> GetIDs<T>(HashSet<int> ids, string tablePart = null);
        HashSet<int> GetIDs(ParametrizedQuery query);
        PrimaryKeyContainer GetIDs(PrimaryKeyContainer container);
    }
}