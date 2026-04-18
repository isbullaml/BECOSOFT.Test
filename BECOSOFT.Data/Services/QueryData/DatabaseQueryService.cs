using BECOSOFT.Data.Models.Assemblies;
using BECOSOFT.Data.Models.QueryData;
using BECOSOFT.Data.Repositories.Interfaces.QueryData;
using BECOSOFT.Data.Services.Interfaces.QueryData;

namespace BECOSOFT.Data.Services.QueryData {
    public sealed class DatabaseQueryService : IDatabaseQueryService {
        private readonly IDatabaseQueryRepository _repository;

        internal DatabaseQueryService(IDatabaseQueryRepository repository) {
            _repository = repository;
        }

        public bool DatabaseExists(string database) {
            return _repository.DatabaseExists(database);
        }

        public bool HasDatabaseAccess(string database) {
            return _repository.HasDatabaseAccess(database);
        }

        public bool HasConnection(int timeout = 1000) {
            return _repository.HasConnection(timeout);
        }

        public bool HasAssembly(string assemblyName) {
            return _repository.HasAssembly(assemblyName);
        }

        public bool HasAssembly(SqlAssembly assembly) {
            return _repository.HasAssembly(assembly.FullName);
        }

        public bool HasAssemblyFunction(string assemblyName, string functionName) {
            return _repository.HasAssemblyFunction(assemblyName, functionName);
        }

        public bool HasAssemblyFunction(SqlAssemblyFunction assemblyFunction) {
            return _repository.HasAssemblyFunction(assemblyFunction.Assembly.FullName, assemblyFunction.AssemblyFunctionName);
        }

        public DatabaseSize GetSize(string database) {
            return _repository.GetSize(database);
        }
    }
}