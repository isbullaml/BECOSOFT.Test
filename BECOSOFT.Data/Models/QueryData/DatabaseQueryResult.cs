using BECOSOFT.Data.Attributes;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Utilities.Annotations;

namespace BECOSOFT.Data.Models.QueryData {
    [ResultTable]
    [UsedImplicitly]
    public class DatabaseQueryResult : BaseResult {
        [Column]
        public bool Exists { get; set; }
    }
}