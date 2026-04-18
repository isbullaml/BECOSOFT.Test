using BECOSOFT.Utilities.Extensions;

namespace BECOSOFT.Data.Models.Assemblies {
    public class SqlAssembly {
        public string Prefix { get; }
        public string Name { get; }

        public string FullName => $"{(Prefix.HasValue() ? $"{Prefix}." : "")}{Name}";

        public SqlAssembly(string prefix, string name) {
            Prefix = prefix;
            Name = name;
        }

        public SqlAssembly(string name) : this(null, name) {
        }
    }
}