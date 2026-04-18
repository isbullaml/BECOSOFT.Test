using System;
using System.Collections.Generic;

namespace BECOSOFT.Utilities.Models.Promotions {
    public class PromotionRecognitionParameters {
        /// <summary>
        /// Filter promotions that are active on <see cref="PromotionDate"/>.
        /// The <see cref="DayOfWeek"/> property on <see cref="PromotionDate"/> is used to determine whether is is contained in the <see cref="Promotion"/>.<see cref="Promotion.ActiveDays"/> list.
        /// </summary>
        public DateTime PromotionDate { get; set; } = DateTime.Now;

        /// <summary>
        /// List of ArticleIDs to recognize promotions for
        /// </summary>
        public List<int> ArticleIDs { get; set; }

        /// <summary>
        /// Contact ID for which to filter the active promotions
        /// </summary>
        public int ContactID { get; set; }

        /// <summary>
        /// Warehouse ID where the promotions are active in
        /// </summary>
        public int WarehouseID { get; set; }

        /// <summary>
        /// Depending on the value of <see cref="IsActiveOnWebshop"/>, promotions active on webshop are also checked.
        /// </summary>
        public bool IsActiveOnWebshop { get; set; }

        /// <summary>
        /// Include promotions with multiple conditions in recognition (Default: <see langword="false"/>)
        /// </summary>
        public bool IncludeMultiConditionPromotions { get; set; } = false;

        /// <summary>
        /// Exclude promotions activated with a barcode from recognition (Default: <see langword="true"/>)
        /// </summary>
        public bool ExcludeBarcodePromotions { get; set; } = true;

        /// <summary>
        /// Exclude customer specific promotions from recognition (Default: <see langword="true"/>)
        /// </summary>
        public bool ExcludeCustomerSpecific { get; set; } = true;

        /// <summary>
        /// Exclude promotions with action vouchers from recognition (Default: <see langword="true"/>)
        /// </summary>
        public bool ExcludeActionVoucher { get; set; } = true;

        /// <summary>
        /// Enables or disables the loyalty filter
        /// </summary>
        public bool FilterLoyalty { get; set; } = true;

        /// <summary>
        /// When <see cref="FilterLoyalty"/> is enabled, the value of <see cref="FilterLoyaltyValue"/> will be used in the check.
        /// </summary>
        public bool FilterLoyaltyValue { get; set; } = false;

        /// <summary>
        /// Exclude promotions with conditions specified in <see cref="ConditionsToExclude"/>
        /// </summary>
        public List<PromotionConditionType> ConditionsToExclude { get; set; }

        /// <summary>
        /// Exclude promotions with actions specified in <see cref="ActionsToExclude"/>
        /// </summary>
        public List<PromotionActionType> ActionsToExclude { get; set; }

        public PromotionArticleContainer PromotionArticleContainer { get; set; }
    }
}