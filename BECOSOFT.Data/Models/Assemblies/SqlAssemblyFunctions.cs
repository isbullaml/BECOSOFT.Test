namespace BECOSOFT.Data.Models.Assemblies {
    public static class SqlAssemblyFunctions {
        public static readonly SqlAssemblyFunction StripHtmlTags = new SqlAssemblyFunction(SqlAssemblies.SqlServerFunctions, "StripHtmlTags", Schema.Dbo, "BECOSOFT_StripHtmlTags");
        public static readonly SqlAssemblyFunction LengthOfStringStrippedFromHtmlTags = new SqlAssemblyFunction(SqlAssemblies.SqlServerFunctions, "LengthOfStringStrippedFromHtmlTags", Schema.Dbo, "BECOSOFT_LengthOfStringStrippedFromHtmlTags");
    }
}