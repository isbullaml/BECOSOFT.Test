using System.Collections.Generic;
using System.Xml.Serialization;

namespace BECOSOFT.Utilities.Models.Configurations {
    [XmlRoot("configuration")]
    public class Configuration {
        [XmlElement("connectionStrings")]
        public ConnectionStringSection ConnectionStringSection { get; set; }
        
        [XmlElement("appSettings")]
        public ApplicationSettingSection AppSettingSection { get; set; }
    }

    public class ConnectionStringSection {
        [XmlElement("add")]
        public List<ConnectionString> ConnectionStrings { get; set; }
    }

    public class ConnectionString {
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlAttribute("connectionString")]
        public string Connection { get; set; }
        [XmlAttribute("providerName")]
        public string ProviderName { get; set; }
    }

    public class ApplicationSettingSection {
        [XmlElement("add")]
        public List<ApplicationSetting> Settings { get; set; }
    }

    public class ApplicationSetting {
        [XmlAttribute("key")]
        public string Key { get; set; }

        [XmlAttribute("value")]
        public string Value { get; set; }
    }
}