using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Extensions.Collections;
using BECOSOFT.Utilities.Extensions.Numeric;
using BECOSOFT.Utilities.Models.Mapping.Filters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BECOSOFT.Utilities.Models.Promotions {
    public class PromotionConditionWrapper {
        private List<List<int>> _groupValues;
        private HashSet<int> _valueSet;
        private HashSet<string> _stringValueSet;
        private const char SplitCharacter = ';';
        public int MinimumGroupLevel { get; }
        public int MaximumGroupLevel { get; }
        public int PromotionID { get; set; }
        public PromotionConditionKind ConditionKind { get; set; }
        public string Group { get; set; }
        public string TypeValue { get; set; }
        public decimal PerAmount { get; set; }
        public decimal Amount { get; set; }
        public bool IsIndividual { get; set; }
        public string Value1 { get; set; }
        public string Value2 { get; set; }
        public string Value3 { get; set; }
        public string Value4 { get; set; }
        public string Value5 { get; set; }
        public PromotionConditionType ConditionType { get; set; }
        public PromotionQuantityComparisonOperator QuantityComparisonOperator { get; set; }
        public PromotionTypeOperator TypeOperator { get; set; }
        public PromotionGrouping Grouping { get; set; }
        public FilterConditionContainer FilterConditionContainer { get; set; }
        public FilterContainer PromotionArticleContainer { get; set; }

        public List<List<int>> GroupValues {
            get {
                if (_groupValues == null) {
                    InitialiseCollections();
                }
                return _groupValues;
            }
        }

        public HashSet<int> ValueSet {
            get {
                if (_groupValues == null) {
                    InitialiseCollections();
                }
                return _valueSet;
            }
        }
        
        /// <summary>
        /// A <see cref="HashSet{T}"/> of lower cased values
        /// </summary>
        public HashSet<string> StringValueSet {
            get {
                if (_groupValues == null) {
                    InitialiseCollections();
                }
                return _stringValueSet;
            }
        }
        
        public PromotionConditionWrapper(int minGroupLevel, int maxGroupLevel) {
            MinimumGroupLevel = minGroupLevel.NullIf(0) ?? 1;
            MaximumGroupLevel = maxGroupLevel.NullIf(0) ?? MinimumGroupLevel;
        }

        private void InitialiseCollections() {
            if (ConditionType == PromotionConditionType.FashionColor 
                || ConditionType == PromotionConditionType.FashionCollection 
                || ConditionType == PromotionConditionType.FashionFabric) {
                var firstValueList = Value1.ToSplitList<string>(SplitCharacter);
                _stringValueSet = firstValueList.Select(s => s.ToLower()).ToSafeHashSet();
                _valueSet = new HashSet<int>(0);
                _groupValues = new List<List<int>>(0);
                return;
            }
            if (ConditionType == PromotionConditionType.Type) {
                _stringValueSet = new HashSet<string>{Value1.ToLower()};
                _valueSet = new HashSet<int>(0);
                _groupValues = new List<List<int>>(0);
                return;
            }
            _stringValueSet = new HashSet<string>();
            var value1List = Value1.ToSplitList<int>(SplitCharacter);
            if (ConditionType == PromotionConditionType.Group || ConditionType == PromotionConditionType.ProductValue) {
                var groupValues = new List<List<int>>(0);
                for (var level = MinimumGroupLevel; level <= MaximumGroupLevel; level++) {
                    var groupValueItems = level == MinimumGroupLevel ? value1List : GetValue(level).ToSplitList<int>(SplitCharacter);
                    groupValues.Add(groupValueItems);
                    // fill with empty values if the value on the condition wasn't set:
                    if (level > MinimumGroupLevel && groupValues[level - 1].IsEmpty()) {
                        groupValues[level - 1] = Enumerable.Repeat(0, groupValues[level - 2].Count).ToList();
                    }
                }
                _groupValues = groupValues;
                _valueSet = new HashSet<int>(0);
            } else {
                _valueSet = value1List.ToSafeHashSet();
                _groupValues = new List<List<int>>(0);
            }
        }

        public string GetValue(int index) {
            switch (index) {
                case 1:
                    return Value1;
                case 2:
                    return Value2;
                case 3:
                    return Value3;
                case 4:
                    return Value4;
                case 5:
                    return Value5;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public bool IsTagCondition() {
            return ConditionType == PromotionConditionType.Tag
                   || (FilterConditionContainer?.JsonData?.Contains("\"TagIDs\"", StringComparison.InvariantCultureIgnoreCase) ?? false);
        }

        public PromotionConditionWrapper Copy() {
            return new PromotionConditionWrapper(MinimumGroupLevel, MaximumGroupLevel) {
                PromotionID = PromotionID,
                ConditionKind = ConditionKind,
                Group = Group,
                TypeValue = TypeValue,
                PerAmount = PerAmount,
                Amount = Amount,
                IsIndividual = IsIndividual,
                Value1 = Value1,
                Value2 = Value2,
                Value3 = Value3,
                Value4 = Value4,
                Value5 = Value5,
                ConditionType = ConditionType,
                QuantityComparisonOperator = QuantityComparisonOperator,
                TypeOperator = TypeOperator,
                Grouping = Grouping,
                FilterConditionContainer = FilterConditionContainer,
            };
        }
    }
}