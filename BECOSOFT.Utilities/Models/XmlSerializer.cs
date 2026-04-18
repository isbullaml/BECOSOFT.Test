using BECOSOFT.Utilities.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace BECOSOFT.Utilities.Models {
    public class XmlSerializer<T> : XmlSerializer where T : class {
        public XmlSerializer() : base(typeof(T)) {
        }

        public XmlSerializer(string rootElementName) : base(typeof(T), new XmlRootAttribute(rootElementName)) {
        }

        public T DeserializeToModel(TextReader stream) {
            var result = (T) Deserialize(stream);
            return result;
        }

        public T DeserializeToModel(Stream stream) {
            var result = (T) Deserialize(stream);
            return result;
        }

        public T DeserializeToModel(XmlTextReader stream) {
            var result = (T) Deserialize(stream);
            return result;
        }

        public T DeserializeToModel(string xmlString) {
            using (var stringReader = new StringReader(xmlString)) {
                var result = (T)Deserialize(stringReader);
                return result;
            }
        }

        public string SerializeToString(T obj, XmlSerializerNamespaces ns = null) {
            using (var stringWriter = EncodableStringWriter.Utf8) {
                if (ns == null) {
                    Serialize(stringWriter, obj);
                } else {
                    Serialize(stringWriter, obj, ns);
                }
                return stringWriter.ToString();
            }
        }

        public XmlSerializationResult<T> DeserializeWithFeedback(TextReader stream) {
            var result = new XmlSerializationResult<T>();

            void ElementHandler(object sender, XmlElementEventArgs args) {
                var unknownAttribute = new XmlUnknown {
                    ExpectedAttributes = args.ExpectedElements,
                    LineNumber = args.LineNumber,
                    LinePosition = args.LinePosition,
                    Item = args.Element.ToString(),
                };
                result.UnknownElements.Add(unknownAttribute);
                Debug.WriteLine("E: {0}:{1}:{2} - {3}", args.LineNumber, args.LinePosition, args.ExpectedElements, args.Element.ToString());
            }

            void AttributeHandler(object sender, XmlAttributeEventArgs args) {
                var unknownAttribute = new XmlUnknown {
                    ExpectedAttributes = args.ExpectedAttributes,
                    LineNumber = args.LineNumber,
                    LinePosition = args.LinePosition,
                    Item = args.Attr.ToString(),
                };
                result.UnknownAttributes.Add(unknownAttribute);
                Debug.WriteLine("A: {0}:{1}:{2} - {3}", args.LineNumber, args.LinePosition, args.ExpectedAttributes, args.Attr.ToString());
            }

            void NodeHandler(object sender, XmlNodeEventArgs args) {
                var unknownElement = new XmlUnknown {
                    LineNumber = args.LineNumber,
                    LinePosition = args.LinePosition,
                    Item = args.LocalName,
                };
                result.UnknownNode.Add(unknownElement);
                Debug.WriteLine("N: {0}:{1}:{2} - {3} - '{4}'", args.LineNumber, args.LinePosition, args.LocalName, args.NodeType, args.Text);
            }

            UnknownElement += ElementHandler;
            UnknownAttribute += AttributeHandler;
            UnknownNode += NodeHandler;
            var deserializedResult = (T) Deserialize(stream);
            result.Result = deserializedResult;
            UnknownAttribute -= AttributeHandler;
            UnknownElement -= ElementHandler;
            UnknownNode -= NodeHandler;
            return result;
        }
    }

    public class XmlSerializationResult<T> where T : class {
        public T Result { get; set; }

        public List<XmlUnknown> UnknownAttributes { get; set; } = new List<XmlUnknown>();
        public List<XmlUnknown> UnknownElements { get; set; } = new List<XmlUnknown>();
        public List<XmlUnknown> UnknownNode { get; set; } = new List<XmlUnknown>();
    }

    public class XmlUnknown {
        public int LineNumber { get; set; }
        public int LinePosition { get; set; }
        public string ExpectedAttributes { get; set; }
        public string Item { get; set; }
    }
}