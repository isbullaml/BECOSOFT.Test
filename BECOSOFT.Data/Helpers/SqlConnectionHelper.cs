using BECOSOFT.Utilities.Extensions;
using System;
using System.Data.SqlClient;

namespace BECOSOFT.Data.Helpers {
    public static class SqlConnectionHelper {
        public static bool IsValid(SqlConnection connection) {
            if (connection == null) { return false; }
            return IsValid(connection.ConnectionString);
        }

        public static bool IsValid(string connectionString) {
            if (connectionString.IsNullOrWhiteSpace()) {
                return false;
            }
            try {
                var builder = new SqlConnectionStringBuilder(connectionString);
                if (builder.DataSource.IsNullOrWhiteSpace()) {
                    return false;
                }
                if (builder.InitialCatalog.IsNullOrWhiteSpace()) {
                    return false;
                }
                if (!builder.IntegratedSecurity && (builder.UserID.IsNullOrWhiteSpace() || builder.Password.IsNullOrWhiteSpace())) {
                    return false;
                }
                var _ = builder.ConnectionString;
                return true;
            } catch (Exception) {
                return false;
            }
        }
    }
}