using Newtonsoft.Json;
using System;
using System.Globalization;

namespace BECOSOFT.Utilities.Converters.Json {
    public class DateOnlyJsonConverter : JsonConverter<DateTime> {
        private readonly string _format = "yyyy-MM-dd";
        public override void WriteJson(JsonWriter writer, DateTime value, JsonSerializer serializer) {
            writer.WriteValue(value.ToString(_format, CultureInfo.InvariantCulture));
        }

        public override DateTime ReadJson(JsonReader reader, Type objectType, DateTime existingValue, bool hasExistingValue, JsonSerializer serializer) {
            return DateTime.ParseExact(reader.Value as string, _format, CultureInfo.InvariantCulture, DateTimeStyles.None);
        }

        public DateOnlyJsonConverter(string format) {
            _format = format;
        }

        public DateOnlyJsonConverter() {
        }
    }
}
