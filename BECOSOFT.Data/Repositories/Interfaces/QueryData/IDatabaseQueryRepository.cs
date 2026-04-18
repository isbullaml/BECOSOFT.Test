using BECOSOFT.Data.Models.QueryData;

namespace BECOSOFT.Data.Repositories.Interfaces.QueryData {
    internal interface IDatabaseQueryRepository : IBaseRepository {
        bool DatabaseExists(string database);
        bool HasDatabaseAccess(string database);
        bool HasConnection(int timeout = 1000);
        bool HasAssembly(string assemblyName);
        bool HasAssemblyFunction(string assemblyName, string functionName);
        DatabaseSize GetSize(string database);
    }
}