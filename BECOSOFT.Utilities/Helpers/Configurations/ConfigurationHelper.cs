using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Models.Configurations;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BECOSOFT.Utilities.Helpers.Configurations {
    public class ConfigurationHelper {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private static readonly string ConfigFolder = "Configs";
        private readonly string _configFileName;
        private ConfigurationHelper(string prefix) {
            if (prefix.IsNullOrWhiteSpace()) {
                _configFileName = "{0}.config";
            } else {
                _configFileName = prefix + ".{0}.config";
            }
        }

        /// <summary>
        /// Attempt to find the configuration file specified by the <see cref="configuration"/> parameter. Optionally you can specify a <see cref="folder"/>.
        /// </summary>
        /// <param name="configuration">Configuration to find</param>
        /// <param name="folder">Optional folder specification where to search for the specified <param name="configuration"></param>.</param>
        /// <returns></returns>
        public ConfigurationFindInfo Find(string configuration, string folder = null) {
            string folderDirectory = null;
            if (!folder.IsNullOrWhiteSpace()) {
                Logger.Info("Checking if folder '{0}' exists.", folder);
                if (Directory.Exists(folder)) {
                    folderDirectory = folder;
                    Logger.Info("Folder '{0}' exists.", folder);
                }
            }
            var fullConfigFileName = Path.Combine(folderDirectory ?? "", _configFileName.FormatWith(configuration));
            var fullConfigFileInfo = new FileInfo(fullConfigFileName);
            Logger.Info("Checking if file '{0}' exists.", fullConfigFileName);
            if (!fullConfigFileInfo.Exists) {
                Logger.Warn("Configuration file '{0}' does not exist.", fullConfigFileName);
                var tempConfig = Path.Combine(ConfigFolder, fullConfigFileName);
                var tempFileInfo = new FileInfo(tempConfig);
                Logger.Info("Checking if file '{0}' exists.", tempConfig);
                if (tempFileInfo.Exists) {
                    fullConfigFileName = tempConfig;
                    fullConfigFileInfo = tempFileInfo;
                } else {
                    Logger.Fatal("Configuration file '{0}' does not exist.", tempConfig);
                }
            }
            if (!fullConfigFileInfo.Exists) {
                return new ConfigurationFindInfo();
            }
            Logger.Info("Configuration file '{0}' found.", fullConfigFileName);
            Logger.Debug("Checking for other configuration files.");
            var fullConfigFileDirectory = fullConfigFileInfo.Directory;
            var otherConfigFiles = new List<string>();
            if (fullConfigFileDirectory != null) {
                var tempDirectoryFileResult = Directory.GetFiles(fullConfigFileDirectory.FullName, "App.*.config", SearchOption.TopDirectoryOnly).ToList();
                otherConfigFiles = tempDirectoryFileResult.Where(f => !f.Contains(fullConfigFileName, StringComparison.InvariantCultureIgnoreCase)).ToList();
                Logger.Debug("Found {0} other configuration files.", otherConfigFiles.Count);
            }
            return new ConfigurationFindInfo(fullConfigFileInfo, otherConfigFiles);
        }

        /// <summary>
        /// Helper to find App.*.config files
        /// </summary>
        public static ConfigurationHelper App => new ConfigurationHelper("App");
        /// <summary>
        /// Helper to find Web.*.config files
        /// </summary>
        public static ConfigurationHelper Web => new ConfigurationHelper("Web");
        /// <summary>
        /// Helper to find custom named .config files
        /// </summary>
        public static ConfigurationHelper Custom => new ConfigurationHelper(null);
    }
}
