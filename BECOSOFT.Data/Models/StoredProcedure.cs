using System;

namespace BECOSOFT.Data.Models {
    public class StoredProcedure {
        private readonly Func<ISqlScript> _resourceFunc;
        internal Schema Schema { get; }
        internal string Name { get; }

        internal string Procedure => _resourceFunc().GetQuery();

        private StoredProcedure(Schema schema, string name, Func<ISqlScript> resourceFunc) {
            Schema = schema;
            Name = name;
            _resourceFunc = resourceFunc;
        }

        /// <summary>
        /// Create a <see cref="StoredProcedure"/>-object.
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="name">Name of the stored procedure</param>
        /// <param name="resourceFunc">Resource retrieval function containing the full stored procedure</param>
        /// <returns></returns>
        public static StoredProcedure Create(Schema schema, string name, Func<ISqlScript> resourceFunc) {
            return new StoredProcedure(schema, name, resourceFunc);
        }

        /// <summary>
        /// Create a <see cref="StoredProcedure"/>-object with <see cref="Schema.Dbo"/>.
        /// </summary>
        /// <param name="name">Name of the stored procedure</param>
        /// <param name="resourceFunc">Resource retrieval function</param>
        /// <returns></returns>
        public static StoredProcedure Create(string name, Func<ISqlScript> resourceFunc) {
            return new StoredProcedure(Schema.Dbo, name, resourceFunc);
        }
    }
}