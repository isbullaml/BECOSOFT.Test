using System.Data.SqlClient;

namespace BECOSOFT.Data.Models {
    /// <summary>
    /// Settings-object for an SQL-connection
    /// </summary>
    public class SqlInfo {
        /// <summary>
        /// The DataSource
        /// Example: '127.0.0.1\TestDB'
        /// </summary>
        public string DataSource { get; set; }

        /// <summary>
        /// The Password
        /// Example: 'Azerty123'
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// The UserID
        /// Example: 'TestUser'
        /// </summary>
        public string UserID { get; set; }

        /// <summary>
        /// Amount of seconds until the connection must time out
        /// </summary>
        public int ConnectTimeout { get; set; }

        /// <summary>
        /// Value indicating whether processing should be async
        /// </summary>
        public bool AsynchronousProcessing { get; set; }

        /// <summary>
        /// Value indicating whether the connection should be encrypted
        /// </summary>
        public bool Encrypt { get; set; }

        /// <summary>
        /// Value indicating whether integrated security should be used
        /// </summary>
        public bool IntegratedSecurity { get; set; }

        /// <summary>
        /// Value indicating whether the server certificate should be trusted (for self-signed certificates)
        /// </summary>
        public bool TrustServerCertificate { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public SqlInfo() {
            ConnectTimeout = 60;
            AsynchronousProcessing = false;
            Encrypt = false;
            IntegratedSecurity = false;
            TrustServerCertificate = false;
        }

        /// <summary>
        /// Converts the SQL-info to a connection string
        /// </summary>
        /// <param name="databaseName">The name of the database to connect to</param>
        /// <returns>The connection string</returns>
        public string ToConnectionString(string databaseName) {
            var builder = new SqlConnectionStringBuilder {
                ApplicationName = "Staging",
                DataSource = DataSource,
                Password = Password,
                UserID = UserID,
                InitialCatalog = databaseName,
                Encrypt = true,
                TrustServerCertificate = true
            };

            return builder.ConnectionString;
        }
    }
}
