using BECOSOFT.Utilities.Attributes;

namespace BECOSOFT.Data.Models {
    /// <summary>
    /// Boolean operators
    /// </summary>
    public enum Operator {
        /// <summary>
        /// AND
        /// </summary>
        [LocalizedEnum("Operator_And", NameResourceType = typeof(Resources))]
        And,
        /// <summary>
        /// OR
        /// </summary>
        [LocalizedEnum("Operator_Or", NameResourceType = typeof(Resources))]
        Or
    }
}
