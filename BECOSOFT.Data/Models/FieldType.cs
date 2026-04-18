namespace BECOSOFT.Data.Models {
    /// <summary>
    /// The SQL-type of a field
    /// </summary>
    public enum FieldType {
        /// <summary>
        /// A numeric value
        /// </summary>
        Numeric = 1,
        /// <summary>
        /// A character
        /// </summary>
        Char = 2,
        /// <summary>
        /// A date
        /// </summary>
        Date = 3,
        /// <summary>
        /// A bit (boolean)
        /// </summary>
        Bit = 4,
        /// <summary>
        /// A byte
        /// </summary>
        Byte = 5,
        /// <summary>
        /// NULL
        /// </summary>
        Null = 6,
        /// <summary>
        /// a free field
        /// </summary>
        Free = 7,
        /// <summary>
        /// a time
        /// </summary>
        Time = 8,
        /// <summary>
        /// a guid
        /// </summary>
        Guid = 9
    }
}
