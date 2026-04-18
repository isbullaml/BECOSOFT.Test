using BECOSOFT.Data.Attributes;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Utilities.Annotations;

namespace BECOSOFT.Data.Models.QueryData {
    [ResultTable]
    [UsedImplicitly]
    public class ServerQueryResult : BaseResult {
        [Column]
        public bool Exists { get; set; }
    }
}