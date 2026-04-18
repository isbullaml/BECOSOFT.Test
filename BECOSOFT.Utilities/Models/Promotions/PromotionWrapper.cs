using BECOSOFT.Utilities.Extensions.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BECOSOFT.Utilities.Models.Promotions {
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class PromotionWrapper {
        public int PromotionID { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime From { get; set; }

        public DateTime To { get; set; }

        public bool IsActionVoucher { get; set; }

        public int DaysValid { get; set; }

        public DateTime? ActionVoucherFrom { get; set; }

        public DateTime? ActionVoucherTo { get; set; }

        public int ContactGroupID { get; set; }

        public int ContactSubGroupID { get; set; }

        public string Barcode { get; set; }

        public bool IsActiveOnWebshop { get; set; }

        public int DaysValidAfterCreation { get; set; }

        public bool NotBelowPurchasePrice { get; set; }

        public bool IsPromoUniquePerCustomer { get; set; }

        public int UniquePerAmountOfDays { get; set; }

        public bool Cumulative { get; set; }

        public int Points { get; set; }

        public bool IsLoyalty { get; set; }

        /// <summary>
        /// Exclude promotion from best promotion calculation
        /// </summary>
        public bool ExcludeFromBestPromotion { get; set; }

        /// <summary>
        /// Disable marking products as used in a promotion, so they can be processed in other promotions
        /// </summary>
        public bool DisablePromotionTagging { get; set; }

        /// <summary>
        /// Indicates that the promotion should always be executed with the full article data set.
        /// </summary>
        public bool AlwaysExecute { get; set; }

        public int Position { get; set; }
        public List<PromotionTranslationWrapper> Translations { get; set; } = new List<PromotionTranslationWrapper>(0);
        public List<PromotionActionWrapper> Actions { get; set; }
        public List<PromotionConditionWrapper> PromotionConditions { get; set; }
        public List<PromotionLocation> Locations { get; set; }
        public List<PromotionPeriodWrapper> Periods { get; set; }
        
        /// <summary>
        /// Contains a list of document types that are excluded from calculation by checking if <see cref="PromotionParameters"/>.<see cref="PromotionParameters.DocumentTypeID"/> is present in the list
        /// </summary>
        public List<PromotionExcludedDocument> ExcludedDocuments { get; set; }

        public List<DayOfWeek> ActiveDays { get; set; }
        public Range<DateTime> DateRange => new Range<DateTime>(From, To);

        /// <summary>
        /// Nullable range based on the current <see cref="ActionVoucherFrom"/> and <see cref="ActionVoucherTo"/> values.
        /// </summary>
        public NullableRange<DateTime> ActionVoucherDateRange => new NullableRange<DateTime>(ActionVoucherFrom, ActionVoucherTo);

        /// <summary>
        /// Conditions that activate a promotion.
        /// </summary>
        public List<PromotionConditionWrapper> ActivationConditions => PromotionConditions.Where(p => p.ConditionKind == PromotionConditionKind.Activation).ToSafeList();

        /// <summary>
        /// Conditions defining on which articles the promotion can be activated
        /// </summary>
        public List<PromotionConditionWrapper> Conditions => PromotionConditions.Where(p => p.ConditionKind == PromotionConditionKind.Default).ToSafeList();

        /// <summary>
        /// Conditions defining on which articles the promotion actions can be activated
        /// </summary>
        public List<PromotionConditionWrapper> ActionConditions => PromotionConditions.Where(p => p.ConditionKind == PromotionConditionKind.Action).ToSafeList();

        /// <summary>
        /// Retrieve a list of <see cref="PromotionConditionWrapper"/> objects for the given <see cref="PromotionConditionKind"/>.
        /// </summary>
        /// <param name="kind"></param>
        /// <returns></returns>
        public List<PromotionConditionWrapper> GetConditions(PromotionConditionKind kind) {
            switch (kind) {
                case PromotionConditionKind.All:
                    return PromotionConditions;
                case PromotionConditionKind.Activation:
                    return ActivationConditions;
                case PromotionConditionKind.Action:
                    return ActionConditions;
                default:
                    return Conditions;
            }
        }

        /// <summary>
        /// Indicates if the <paramref name="type"/> is present in the <see cref="PromotionActionWrapper"/> list.
        /// </summary>
        /// <param name="type"><see cref="PromotionActionType"/> to check.</param>
        /// <returns></returns>
        public bool HasAction(PromotionActionType type) {
            return Actions.Any(a => a.Type == type);
        }

        /// <summary>
        /// Indicates the promotion contains an action of one of the <paramref name="types"/> in the <see cref="PromotionActionWrapper"/> list.
        /// </summary>
        /// <param name="types">Array of <see cref="PromotionActionType"/> to check.</param>
        /// <returns></returns>
        public bool HasAction(params PromotionActionType[] types) {
            if (types.IsEmpty()) { return false; }
            return Actions.Any(a => types.Contains(a.Type));
        }

        private string DebuggerDisplay => $"{PromotionID}, {Name} ({From} - {To}), {PromotionConditions.Count} conditions ({Conditions.Count} default, {ActivationConditions.Count} activation, {ActionConditions.Count} actions), {Actions.Count} actions";
        
        public bool HasCondition(PromotionConditionKind kind, PromotionConditionType type) {
            return GetConditions(kind).Any(c => c.ConditionType == type);
        }

        public PromotionInfo ToPromotionInfo() {
            var translationData = Translations?.GroupBy(t => t.LanguageID)
                                              .ToDictionary(g => g.Key, g => {
                                                  var t = g.First();
                                                  return new LocalizedPromotionInfo {
                                                      Name = t.Name,
                                                      Description = t.Description,
                                                      Notification = t.Notification,
                                                  };
                                              }) ?? new Dictionary<int, LocalizedPromotionInfo>(0);
            return new PromotionInfo {
                ID = PromotionID,
                Name = Name,
                TranslationData = translationData,
            };
        }

        public PromotionWrapper Copy() {
            var promotion = new PromotionWrapper {
                PromotionID = PromotionID,
                Name = Name,
                Description = Description,
                From = From,
                To = To,
                IsActionVoucher = IsActionVoucher,
                DaysValid = DaysValid,
                ActionVoucherFrom = ActionVoucherFrom,
                ActionVoucherTo = ActionVoucherTo,
                ContactGroupID = ContactGroupID,
                ContactSubGroupID = ContactSubGroupID,
                Barcode = Barcode,
                IsActiveOnWebshop = IsActiveOnWebshop,
                DaysValidAfterCreation = DaysValidAfterCreation,
                NotBelowPurchasePrice = NotBelowPurchasePrice,
                IsPromoUniquePerCustomer = IsPromoUniquePerCustomer,
                UniquePerAmountOfDays = UniquePerAmountOfDays,
                Cumulative = Cumulative,
                Points = Points,
                IsLoyalty = IsLoyalty,
                Position = Position,
                Actions = Actions.Select(a => a.Copy()).ToSafeList(),
                PromotionConditions = PromotionConditions.Select(c => c.Copy()).ToSafeList(),
                Locations = Locations.Select(w => w.Copy()).ToSafeList(),
                Periods = Periods.Select(p => p.Copy()).ToSafeList(),
                ExcludedDocuments = ExcludedDocuments.Select(d => d.Copy()).ToSafeList(),
                ExcludeFromBestPromotion = ExcludeFromBestPromotion,
                DisablePromotionTagging = DisablePromotionTagging,
                AlwaysExecute = AlwaysExecute,
                Translations = Translations.Select(p => p.Copy()).ToSafeList(),
            };
            return promotion;
        }
    }
}