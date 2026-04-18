using System.Text;

namespace BECOSOFT.Data.Helpers {
    public static class ColumnHelper {
        public static string Clean(string part, bool removeBrackets = true) {
            var partBuilder = new StringBuilder(part);
            partBuilder.Replace("'", "''");
            if (removeBrackets) {
                partBuilder.Replace("[", "").Replace("]", "");
            }
            return partBuilder.ToString();
        }
    }
}