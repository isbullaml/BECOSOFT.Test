namespace BECOSOFT.Utilities.Models.Promotions {
    public enum PromotionConditionKind {
        All = 0,

        /// <summary>
        /// <see cref="Default"/> conditions define the articles that activate the promotion (through <see cref="PromotionWrapper"/>).
        /// If no <see cref="Action"/> conditions are specified, they also define the articles on which promotion actions (through <see cref="PromotionActionWrapper"/>) are activated.
        /// </summary>
        Default = 1,

        /// <summary>
        /// <see cref="Activation"/> conditions define the articles required to generate a voucher that can activate the rest of the promotion.
        /// </summary>
        Activation = 2,

        /// <summary>
        /// <see cref="Action"/> conditions define the articles on which the promotion actions (through <see cref="PromotionActionWrapper"/>) need to be executed.
        /// </summary>
        Action = 3,
    }
}