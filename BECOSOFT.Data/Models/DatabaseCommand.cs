using BECOSOFT.Data.Exceptions;
using BECOSOFT.Data.Helpers;
using BECOSOFT.Data.Parsers;
using BECOSOFT.Utilities.Extensions.Collections;
using BECOSOFT.Utilities.Helpers;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

namespace BECOSOFT.Data.Models {
    /// <summary>
    /// Wrapper around the <see cref="SqlCommand"/> class.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class DatabaseCommand {
        public static int DefaultQueryTimeout = 60;

        internal List<TempTable<object>> BulkCopyTempTables { get; set; } = new List<TempTable<object>>(0);

        public SqlInfoMessageEventHandler InfoMessageHandler;

        public bool FireInfoMessageEventOnUserErrors { get; set; }

        public SqlTransaction Transaction { get; set; }

        public string CommandText { get; set; }
        
        /// <summary>
        /// Query command timeout (default: <see cref="DefaultQueryTimeout"/>)
        /// </summary>
        public int? CommandTimeout { get; set; }

        public CommandType CommandType { get; set; }

        public UpdateRowSource UpdatedRowSource { get; set; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly List<SqlParameter> _parameters = new List<SqlParameter>();

        public IReadOnlyList<SqlParameter> Parameters => _parameters.AsReadOnly();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string _parsedCommandTextString;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        [DebuggerHidden]
        private string ParsedCommandTextString {
            get {
                if (!IsPrepared) {
                    throw new UnpreparedDatabaseCommandException();
                }
                if (_parsedCommandTextString == null) {
                    _parsedCommandTextString = QueryDumper.GetCommandText(this).ToString();
                }
                return _parsedCommandTextString;
            }
        }

        public bool IsPrepared { get; internal set; }

        /// <summary>
        /// <see cref="TablePart"/> is used when parsing (<see cref="BaseParser{T}"/>) the data reader, when checking for table existence in the result linking
        /// </summary>
        public string TablePart { get; set; }

        /// <summary>
        /// Creates a <see cref="Dictionary{TKey,TValue}"/> from the <see cref="Parameters"/>
        /// </summary>
        /// <returns>A <see cref="Dictionary{TKey,TValue}"/> from the <see cref="Parameters"/></returns>
        public Dictionary<string, SqlParameter> GetParameterDictionary() {
            var parameterCount = _parameters.Count;
            if (parameterCount == 0) {
                return new Dictionary<string, SqlParameter>();
            }
            var result = new Dictionary<string, SqlParameter>(parameterCount);
            for (var i = 0; i < parameterCount; i++) {
                var item = _parameters[i];
                result.Add(item.ParameterName, item);
            }
            return result;
        }

        /// <summary>
        /// Adds the <see cref="parameters"/>, optionally clearing the <see cref="Parameters"/> first.
        /// </summary>
        /// <param name="parameters">Parameters to add </param>
        /// <param name="withClear">Specify if the parameters  need to be cleared first.</param>
        public void AddParameters(Dictionary<string, SqlParameter> parameters, bool withClear = false) {
            if (withClear) {
                _parameters.Clear();
            }
            _parameters.AddRange(parameters.GetValueArray());
        }

        /// <summary>
        /// Adds the <see cref="parameter"/>
        /// </summary>
        /// <param name="parameter">Parameter to add </param>
        public void AddParameter(SqlParameter parameter) {
            _parameters.Add(parameter);
        }

        public string ToHashString() {
            return HashHelper.GetMd5StringFromString(ParsedCommandTextString);
        }

        public string ToHashString(string prefix) {
            var key = prefix + ParsedCommandTextString;
            return HashHelper.GetMd5StringFromString(key);
        }

        private string DebuggerDisplay => IsPrepared ? ParsedCommandTextString : "Query not yet prepared";

        /// <summary>
        /// Contains the information of the temp tables present in this <see cref="DatabaseCommand"/>.
        /// </summary>
        internal IReadOnlyList<TempTable<object>> TempTables { get; set; }

        internal string GetParsedCommandTextStringForLogging() {
            return ParsedCommandTextString;
        }
    }
}