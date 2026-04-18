using NLog;
using System.Data;

namespace BECOSOFT.Data.Migrator {
    public class MigrationRunData {
        public IDbConnection Connection { get; }
        public ILogger Logger { get; }

        public MigrationRunData(IDbConnection connection, ILogger logger) {
            Connection = connection;
            Logger = logger;
        }
    }
}