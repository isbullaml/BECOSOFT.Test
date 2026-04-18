using System.Data;

namespace BECOSOFT.Data.Context {
    /// <summary>
    /// Connection factory
    /// </summary>
    internal interface IDbConnectionFactory {
        string Connection { get; }
        /// <summary>
        /// Create a connection
        /// </summary>
        /// <returns>The connection</returns>
        IDbConnection CreateConnection();
    }
}
