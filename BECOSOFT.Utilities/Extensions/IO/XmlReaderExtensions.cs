using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Schema;

namespace BECOSOFT.Utilities.Extensions.IO {
    public static class XmlReaderExtensions {
        public static void ValidateXmlDocument(this XmlReader documentToValidate, ValidationEventHandler validationEventHandler, params FileInfo[] schemaPaths) {
            var schemaStreams = schemaPaths.Select(f => f.OpenRead()).ToArray();
            var schemaStreamReaders = schemaStreams.Select(r => (TextReader) new StreamReader(r))
                                                   .ToArray();
            try {
                documentToValidate.ValidateXmlDocument(validationEventHandler, schemaStreamReaders);
            } finally {
                foreach (var reader in schemaStreamReaders) {
                    reader.Dispose();
                }

                foreach (var stream in schemaStreams) {
                    stream.Dispose();
                }
            }
        }

        public static void ValidateXmlDocument(this XmlReader documentToValidate, ValidationEventHandler validationEventHandler, params string[] schemes) {
            var schemaStreamReaders = schemes.Select(f => (TextReader) new StringReader(f)).ToArray();
            try {
                documentToValidate.ValidateXmlDocument(validationEventHandler, schemaStreamReaders);
            } finally {
                foreach (var reader in schemaStreamReaders) {
                    reader.Dispose();
                }
            }
        }

        public static void ValidateXmlDocument(this XmlReader documentToValidate, ValidationEventHandler validationEventHandler, params TextReader[] schemaInputReaders) {
            var schemas = new XmlSchemaSet();

            foreach (var schemaInputReader in schemaInputReaders) {
                using (var schemaReader = XmlReader.Create(schemaInputReader)) {
                    var schema = XmlSchema.Read(schemaReader, validationEventHandler);
                    schemas.Add(schema);
                }
            }

            var settings = new XmlReaderSettings {
                ValidationType = ValidationType.Schema,
                Schemas = schemas,
                ValidationFlags = XmlSchemaValidationFlags.ProcessIdentityConstraints | XmlSchemaValidationFlags.ReportValidationWarnings
            };

            settings.ValidationEventHandler += validationEventHandler;
            using (var validationReader = XmlReader.Create(documentToValidate, settings)) {
                while (validationReader.Read()) { }
            }
        }
    }
}