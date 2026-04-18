using System.IO;
using System.Xml;

namespace BECOSOFT.Utilities.Extensions.IO {
    public static class XmlDocumentExtensions {
        public static string ToXmlString(this XmlDocument xmlDocument) {
            using (var stringWriter = new StringWriter()) {
                using (var xmlTextWriter = XmlWriter.Create(stringWriter)) {
                    xmlDocument.WriteTo(xmlTextWriter);
                }
                return stringWriter.ToString();
            }
        }
    }
}
