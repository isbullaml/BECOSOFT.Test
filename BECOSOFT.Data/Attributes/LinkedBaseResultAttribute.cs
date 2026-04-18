using BECOSOFT.Data.Models.Base;
using System;

namespace BECOSOFT.Data.Attributes {
    /// <summary>
    /// Attribute indicating the property is linked to another table with a one-to-one relationship.
    /// This attribute should only be used on a <see cref="BaseResult"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class LinkedBaseResultAttribute : Attribute {
        /// <summary>
        /// The name of the foreign key in the other table.
        /// </summary>
        public string ForeignKeyColumn { get; }

        /// <summary>
        /// The name of the key in the current table.
        /// </summary>
        public string OwnKeyColumn { get; }

        /// <summary>
        /// Creates the attribute.
        /// </summary>
        /// <param name="foreignKeyColumn">The foreign key column in the joined table.</param>
        /// <param name="ownKeyColumn">The key column in the current table.</param>
        public LinkedBaseResultAttribute(string foreignKeyColumn, string ownKeyColumn) {
            ForeignKeyColumn = foreignKeyColumn;
            OwnKeyColumn = ownKeyColumn;
        }
    }
}
