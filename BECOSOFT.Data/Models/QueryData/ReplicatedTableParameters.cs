using BECOSOFT.Utilities.Collections;
using System;
using System.Collections.Generic;

namespace BECOSOFT.Data.Models.QueryData {
    public class ReplicatedTableParameters {
        /// <summary>
        /// Replicated tables that have been marked optional, but are required due to certain settings.
        /// </summary>
        public HashSet<Type> RequiredOptionalTables { get; set; } = new HashSet<Type>();

        /// <summary>
        /// Contains the table parts to check for types that are table consuming (and require a table part).
        /// </summary>
        public KeyValueList<Type, string> TablePartEntries { get; set; } = new KeyValueList<Type, string>();

        /// <summary>
        /// Defines the types that need to be checked, overrules the default types that will be checked
        /// </summary>
        public List<Type> TypesToCheck { get; set; }
    }
}