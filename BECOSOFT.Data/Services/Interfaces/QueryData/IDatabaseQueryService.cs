using BECOSOFT.Data.Models.Assemblies;
using BECOSOFT.Data.Models.QueryData;

namespace BECOSOFT.Data.Services.Interfaces.QueryData {
    public interface IDatabaseQueryService : IBaseService {
        bool DatabaseExists(string database);
        bool HasDatabaseAccess(string database);
        bool HasConnection(int timeout = 1000);
        bool HasAssembly(string assemblyName);
        bool HasAssembly(SqlAssembly assembly);
        bool HasAssemblyFunction(string assemblyName, string functionName);
        bool HasAssemblyFunction(SqlAssemblyFunction assemblyFunction);
        DatabaseSize GetSize(string database);
    }
}