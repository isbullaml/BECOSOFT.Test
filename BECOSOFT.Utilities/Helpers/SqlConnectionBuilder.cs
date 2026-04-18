using BECOSOFT.Utilities.Exceptions;
using BECOSOFT.Utilities.Security;
using System;
using System.Data.SqlClient;

namespace BECOSOFT.Utilities.Helpers {
    public static class SqlConnectionBuilder {
        public static string Redact(string connectionString) {
            var temp = connectionString;
            if (CanDecrypt(temp)) {
                temp = GetDecryptedConnectionString(temp);
            }
            var builder = new SqlConnectionStringBuilder(temp) {
                Password = "",
                UserID = "",
            };
            return builder.ToString();
        }

        public static bool CanDecrypt(string connectionString) {
            try {
                var builder = new SqlConnectionStringBuilder(connectionString);
                return builder.Password.Length > 40;
            } catch (Exception) {
                return false;
            }
        }

        public static string GetDecryptedConnectionString(string connectionString) {
            if (CanDecrypt(connectionString)) {
                try {
                    var builder = new SqlConnectionStringBuilder(connectionString);
                    var key = builder.Password.Substring(0, 40);
                    var password = builder.Password.Substring(40);
                    var decryptedBuilder = new SqlConnectionStringBuilder {
                        ApplicationName = builder.ApplicationName,
                        UserID = Encryptor.Decrypt(builder.UserID, key),
                        Password = Encryptor.Decrypt(password, key),
                        InitialCatalog = Encryptor.Decrypt(builder.InitialCatalog, key),
                        DataSource = Encryptor.Decrypt(builder.DataSource, key)
                    };

                    return decryptedBuilder.ConnectionString;
                } catch (Exception) {
                    throw new DbConnectionException("Invalid connectionstring");
                }
            }

            return connectionString;
        }

        public static string GetEncryptedConnectionString(string connectionString) {
            try {
                var builder = new SqlConnectionStringBuilder(connectionString);
                var key = Encryptor.GetUniqueKey();
                var encryptedBuilder = new SqlConnectionStringBuilder {
                    ApplicationName = builder.ApplicationName,
                    UserID = Encryptor.Encrypt(builder.UserID, key),
                    Password = key + Encryptor.Encrypt(builder.Password, key),
                    InitialCatalog = Encryptor.Encrypt(builder.InitialCatalog, key),
                    DataSource = Encryptor.Encrypt(builder.DataSource, key)
                };

                return encryptedBuilder.ConnectionString;
            } catch (Exception) {
                return connectionString;
            }
        }
    }
}