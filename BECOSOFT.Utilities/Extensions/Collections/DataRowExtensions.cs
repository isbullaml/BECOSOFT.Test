using System.Data;

namespace BECOSOFT.Utilities.Extensions.Collections {
    /// <summary>
    /// Extensions for the <see cref="DataRow"/>-class
    /// </summary>
    public static class DataRowExtensions {
        /// <summary>
        /// Adds a column with a value to the row
        /// </summary>
        /// <param name="row">The row to add the pair to</param>
        /// <param name="column">The column-name</param>
        /// <param name="value">The value to store in the column</param>
        public static void Add(this DataRow row, string column, object value) {
            row[column] = value;
        }
    }
}
