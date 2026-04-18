using BECOSOFT.Data.Models;
using BECOSOFT.Utilities.Extensions;
using System;
using System.Runtime.CompilerServices;

namespace BECOSOFT.Data.Attributes {
    /// <summary>
    /// Attribute indicating the property is a column
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnAttribute : Attribute {
        private readonly string _propertyName;

        /// <summary>
        /// The type of the property
        /// </summary>
        public FieldType FieldType { get; set; }
        /// <summary>
        /// The name of the column
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Value indicating whether the property is insertable
        /// </summary>
        public bool Insertable { get; set; }
        /// <summary>
        /// Value indicating whether the property is updateable
        /// </summary>
        public bool Updateable { get; set; }

        public string Column => Name.IsNullOrWhiteSpace() ? _propertyName : Name;

        public ColumnAttribute([CallerMemberName] string propertyName = null) {
            _propertyName = propertyName;
            Insertable = true;
            Updateable = true;
        }
    }
}