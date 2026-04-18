using BECOSOFT.Utilities.Promotions;
using System.Collections.Generic;
using System.Diagnostics;

namespace BECOSOFT.Utilities.Models.Promotions {
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    internal class PromotionEligibleArticlesResult {
        public List<List<EligiblePromotionArticle>> Included { get; set; }
        public List<EligiblePromotionArticle> ToMark { get; set; }

        /// <summary>
        /// Indicates that all checked conditions and criteria were "From".
        /// </summary>
        public bool AllFrom { get; set; }

        private string DebuggerDisplay => $"Included: {Included?.Count}, ToMark: {ToMark?.Count}, AllFrom: {AllFrom}";
    }
}