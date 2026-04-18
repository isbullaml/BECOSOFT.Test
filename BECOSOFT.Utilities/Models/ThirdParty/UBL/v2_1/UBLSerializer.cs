using BECOSOFT.Utilities.IO;
using System.IO;
using System.Xml.Serialization;

namespace BECOSOFT.Utilities.Models.ThirdParty.UBL.v2_1 {
    public static class UBLSerializer {

        public static string Serialize<T>(T obj) where T : class {
            var ns = new XmlSerializerNamespaces();
            ns.Add("cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
            ns.Add("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");

            var serializer = new XmlSerializer(typeof(T));

            using (var stringWriter = EncodableStringWriter.Utf8) {
                serializer.Serialize(stringWriter, obj, ns);
                return stringWriter.ToString();
            }
        }

        public static T Deserialize<T>(string xml) where T : class {
            var serializer = new XmlSerializer(typeof(T));

            using (var stringReader = new StringReader(xml)) {
                return (T) serializer.Deserialize(stringReader);
            }
        }
    }
}
