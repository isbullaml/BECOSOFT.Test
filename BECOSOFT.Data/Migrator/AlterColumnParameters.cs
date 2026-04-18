using BECOSOFT.Utilities.Extensions;
using System;
using System.Data;

namespace BECOSOFT.Data.Migrator {
    public class AlterColumnParameters {
        private int? _length;
        private int? _precision;
        private bool? _nullable;
        private string _defaultValue;

        public SqlDbType Type { get; }
        public int Length => _length ?? 0;
        public int Precision => _precision ?? 0;
        public bool Nullable => _nullable ?? true;
        public string DefaultValue => _defaultValue;

        private AlterColumnParameters(SqlDbType type) {
            Type = type;
        }

        public static AlterColumnParameters ToChar(int length, bool? nullable = null, string defaultValue = null) {
            return new AlterColumnParameters(SqlDbType.Char) {
                _length = length,
                _nullable = nullable,
                _defaultValue = defaultValue,
            };
        }

        public static AlterColumnParameters ToDecimal(int length = 18, int precision = 0, bool? nullable = null, string defaultValue = null) {
            return new AlterColumnParameters(SqlDbType.Decimal) {
                _length = length,
                _precision = precision,
                _nullable = nullable,
                _defaultValue = defaultValue,
            };
        }

        public static AlterColumnParameters ToFloat(int length = 53, bool? nullable = null, string defaultValue = null) {
            return new AlterColumnParameters(SqlDbType.Float) {
                _length = length,
                _nullable = nullable,
                _defaultValue = defaultValue,
            };
        }

        public static AlterColumnParameters ToNvarchar(int length, bool? nullable = null, string defaultValue = null) {
            return new AlterColumnParameters(SqlDbType.NVarChar) {
                _length = length,
                _nullable = nullable,
                _defaultValue = defaultValue,
            };
        }

        public static AlterColumnParameters ToVarchar(int length, bool? nullable = null, string defaultValue = null) {
            return new AlterColumnParameters(SqlDbType.VarChar) {
                _length = length,
                _nullable = nullable,
                _defaultValue = defaultValue,
            };
        }

        public static AlterColumnParameters ToNvarchar(bool max, bool? nullable = null, string defaultValue = null) {
            return new AlterColumnParameters(SqlDbType.NVarChar) {
                _length = -1,
                _nullable = nullable,
                _defaultValue = defaultValue,
            };
        }

        public static AlterColumnParameters ToVarchar(bool max, bool? nullable = null, string defaultValue = null) {
            return new AlterColumnParameters(SqlDbType.VarChar) {
                _length = -1,
                _nullable = nullable,
                _defaultValue = defaultValue,
            };
        }

        public static AlterColumnParameters ToVarbinary(int length, bool? nullable = null, string defaultValue = null) {
            return new AlterColumnParameters(SqlDbType.VarBinary) {
                _length = length,
                _nullable = nullable,
                _defaultValue = defaultValue,
            };
        }

        public static AlterColumnParameters ToVarbinary(bool max, bool? nullable = null, string defaultValue = null) {
            return new AlterColumnParameters(SqlDbType.VarBinary) {
                _length = -1,
                _nullable = nullable,
                _defaultValue = defaultValue,
            };
        }

        public static AlterColumnParameters ToBigInt(bool? nullable = null, string defaultValue = null) {
            return new AlterColumnParameters(SqlDbType.BigInt) {
                _nullable = nullable,
                _defaultValue = defaultValue,
            };
        }

        public static AlterColumnParameters ToBit(bool? nullable = null, string defaultValue = null) {
            return new AlterColumnParameters(SqlDbType.Bit) {
                _nullable = nullable,
                _defaultValue = defaultValue,
            };
        }

        public static AlterColumnParameters ToDateTime(bool? nullable = null, string defaultValue = null) {
            return new AlterColumnParameters(SqlDbType.DateTime) {
                _nullable = nullable,
                _defaultValue = defaultValue,
            };
        }

        public static AlterColumnParameters ToImage(bool? nullable = null, string defaultValue = null) {
            return new AlterColumnParameters(SqlDbType.Image) {
                _nullable = nullable,
                _defaultValue = defaultValue,
            };
        }

        public static AlterColumnParameters ToInt(bool? nullable = null, string defaultValue = null) {
            return new AlterColumnParameters(SqlDbType.Int) {
                _nullable = nullable,
                _defaultValue = defaultValue,
            };
        }

        public static AlterColumnParameters ToSmallDateTime(bool? nullable = null, string defaultValue = null) {
            return new AlterColumnParameters(SqlDbType.SmallDateTime) {
                _nullable = nullable,
                _defaultValue = defaultValue,
            };
        }

        public static AlterColumnParameters ToSmallInt(bool? nullable = null, string defaultValue = null) {
            return new AlterColumnParameters(SqlDbType.SmallInt) {
                _nullable = nullable,
                _defaultValue = defaultValue,
            };
        }

        public static AlterColumnParameters ToTinyInt(bool? nullable = null, string defaultValue = null) {
            return new AlterColumnParameters(SqlDbType.TinyInt) {
                _nullable = nullable,
                _defaultValue = defaultValue,
            };
        }

        public override string ToString() {
            string definition;
            switch (Type) {
                case SqlDbType.Char:
                    definition = $"{Type}({_length})";
                    break;
                case SqlDbType.Decimal:
                    definition = $"{Type}({_length}, {_precision})";
                    break;
                case SqlDbType.Float:
                    definition = $"{Type}({_precision})";
                    break;
                case SqlDbType.NVarChar:
                case SqlDbType.VarChar:
                    var nvarcharLength = _length.HasValue && _length.Value > 0 ? _length.Value.ToString() : "MAX";
                    definition = $"{Type}({nvarcharLength})";
                    break;
                case SqlDbType.VarBinary:
                    var varBinaryLength = _length.HasValue && _length.Value > 0 ? _length.Value.ToString() : "MAX";
                    definition = $"{Type}({varBinaryLength})";
                    break;
                case SqlDbType.BigInt:
                case SqlDbType.Bit:
                case SqlDbType.DateTime:
                case SqlDbType.Image:
                case SqlDbType.Int:
                case SqlDbType.SmallDateTime:
                case SqlDbType.SmallInt:
                case SqlDbType.TinyInt:
                    definition = Type.ToString();
                    break;
                case SqlDbType.Text:
                case SqlDbType.Binary:
                case SqlDbType.NChar:
                case SqlDbType.NText:
                case SqlDbType.Real:
                case SqlDbType.UniqueIdentifier:
                case SqlDbType.SmallMoney:
                case SqlDbType.Money:
                case SqlDbType.Timestamp:
                case SqlDbType.Variant:
                case SqlDbType.Xml:
                case SqlDbType.Udt:
                case SqlDbType.Structured:
                case SqlDbType.Date:
                case SqlDbType.Time:
                case SqlDbType.DateTime2:
                case SqlDbType.DateTimeOffset:
                    throw new NotSupportedException();
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return $"{definition} {(Nullable ? "NULL" : "NOT NULL")}";
        }
    }
}