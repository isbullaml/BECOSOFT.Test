namespace BECOSOFT.Data.Models.Assemblies {
    public class SqlAssemblyFunction {
        public SqlAssembly Assembly { get; }
        public Schema Schema { get; }
        public string SqlName { get; }
        public string AssemblyFunctionName { get; }

        public string FullName => $"{Assembly.FullName}.{SqlFunctionName}";

        public string SqlFunctionName => $"{Schema.ToSql()}.{SqlName}";

        public SqlAssemblyFunction(SqlAssembly assembly, string assemblyFunctionName, Schema schema, string sqlName) {
            Assembly = assembly;
            AssemblyFunctionName = assemblyFunctionName;
            Schema = schema;
            SqlName = sqlName;
        }
    }
}