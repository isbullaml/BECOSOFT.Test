using BECOSOFT.Data.Attributes;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Utilities.Annotations;
using BECOSOFT.Utilities.Converters;
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Helpers;
using System;
using System.Globalization;

namespace BECOSOFT.Data.Models.QueryData {
    [ResultTable]
    [UsedImplicitly]
    public class ServerVersion : BaseResult, ICacheableResult {
        private string _versionNumber;

        [Column]
        public string VersionNumber {
            get => _versionNumber;
            set {
                _versionNumber = value;
                Version = GetVersion();
            }
        }

        private SqlServerVersion GetVersion() {
            if (_versionNumber.IsNullOrWhiteSpace()) {
                return SqlServerVersion.Unknown;
            }
            var split = _versionNumber.ToSplitList<int>('.');
            if (split.Count < 2) {
                return SqlServerVersion.Unknown;
            }
            var values = EnumHelper.GetValues<SqlServerVersion>();
            foreach (var version in values) {
                var parsed = (int)version / 100m;
                var str = parsed.ToString("F2", CultureInfo.InvariantCulture);
                var major = str.Substring(0, str.IndexOf(".", StringComparison.InvariantCulture)).ToInt();
                var minor = str.Substring(str.IndexOf(".", StringComparison.InvariantCulture) + 1, 2).ToInt();
                if (split[0] == major && split[1] == minor) {
                    return version;
                }
            }
            return SqlServerVersion.Unknown;
        }

        public SqlServerVersion Version { get; private set; }

        /// <summary>
        /// Indicates that the server supports the following:
        /// <para>
        /// SELECT col1, col2, ... FROM ... WHERE ... ORDER BY
        /// OFFSET     10 ROWS       -- skip 10 rows
        /// FETCH NEXT 10 ROWS ONLY; -- take 10 rows
        /// </para>
        /// If the value is <see langword="false"/>, then ROW_NUMBER() should be used to sort the rows and then limit by the generated row number
        /// </summary>
        public bool SupportsOffsetAndFetchNext => Version >= SqlServerVersion.Sql2012;

        /// <summary>
        /// Indicates that the server supports the following:
        /// <para>
        /// TRY_CONVERT(INT, col)
        /// TRY_CAST(col AS INT)
        /// </para>
        /// If the value is <see langword="false"/>, then TRY_CONVERT or TRY_CAST should not be used
        /// </summary>
        public bool SupportsTryConvertOrTryCast => Version >= SqlServerVersion.Sql2012;

        /// <summary>
        /// Indicates that the SQL Server supports the STRING_AGG function
        /// </summary>
        public bool SupportsStringAggregation => Version >= SqlServerVersion.Sql2017;

        /// <summary>
        /// Indicates that the SQL Server supports the TRIM function
        /// </summary>
        public bool SupportsTrim => Version >= SqlServerVersion.Sql2017;

        /// <summary>
        /// Indicates that the SQL Server supports the STRING_SPLIT function
        /// </summary>
        public bool SupportsStringSplit => Version >= SqlServerVersion.Sql2016;

        /// <summary>
        /// Indicates that the SQL Server supports the THROW function
        /// </summary>
        public bool SupportsThrow => Version >= SqlServerVersion.Sql2012;
    }
}