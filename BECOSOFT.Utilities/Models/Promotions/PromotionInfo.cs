using BECOSOFT.Utilities.Extensions.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BECOSOFT.Utilities.Models.Promotions {
    public class PromotionInfo {
        public int ID { get; set; }
        public string Name { get; set; }
        public Dictionary<int, LocalizedPromotionInfo> TranslationData { get; set; }

        public LocalizedPromotionInfo GetLocalizedPromotionInfo(int languageID) {
            var item = TranslationData?.TryGetValueWithDefault(languageID);
            if (item == null) { return new LocalizedPromotionInfo { Name = Name }; }
            return item;
        }

        public override string ToString() => $"{Name} ({ID})";
    }

    public class LocalizedPromotionInfo {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Notification { get; set; }
    }
}