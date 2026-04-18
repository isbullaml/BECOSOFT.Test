using System.Collections.Generic;
using System.IO;

namespace BECOSOFT.Utilities.Models.Configurations {
    public class ConfigurationFindInfo {
        /// <summary>
        /// <see cref="FileInfo"/> of the configuration file.
        /// </summary>
        public FileInfo ConfigurationFile { get; }
        public bool FoundConfiguration => ConfigurationFile != null && ConfigurationFile.Exists;
        /// <summary>
        /// Other configuration files present in the folder where the <see cref="ConfigurationFile"/> was found.
        /// </summary>
        public List<string> OtherConfigurationFiles { get; } = new List<string>();

        public ConfigurationFindInfo() {
        }

        public ConfigurationFindInfo(FileInfo configurationFile, List<string> otherConfigurationFiles) {
            ConfigurationFile = configurationFile;
            OtherConfigurationFiles = otherConfigurationFiles;
        }
    }
}