using BECOSOFT.Utilities.Helpers;
using System;
using System.ComponentModel;
using System.Diagnostics;

namespace BECOSOFT.Utilities.IO {
    public enum ByteUnit {
        [Description("")]
        Base = 0,

        [Description("K")]
        Kilo,

        [Description("M")]
        Mega,

        [Description("G")]
        Giga,

        [Description("T")]
        Tera,

        [Description("P")]
        Peta,

        [Description("E")]
        Exa,

        [Description("Z")]
        Zetta,

        [Description("Y")]
        Yotta,
    }

    public enum ByteStandard {
        Decimal,
        Binary,
    }

    public static class ByteStandardValue {
        public const string BaseSymbol = "B";
        public const double Decimal = 1000;
        public const double Binary = 1024;
        public static double GetMultiplier(ByteStandard standard) => standard == ByteStandard.Decimal ? Decimal : Binary;

        public static double GetRatio(ByteStandard old, ByteStandard newStandard) {
            if (old == newStandard) { return 1; }
            if (old == ByteStandard.Binary) {
                return Decimal / Binary;
            }

            if (newStandard == ByteStandard.Decimal) {
                return Binary / Decimal;
            }
            return 1;
        }

        public static string GetSymbol(ByteStandard standard, ByteUnit unit) {
            var infix = standard == ByteStandard.Decimal ? string.Empty : "i";
            return unit.GetDescription() + infix + BaseSymbol;
        }
    }

    /// <summary>
    /// Inspiration: https://github.com/omar/ByteSize
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public readonly struct ByteSize : IEquatable<ByteSize>, IComparable<ByteSize> {
        public ByteStandard Standard { get; }
        public double Bytes { get; }
        public ByteUnit Unit { get; }

        public ByteSize(double byteSize, ByteStandard standard) : this(byteSize, ByteUnit.Base, standard) {
        }

        public ByteSize(double byteSize, ByteUnit unit) : this(byteSize, unit, ByteStandard.Decimal) {
        }

        public ByteSize(double byteSize) : this(byteSize, ByteUnit.Base, ByteStandard.Decimal) {
        }

        public ByteSize(double byteSize, ByteUnit unit, ByteStandard standard) {
            Bytes = byteSize;
            Unit = unit;
            Standard = standard;
        }
        

        /// <summary>
        /// Returns a <see cref="ByteSize"/> from the provided byte (B) value. The multiplier is 1000 (<see cref="ByteStandard.Decimal"/>).
        /// </summary>
        /// <param name="value">Number of bytes</param>
        /// <returns></returns>
        public static ByteSize FromByte(double value) {
            return From(ByteUnit.Base, value);
        }

        /// <summary>
        /// Returns a <see cref="ByteSize"/> from the provided kilobyte (KB) value. The multiplier is 1000 (<see cref="ByteStandard.Decimal"/>).
        /// </summary>
        /// <param name="value">Number of kilobytes</param>
        /// <returns></returns>
        public static ByteSize FromKiloByte(double value) {
            return From(ByteUnit.Kilo, value, ByteStandard.Decimal);
        }


        /// <summary>
        /// Returns a <see cref="ByteSize"/> from the provided megabyte (MB) value. The multiplier is 1000 (<see cref="ByteStandard.Decimal"/>).
        /// </summary>
        /// <param name="value">Number of megabytes</param>
        /// <returns></returns>
        public static ByteSize FromMegaByte(double value) {
            return From(ByteUnit.Mega, value, ByteStandard.Decimal);
        }


        /// <summary>
        /// Returns a <see cref="ByteSize"/> from the provided gigabyte (GB) value. The multiplier is 1000 (<see cref="ByteStandard.Decimal"/>).
        /// </summary>
        /// <param name="value">Number of gigabytes</param>
        /// <returns></returns>
        public static ByteSize FromGigaByte(double value) {
            return From(ByteUnit.Giga, value, ByteStandard.Decimal);
        }


        /// <summary>
        /// Returns a <see cref="ByteSize"/> from the provided terabyte (TB) value. The multiplier is 1000 (<see cref="ByteStandard.Decimal"/>).
        /// </summary>
        /// <param name="value">Number of terabytes</param>
        /// <returns></returns>
        public static ByteSize FromTeraByte(double value) {
            return From(ByteUnit.Tera, value, ByteStandard.Decimal);
        }

        /// <summary>
        /// Returns a <see cref="ByteSize"/> from the provided kibibyte (KiB) value. The multiplier is 1024 (<see cref="ByteStandard.Binary"/>).
        /// </summary>
        /// <param name="value">Number of kibibytes</param>
        /// <returns></returns>
        public static ByteSize FromKibiByte(double value) {
            return From(ByteUnit.Kilo, value, ByteStandard.Binary);
        }
        
        /// <summary>
        /// Returns a <see cref="ByteSize"/> from the provided mebibyte (MiB) value. The multiplier is 1024 (<see cref="ByteStandard.Binary"/>).
        /// </summary>
        /// <param name="value">Number of mebibytes</param>
        /// <returns></returns>
        public static ByteSize FromMebiByte(double value) {
            return From(ByteUnit.Mega, value, ByteStandard.Binary);
        }
        
        /// <summary>
        /// Returns a <see cref="ByteSize"/> from the provided gibibyte (GiB) value. The multiplier is 1024 (<see cref="ByteStandard.Binary"/>).
        /// </summary>
        /// <param name="value">Number of gibibytes</param>
        /// <returns></returns>
        public static ByteSize FromGibiByte(double value) {
            return From(ByteUnit.Giga, value, ByteStandard.Binary);
        }
        
        
        /// <summary>
        /// Returns a <see cref="ByteSize"/> from the provided tebibyte (TiB) value. The multiplier is 1024 (<see cref="ByteStandard.Binary"/>).
        /// </summary>
        /// <param name="value">Number of tebibytes</param>
        public static ByteSize FromTebiByte(double value) {
            return From(ByteUnit.Tera, value, ByteStandard.Binary);
        }

        public static ByteSize From(ByteUnit unit, double value) {
            return From(unit, value, ByteStandard.Decimal);
        }

        public static ByteSize From(ByteUnit unit, double value, ByteStandard standard) {
            return new ByteSize(value * GetBytesPerUnit(unit, standard), standard);
        }

        public double To(ByteUnit unit) {
            return Bytes / GetBytesPerUnit(unit, Standard);
        }

        private static double GetBytesPerUnit(ByteUnit unit, ByteStandard standard) {
            return Math.Pow(ByteStandardValue.GetMultiplier(standard), (int) unit);
        }

        public ByteSize ConvertTo(ByteStandard standard) {
            var multiplier = ByteStandardValue.GetRatio(Standard, standard);
            return new ByteSize(Bytes * multiplier, standard);
        }

        public static ByteSize ConvertTo(ByteSize size, ByteStandard standard) {
            var multiplier = ByteStandardValue.GetRatio(size.Standard, standard);
            return new ByteSize(size.Bytes * multiplier, standard);
        }

        public bool Equals(ByteSize other) {
            return Standard == other.Standard && Bytes.Equals(other.Bytes) && Unit == other.Unit;
        }

        public bool Equals(double value) {
            return Bytes.Equals(value);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (obj is double) {
                return Equals((double) obj);
            }
            return obj is ByteSize && Equals((ByteSize) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (int) Standard;
                hashCode = (hashCode * 397) ^ Bytes.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) Unit;
                return hashCode;
            }
        }

        public int CompareTo(ByteSize other) {
            var standardComparison = Standard.CompareTo(other.Standard);
            if (standardComparison != 0) { return standardComparison; }
            var bytesComparison = Bytes.CompareTo(other.Bytes);
            if (bytesComparison != 0) { return bytesComparison; }
            return Unit.CompareTo(other.Unit);
        }

        public static ByteSize operator +(ByteSize lhs, ByteSize rhs) {
            var converted = ConvertTo(rhs, lhs.Standard);
            return new ByteSize(lhs.Bytes + converted.Bytes, lhs.Unit, lhs.Standard);
        }

        public static ByteSize operator -(ByteSize lhs, ByteSize rhs) {
            var converted = ConvertTo(rhs, lhs.Standard);
            return new ByteSize(lhs.Bytes - converted.Bytes, lhs.Unit, lhs.Standard);
        }

        public static bool operator ==(ByteSize lhs, ByteSize rhs) {
            return lhs.Equals(rhs);
        }

        public static bool operator ==(ByteSize lhs, double rhs) {
            return lhs.Equals(rhs);
        }

        public static bool operator ==(double lhs, ByteSize rhs) {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(ByteSize lhs, ByteSize rhs) {
            return !lhs.Equals(rhs);
        }

        public static bool operator !=(ByteSize lhs, double rhs) {
            return !lhs.Equals(rhs);
        }

        public static bool operator !=(double lhs, ByteSize rhs) {
            return !lhs.Equals(rhs);
        }

        public static bool operator <(ByteSize lhs, ByteSize rhs) {
            var converted = ConvertTo(rhs, lhs.Standard);
            return lhs.Bytes < converted.Bytes;
        }

        public static bool operator >(ByteSize lhs, ByteSize rhs) {
            var converted = ConvertTo(rhs, lhs.Standard);
            return lhs.Bytes > converted.Bytes;
        }

        public static implicit operator double(ByteSize size) {
            return size.Bytes;
        }

        public static implicit operator int(ByteSize size) {
            return (int) size.Bytes;
        }

        public static implicit operator long(ByteSize size) {
            return (long) size.Bytes;
        }

        public override string ToString() {
            return ToString(4);
        }

        public string ToString(int decimals) {
            var format = "0" + (decimals == 0 ? "" : "." + "".PadRight(decimals, '#'));
            return $"{Bytes.ToString(format)} {ByteStandardValue.GetSymbol(Standard, Unit)}";
        }

        public string ToString(ByteUnit unit) {
            var converted = new ByteSize(To(unit), unit, Standard);
            return converted.ToString();
        }

        public string ToString(ByteUnit unit, int decimals) {
            var converted = new ByteSize(To(unit), unit, Standard);
            return converted.ToString(decimals);
        }

        public string DebuggerDisplay => $"{Bytes} {Unit.ToString()} ({Standard.ToString()})";
    }
}