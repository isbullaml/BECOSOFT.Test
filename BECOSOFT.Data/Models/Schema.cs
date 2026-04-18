using BECOSOFT.Data.Exceptions;
using BECOSOFT.Utilities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BECOSOFT.Data.Models {
    public enum Schema {
        //Do not change the order of this enum, only add at the end
        Unknown = 0,
        Accounting,
        Articles,
        Branches,
        Contacts,
        Dbo,
        Document,
        Drawing,
        Files,
        Global,
        Internet,
        Logs,
        Performance,
        Portal,
        Pos,
        Price2Spy,
        Production,
        Projects,
        Promotion,
        Repair,
        Requests,
        Sales,
        Salesdata,
        ScanMobile,
        Security,
        Servers,
        Servicedesk,
        Shipping,
        Staging,
        Statistic,
        Stockbase,
        Sync,
        Thirdparty,
        Warehouse,
        Website,
        Wms,
        Wordpress,
        Edi
    }

    public static class SchemaExtensions {
        public static string ToSql(this Schema schema) {
            return schema.ToString().ToLower();
        }
    }

    public static class SchemaHelpers {
        public static Schema From(string schema) {
            if (Enum.TryParse(schema, true, out Schema parsed) || parsed == Schema.Unknown) {
                return parsed;
            }
            throw new UnknownSchemaException($"Schema '{schema}' is not a known schema");
        }

        public static List<Schema> GetSqlSchemes() {
            var values = EnumHelper.GetValues<Schema>();
            return values.Where(s => s != Schema.Unknown).ToList();
        }
    }
}