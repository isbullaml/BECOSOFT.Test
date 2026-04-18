using BECOSOFT.Data.Attributes;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Utilities.Annotations;

namespace BECOSOFT.Data.Models.QueryData {
    [ResultTable]
    [UsedImplicitly]
    public class TriggerQueryResult : BaseResult {
        [Column]
        public string Name { get; set; }
        [Column]
        public bool IsEnabled { get; set; }

    }
}