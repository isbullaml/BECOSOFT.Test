namespace BECOSOFT.Data.Models {
    /// <summary>
    /// The type of query
    /// </summary>
    public enum QueryType {
        /// <summary>
        /// An INSERT statement
        /// </summary>
        Insert,
        /// <summary>
        /// An UPDATE statement
        /// </summary>
        Update,
        /// <summary>
        /// A DELETE statement
        /// </summary>
        Delete,
        /// <summary>
        /// A SELECT statement
        /// </summary>
        Select,
        /// <summary>
        /// An EXISTS statement
        /// </summary>
        Exists,
        /// <summary>
        /// A custom statement
        /// </summary>
        Custom,
        /// <summary>
        /// An UPDATE statement for a single property
        /// </summary>
        UpdateProperty
    }
}