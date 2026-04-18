using BECOSOFT.Data.Models;
using BECOSOFT.Utilities.Extensions;
using System.Text;

namespace BECOSOFT.Data.Helpers {
    public static class TableHelper {
        public static string GetCombined(Schema schema, string part, string tablePart = null) {
            if (schema == 0) {
                schema = Schema.Dbo;
            }
            part = Clean(part).EscapeEnterCharacters().SqlEscape();
            if (part.IsNullOrWhiteSpace()) {
                return string.Empty;
            }
            if (!tablePart.IsNullOrWhiteSpace()) {
                part = string.Format(part, tablePart);
            }
            var tableBuilder = new StringBuilder();
            tableBuilder.Append("[").Append(schema.ToSql()).Append("].");
            tableBuilder.Append("[").Append(part).Append("]");
            return tableBuilder.ToString();
        }

        public static string Clean(string part, bool removeBrackets = true) {
            if (part.IsNullOrWhiteSpace()) {
                return string.Empty;
            }
            var partBuilder = new StringBuilder(part);
            partBuilder.Replace("'", "''");
            if (removeBrackets) {
                partBuilder.Replace("[", "").Replace("]", "");
            }
            return partBuilder.ToString();
        }

        public static TableDefinition GetDefinition(Schema schema, string table) {
            var tableName = table;
            if (schema == 0 && tableName.Contains(".")) {
                var tableSplit = tableName.Split('.');
                schema = SchemaHelpers.From(Clean(tableSplit[0].EscapeEnterCharacters().SqlEscape()));
                tableName = tableSplit[1];
            }
            tableName = Clean(tableName.EscapeEnterCharacters().SqlEscape());

            return new TableDefinition(schema, tableName);
        }
    }
}