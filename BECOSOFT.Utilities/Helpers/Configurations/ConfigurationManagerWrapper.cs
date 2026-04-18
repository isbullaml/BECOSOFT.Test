using BECOSOFT.Utilities.Converters;
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Extensions.Collections;
using BECOSOFT.Utilities.IO;
using BECOSOFT.Utilities.Models;
using BECOSOFT.Utilities.Models.Configurations;
using NLog;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using Configuration = BECOSOFT.Utilities.Models.Configurations.Configuration;

namespace BECOSOFT.Utilities.Helpers.Configurations {
    /// <summary>
    /// Wrapper of the <see cref="ConfigurationManager"/>.
    /// </summary>
    public static class ConfigurationManagerWrapper {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Retrieves a <see cref="ConfigurationManager.ConnectionStrings"/> connection string by <see cref="name"/>.
        /// The connection string is logged with level '<see cref="LogLevel.Trace"/>' without password and userID.
        /// </summary>
        /// <param name="name">name of the connection string to retrieve</param>
        /// <returns></returns>
        public static string GetConnectionString(string name) {
            var connection = ConfigurationManager.ConnectionStrings[name];
            var connectionString = connection.ConnectionString;
            var redacted = SqlConnectionBuilder.Redact(connectionString);
            Logger.Debug("Retrieved ConnectionString '{0}' with value '{1}'", name, redacted);
            return connectionString;
        }

        /// <summary>
        /// Retrieves an <see cref="ConfigurationManager.AppSettings"/> value by <see cref="name"/>. The string value is converted to the given type.
        /// The value retrieval is logged with level '<see cref="LogLevel.Trace"/>' and is <see cref="redacted"/> by default.
        /// </summary>
        /// <typeparam name="T">Type of the return value</typeparam>
        /// <param name="name">Name of the AppSetting to retrieve</param>
        /// <param name="redacted">Redact the value when logging. Default = <see cref="bool.True"/></param>
        /// <returns></returns>
        public static T GetAppSetting<T>(string name, bool redacted = true) {
            var value = ConfigurationManager.AppSettings[name];
            var converted = value.To<T>();
            if (redacted) {
                Logger.Debug("Retrieved AppSetting '{0}' with value <redacted> (converted as type '{1}')", name, typeof(T).Name);
            } else {
                Logger.Debug("Retrieved AppSetting '{0}' with value '{1}' (converted to '{2}' as type '{3}')", name, value, converted, typeof(T).Name);
            }
            return converted;
        }


        /// <summary>
        /// Retrieves an <see cref="ConfigurationManager.AppSettings"/> value by name.
        /// The value retrieval is logged with level '<see cref="LogLevel.Trace"/>' and is <see cref="redacted"/> by default.
        /// </summary>
        /// <param name="name">Name of the AppSetting to retrieve</param>
        /// <param name="redacted">Redact the value when logging. Default = <see cref="bool.True"/></param>
        /// <returns></returns>
        public static string GetAppSetting(string name, bool redacted = true) {
            var value = ConfigurationManager.AppSettings[name];
            if (redacted) {
                Logger.Debug("Retrieved AppSetting '{0}' with value <redacted>", name);
            } else {
                Logger.Debug("Retrieved AppSetting '{0}' with value '{1}'", name, value);
            }
            return value;
        }

        /// <summary>
        /// Retrieves an <see cref="ConfigurationManager.AppSettings"/> value by <see cref="name"/>. The string value is converted to the given type.
        /// If the key is not present, <see cref="defaultValue"/> is used.
        /// The value retrieval is logged with level '<see cref="LogLevel.Trace"/>' and is <see cref="redacted"/> by default.
        /// </summary>
        /// <typeparam name="T">Type of the return value</typeparam>
        /// <param name="name">Name of the AppSetting to retrieve</param>
        /// <param name="defaultValue">The default value, in case the key doesn't exist</param>
        /// <param name="redacted">Redact the value when logging. Default = <see cref="bool.True"/></param>
        /// <returns></returns>
        public static T GetAppSettingOrDefault<T>(string name, T defaultValue = default, bool redacted = true) {
            T converted;
            var value = "";

            if (ContainsSetting(name)) {
                value = ConfigurationManager.AppSettings[name];
                converted = value.To<T>();
            } else {
                converted = defaultValue;
            }

            if (redacted) {
                Logger.Debug("Retrieved AppSetting '{0}' with value <redacted> (converted as type '{1}')", name, typeof(T).Name);
            } else {
                Logger.Debug("Retrieved AppSetting '{0}' with value '{1}' (converted to '{2}' as type '{3}')", name, value, converted, typeof(T).Name);
            }
            return converted;
        }

        /// <summary>
        /// Set a new <see cref="value"/> for the provided <see cref="setting"/> in the <see cref="ConfigurationManager.AppSettings"/> with the provided <see cref="format"/> and <see cref="formatProvider"/>.
        /// Refreshes the <see cref="ConfigurationManager.AppSettings"/> section after saving.
        /// </summary>
        /// <typeparam name="T">Type of the value</typeparam>
        /// <param name="setting">Name of the key value combination in the AppSettings section</param>
        /// <param name="value">Value to set</param>
        /// <param name="format">Format specifier for the value</param>
        /// <param name="formatProvider">Format provider to use</param>
        public static void SetAppSetting<T>(string setting, T value, string format, IFormatProvider formatProvider) where T : IFormattable {
            SetAppSetting(setting, value.ToString(format, formatProvider));
        }

        /// <summary>
        /// Set a new <see cref="value"/> for the provided <see cref="setting"/> in the <see cref="ConfigurationManager.AppSettings"/>.
        /// Refreshes the <see cref="ConfigurationManager.AppSettings"/> section after saving.
        /// </summary>
        /// <typeparam name="T">Type of the value</typeparam>
        /// <param name="setting">Name of the key value combination in the AppSettings section</param>
        /// <param name="value">Value to set</param>
        public static void SetAppSetting<T>(string setting, T value) {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings[setting].Value = value.ToString();
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        /// <summary>
        /// Indicates whether the provided <see cref="setting"/> exists in the <see cref="ConfigurationManager.AppSettings"/>.
        /// </summary>
        /// <param name="setting">Name of the key value combination in the AppSettings section</param>
        /// <returns></returns>
        public static bool ContainsSetting(string setting) {
            var value = ConfigurationManager.AppSettings[setting];
            return value.HasValue();
        }

        /// <summary>
        /// Loads a configuration file (containing appSettings or/and connectionStrings sections) and updates the current configuration loaded in the <see cref="ConfigurationManager"/>.
        /// </summary>
        /// <param name="configuration">Configuration to load</param>
        public static void LoadConfiguration(ConfigurationFindInfo configuration) {
            LoadConfiguration(configuration.ConfigurationFile.FullName);
        }

        /// <summary>
        /// Loads a configuration file (containing appSettings or/and connectionStrings sections) and updates the current configuration loaded in the <see cref="ConfigurationManager"/>.
        /// </summary>
        /// <param name="filePath">Configuration file to load</param>
        public static void LoadConfiguration(string filePath) {
            EnsureEncryptedConnection(filePath);
            var config = LoadConfigurationObject(filePath);
            if (config.AppSettingSection != null) {
                foreach (var setting in config.AppSettingSection.Settings) {
                    ConfigurationManager.AppSettings[setting.Key] = setting.Value;
                }
            }
            if (config.ConnectionStringSection == null) { return; }

            var configElemType = typeof(ConfigurationElement);
            var readonlyField = configElemType.GetField("_bReadOnly", BindingFlags.NonPublic | BindingFlags.Instance);
            if (readonlyField == null) { return; }
            foreach (var settings in config.ConnectionStringSection.ConnectionStrings) {
                var existing = ConfigurationManager.ConnectionStrings[settings.Name];
                if (existing == null) { continue; }
                var readonlyVal = readonlyField.GetValue(existing);
                readonlyField.SetValue(existing, false);
                existing.ConnectionString = settings.Connection;
                existing.ProviderName = settings.ProviderName;
                readonlyField.SetValue(existing, readonlyVal);
            }
        }

        private static Configuration LoadConfigurationObject(ConfigurationFindInfo configuration) {
            return LoadConfigurationObject(configuration.ConfigurationFile.FullName);
        }

        private static Configuration LoadConfigurationObject(string filePath) {
            var serializer = new XmlSerializer<Configuration>();
            Configuration config;
            using (var sr = new StreamReader(filePath)) {
                config = serializer.DeserializeToModel(sr);
            }
            return config;
        }

        private static void SaveConfigurationObject(ConfigurationFindInfo configuration, Configuration config) {
            SaveConfigurationObject(configuration.ConfigurationFile.FullName, config);
        }

        private static void SaveConfigurationObject(string filePath, Configuration config) {
            EnsureEnoughAvailableFreeSpace(filePath);
            var serializer = new XmlSerializer<Configuration>();
            using (var sw = new StreamWriter(filePath)) {
                serializer.Serialize(sw, config);
                sw.Flush();
            }
        }

        /// <summary>
        /// Loads the configuration found in <see cref="configuration"/> and checks for unencrypted connection strings and encrypts them.
        /// </summary>
        /// <param name="configuration"></param>
        public static void EnsureEncryptedConnection(ConfigurationFindInfo configuration) {
            EnsureEncryptedConnection(configuration.ConfigurationFile.FullName);
        }

        /// <summary>
        /// Loads the configuration found in <see cref="filePath"/> and checks for unencrypted connection strings and encrypts them.
        /// </summary>
        /// <param name="filePath"></param>
        public static void EnsureEncryptedConnection(string filePath) {
            var config = LoadConfigurationObject(filePath);
            if (config.ConnectionStringSection == null || config.ConnectionStringSection.ConnectionStrings.IsEmpty()) {
                Logger.Warn("No connection strings to check.");
                return;
            }
            foreach (var connectionString in config.ConnectionStringSection.ConnectionStrings) {
                if (connectionString.Name == "LocalSqlServer") { continue; }
                var connection = connectionString.Connection;
                if (SqlConnectionBuilder.CanDecrypt(connection)) { continue; }
                var encryptedConnectionString = SqlConnectionBuilder.GetEncryptedConnectionString(connection);
                connectionString.Connection = encryptedConnectionString;
            }
            SaveConfigurationObject(filePath, config);
        }

        /// <summary>
        /// Update or add an <see cref="ConnectionString"/> in the provided <param name="configuration"></param> file.
        /// </summary>
        /// <param name="configuration">Configuration file where update the <see cref="ConnectionString"/>.</param>
        /// <param name="key"><see cref="ConnectionString.Name"/> of the <see cref="ConnectionString"/> to update.</param>
        /// <param name="value">New <see cref="ConnectionString.Connection"/> of the <see cref="ConnectionString"/></param>
        /// <returns></returns>
        public static bool SetConnectionString(ConfigurationFindInfo configuration, string key, string value) {
            if (!configuration.FoundConfiguration) {
                Logger.Fatal("Invalid configuration passed.");
                return false;
            }
            var config = LoadConfigurationObject(configuration);
            var setting = config.ConnectionStringSection.ConnectionStrings.FirstOrDefault(s => s.Name.Equals(key));
            if (setting == null) {
                setting = new ConnectionString { Name = key, };
                config.ConnectionStringSection.ConnectionStrings.Add(setting);
            }
            setting.Connection = value;
            setting.ProviderName = setting.ProviderName.NullIf("") ?? "System.Data.SqlClient";
            SaveConfigurationObject(configuration, config);
            return true;
        }

        /// <summary>
        /// Update or add an <see cref="ApplicationSetting"/> in the provided <param name="configuration"></param> file.
        /// </summary>
        /// <param name="configuration">Configuration file where update the <see cref="ApplicationSetting"/>.</param>
        /// <param name="key"><see cref="ApplicationSetting.Key"/> of the <see cref="ApplicationSetting"/> to update.</param>
        /// <param name="value">New <see cref="ApplicationSetting.Value"/> of the <see cref="ApplicationSetting"/></param>
        /// <returns></returns>
        public static bool SetAppSetting(ConfigurationFindInfo configuration, string key, string value) {
            if (!configuration.FoundConfiguration) {
                Logger.Fatal("Invalid configuration passed.");
                return false;
            }
            var config = LoadConfigurationObject(configuration);
            var setting = config.AppSettingSection.Settings.FirstOrDefault(s => s.Key.Equals(key));
            if (setting == null) {
                setting = new ApplicationSetting { Key = key, };
                config.AppSettingSection.Settings.Add(setting);
            }
            setting.Value = value;
            SaveConfigurationObject(configuration, config);
            return true;
        }


        /// <summary>
        /// Update or add an <see cref="ApplicationSetting"/> in the provided <param name="configuration"></param> file.
        /// </summary>
        /// <param name="configuration">Configuration file where update the <see cref="ApplicationSetting"/>.</param>
        /// <param name="key"><see cref="ApplicationSetting.Key"/> of the <see cref="ApplicationSetting"/> to update.</param>
        /// <param name="value">New <see cref="ApplicationSetting.Value"/> of the <see cref="ApplicationSetting"/></param>
        /// <returns></returns>
        public static bool SetAppSetting<T>(ConfigurationFindInfo configuration, string key, T value) {
            return SetAppSetting(configuration, key, value.ToString());
        }

        /// <summary>
        /// Update or add an <see cref="ApplicationSetting"/> in the provided <param name="configuration"></param> file.
        /// </summary>
        /// <param name="configuration">Configuration file where update the <see cref="ApplicationSetting"/>.</param>
        /// <param name="key"><see cref="ApplicationSetting.Key"/> of the <see cref="ApplicationSetting"/> to update.</param>
        /// <param name="value">New <see cref="ApplicationSetting.Value"/> of the <see cref="ApplicationSetting"/></param>
        /// <param name="format">The format to use.</param>
        /// <param name="formatProvider">The provider to use to format the value. If <see langword="null"/>, the provider of the current locale setting is used.</param>
        /// <returns></returns>
        public static bool SetAppSetting<T>(ConfigurationFindInfo configuration, string key, T value, string format, IFormatProvider formatProvider = null) where T : IFormattable {
            return SetAppSetting(configuration, key, value.ToString(format, formatProvider));
        }

        private static void EnsureEnoughAvailableFreeSpace(string filePath) {
            var fileInfo = new FileInfo(filePath);
            var rootPath = fileInfo.Directory?.Root.FullName;
            if (rootPath.IsNullOrWhiteSpace()) {
                throw new ArgumentException("Invalid root path found for '{0}'".FormatWith(filePath));
            }
            var driveInfo = new DriveInfo(rootPath);
            var minimumRequiredAvailableFreeSpace = ByteSize.From(ByteUnit.Mega, 50);
            if (driveInfo.AvailableFreeSpace <= minimumRequiredAvailableFreeSpace) {
                var error = $"Available free space on {rootPath} is less than {minimumRequiredAvailableFreeSpace.ToString(ByteUnit.Mega, 1)}";
                throw new IOException(error);
            }
        }
    }
}