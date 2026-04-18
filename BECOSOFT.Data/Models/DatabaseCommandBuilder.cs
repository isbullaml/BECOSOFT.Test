using BECOSOFT.Data.Query;
using System.Collections.Generic;

namespace BECOSOFT.Data.Models {
    public class DatabaseCommandBuilder<T> {
        internal BaseQueryBuilder QueryBuilder { get; set; }
        internal IEnumerable<T> Entities { get; set; }
        internal int Timeout { get; set; }
    }
}