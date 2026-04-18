using System.IO;
using System.Xml;

namespace BECOSOFT.Utilities.Models {
    public class IgnoreNamespaceXmlTextReader : XmlTextReader {
        public IgnoreNamespaceXmlTextReader(TextReader reader) : base(reader) { }
        public IgnoreNamespaceXmlTextReader(string fileName) : base(fileName) { }
        
        public override string NamespaceURI => string.Empty;
    }
}