using System.Collections.Generic;

namespace BECOSOFT.Web.Helpers {
    public class ColumnInfo {
        public string PropertyName { get; set; }
        public int ColumnKey { get; set; }
        public int ColumnIndex { get; set; }
        public string ColumnName { get; set; }
        /// <summary>
        /// Indicates whether the column is default visible or not (Column will be added to the headers / column selection)
        /// </summary>
        public bool Visible { get; set; }

        /// <summary>
        /// Indicates whether the column will be added to the headers / column selection or not
        /// </summary>
        public bool Hidden { get; set; } = false;
        public bool Sortable { get; set; }
        public string SortKey { get; set; }
        public HeaderAlignment Alignment { get; set; }
        public bool HideColumnNameInHeader { get; set; }
        public string Title { get; set; }

        public ColumnInfo(string propertyName, int columnIndex, string columnName, bool visible, bool sortable, string sortKey, HeaderAlignment alignment, bool hideColumnNameInHeader, string title) {
            PropertyName = propertyName;
            ColumnIndex = columnIndex;
            ColumnKey = columnIndex;
            ColumnName = columnName;
            Visible = visible;
            Sortable = sortable;
            SortKey = sortKey;
            Alignment = alignment;
            HideColumnNameInHeader = hideColumnNameInHeader;
            Title = title;
        }

        public ColumnInfo() { }
    }

    public enum HeaderAlignment {
        Left,
        Right
    }

    public class TableInfo {
        public List<ColumnInfo> Columns { get; set; }
        public string Key { get; set; }
    }
}