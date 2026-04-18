using BECOSOFT.Data.Models;
using BECOSOFT.Utilities.Extensions.Collections;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;

namespace BECOSOFT.Data.Query {
    internal class BulkCopyHelper : IBulkCopyHelper {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        public void Handle(List<TempTable<object>> tempTables, SqlConnection connection, Func<DatabaseCommand, int> commandExecuter) {
            if (tempTables.IsEmpty()) { return; }
            var tempTableCreation = new StringBuilder();
            foreach (var tempTable in tempTables) {
                tempTableCreation.AppendLine(tempTable.GetCreationScript());
            }
            var command = new DatabaseCommand {
                CommandText = tempTableCreation.ToString(),
                IsPrepared = true,
            };
            commandExecuter(command);
            foreach (var tempTable in tempTables) {
                using (var bulkCopy = new SqlBulkCopy(connection)) {
                    bulkCopy.DestinationTableName = tempTable.TableName;
                    IDataReader reader;
                    if (tempTable.IsBulkCopyable) {
                        var indexedBulkCopyableProperties = tempTable.GetIndexedBulkCopyableProperties();
                        for (var index = 0; index < indexedBulkCopyableProperties.Count; index++) {
                            bulkCopy.ColumnMappings.Add(index, index);
                        }
                        reader = new BulkCopyableDataReader(tempTable.Values, indexedBulkCopyableProperties);
                    } else {
                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(0, 0));
                        reader = new EnumerableDataReader<object>(tempTable.Values);
                    }
                    try {
                        Logger.Debug("Preparing to bulk copy {0} items ({1} columns)", tempTable.Values.Count, bulkCopy.ColumnMappings.Count);
                        bulkCopy.BatchSize = 8000;
                        bulkCopy.WriteToServer(reader);
                        Logger.Debug("Bulk copied {0} items", tempTable.Values.Count);
                    } catch (Exception e) {
                        Logger.Error(e);
                        throw;
                    }
                }
            }
        }

        private class EnumerableDataReader<T> : IDataReader {
            private readonly IEnumerator<T> _iterator;

            internal EnumerableDataReader(IEnumerable<T> items) {
                _iterator = items.GetEnumerator();
            }

            public void Dispose() {
                Close();
            }

            public string GetName(int i) {
                throw new NotImplementedException();
            }

            public string GetDataTypeName(int i) {
                throw new NotImplementedException();
            }

            public Type GetFieldType(int i) {
                throw new NotImplementedException();
            }

            public object GetValue(int i) {
                return _iterator.Current;
            }

            public int GetValues(object[] values) {
                values[0] = GetValue(0);
                return 1;
            }

            public int GetOrdinal(string name) {
                throw new NotImplementedException();
            }

            public bool GetBoolean(int i) {
                return (bool)GetValue(i);
            }

            public byte GetByte(int i) {
                return (byte)GetValue(i);
            }

            public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length) {
                throw new NotImplementedException();
            }

            public char GetChar(int i) {
                return (char)GetValue(i);
            }

            public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length) {
                throw new NotImplementedException();
            }

            public Guid GetGuid(int i) {
                return (Guid)GetValue(i);
            }

            public short GetInt16(int i) {
                return (short)GetValue(i);
            }

            public int GetInt32(int i) {
                return (int)GetValue(i);
            }

            public long GetInt64(int i) {
                return (long)GetValue(i);
            }

            public float GetFloat(int i) {
                return (float)GetValue(i);
            }

            public double GetDouble(int i) {
                return (double)GetValue(i);
            }

            public string GetString(int i) {
                return (string)GetValue(i);
            }

            public decimal GetDecimal(int i) {
                return (decimal)GetValue(i);
            }

            public DateTime GetDateTime(int i) {
                return (DateTime)GetValue(i);
            }

            public IDataReader GetData(int i) {
                throw new NotImplementedException();
            }

            public bool IsDBNull(int i) {
                return GetValue(i) == null;
            }

            public int FieldCount => 1;

            public object this[int i] => GetValue(i);

            public object this[string name] => GetValue(0);

            public void Close() {
                _iterator.Dispose();
            }

            public DataTable GetSchemaTable() {
                throw new NotImplementedException();
            }

            public bool NextResult() {
                throw new NotImplementedException();
            }

            public bool Read() {
                return _iterator.MoveNext();
            }

            public int Depth => 0;
            public bool IsClosed => _iterator == null;
            public int RecordsAffected => 0;
        }

        private class BulkCopyableDataReader : IDataReader {
            private readonly IReadOnlyList<object> _values;
            private readonly IReadOnlyList<PropertyInfo> _indexedBulkCopyableProperties;
            private readonly Dictionary<string, int> _indexByLowerPropertyName;
            private readonly List<string> _propertyNameByIndex;

            private int TotalItems => _values.Count;
            private int _currentIndex = -1;

            public BulkCopyableDataReader(IReadOnlyList<object> tempTableValues, IReadOnlyList<PropertyInfo> indexedBulkCopyableProperties) {
                _values = tempTableValues;
                _indexedBulkCopyableProperties = indexedBulkCopyableProperties;
                _indexByLowerPropertyName = new Dictionary<string, int>(indexedBulkCopyableProperties.Count);
                _propertyNameByIndex = new List<string>(indexedBulkCopyableProperties.Count);
                for (var index = 0; index < indexedBulkCopyableProperties.Count; index++) {
                    var indexedBulkCopyableProperty = indexedBulkCopyableProperties[index];
                    _indexByLowerPropertyName[indexedBulkCopyableProperty.Name.ToLowerInvariant()] = index;
                    _propertyNameByIndex.Add(indexedBulkCopyableProperty.Name);
                }
            }

            public void Dispose() {
                Close();
            }

            public string GetName(int i) => _propertyNameByIndex[i];

            public string GetDataTypeName(int i) {
                throw new NotImplementedException();
            }

            public Type GetFieldType(int i) {
                throw new NotImplementedException();
            }

            public object GetValue(int i) {
                var currentObject = _values[_currentIndex];
                var propInfo = _indexedBulkCopyableProperties[i];
                return propInfo.GetValue(currentObject);
            }

            public int GetValues(object[] values) {
                throw new NotImplementedException();
            }

            public int GetOrdinal(string name) {
                return _indexByLowerPropertyName[name.ToLowerInvariant()];
            }

            public bool GetBoolean(int i) {
                return (bool)GetValue(i);
            }

            public byte GetByte(int i) {
                return (byte)GetValue(i);
            }

            public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length) {
                throw new NotImplementedException();
            }

            public char GetChar(int i) {
                return (char)GetValue(i);
            }

            public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length) {
                throw new NotImplementedException();
            }

            public Guid GetGuid(int i) {
                return (Guid)GetValue(i);
            }

            public short GetInt16(int i) {
                return (short)GetValue(i);
            }

            public int GetInt32(int i) {
                return (int)GetValue(i);
            }

            public long GetInt64(int i) {
                return (long)GetValue(i);
            }

            public float GetFloat(int i) {
                return (float)GetValue(i);
            }

            public double GetDouble(int i) {
                return (double)GetValue(i);
            }

            public string GetString(int i) {
                return (string)GetValue(i);
            }

            public decimal GetDecimal(int i) {
                return (decimal)GetValue(i);
            }

            public DateTime GetDateTime(int i) {
                return (DateTime)GetValue(i);
            }

            public IDataReader GetData(int i) {
                throw new NotImplementedException();
            }

            public bool IsDBNull(int i) {
                return GetValue(i) == null;
            }

            public int FieldCount => _indexedBulkCopyableProperties.Count;

            public object this[int i] => GetValue(i);

            public object this[string name] => GetValue(_indexByLowerPropertyName[name.ToLowerInvariant()]);

            public void Close() {
            }

            public DataTable GetSchemaTable() {
                throw new NotImplementedException();
            }

            public bool NextResult() {
                throw new NotImplementedException();
            }

            public bool Read() {
                if (_currentIndex < TotalItems - 1) {
                    _currentIndex++;
                    return true;
                }
                return false;
            }

            public int Depth => 0;
            public bool IsClosed => _values == null;
            public int RecordsAffected => 0;
        }
    }

}