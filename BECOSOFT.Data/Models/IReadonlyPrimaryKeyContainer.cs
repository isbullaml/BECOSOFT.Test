using System;
using System.Collections.Generic;

namespace BECOSOFT.Data.Models {
    public interface IReadonlyPrimaryKeyContainer : IEnumerable<KeyValuePair<PrimaryKeyType, HashSet<int>>> {
        int Count { get; }
        bool IsEmpty();
        HashSet<int> TryGetIDs<T>(string tablePart = null);
        HashSet<int> TryGetIDs(Type type, string tablePart = null);
    }
}