using System;

namespace BECOSOFT.Utilities.Models.Accounting {
    public readonly struct AccountingPeriod : IEquatable<AccountingPeriod>, IComparable<AccountingPeriod> {
        public int Year { get; }
        public bool Quarterly { get; }
        public byte Period { get; }

        public AccountingPeriod(int year, bool quarterly, byte period) {
            Year = year;
            Quarterly = quarterly;
            Period = period;
        }

        public bool Equals(AccountingPeriod other) {
            return Year == other.Year
                   && Quarterly == other.Quarterly
                   && Period == other.Period;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }
            return obj is AccountingPeriod period && Equals(period);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = Year;
                hashCode = (hashCode * 397) ^ Quarterly.GetHashCode();
                hashCode = (hashCode * 397) ^ Period.GetHashCode();
                return hashCode;
            }
        }

        public int CompareTo(AccountingPeriod other) {
            var yearComparison = Year.CompareTo(other.Year);
            if (yearComparison != 0) {
                return yearComparison;
            }
            var quarterlyComparison = Quarterly.CompareTo(other.Quarterly);
            if (quarterlyComparison != 0) {
                return quarterlyComparison;
            }
            return Period.CompareTo(other.Period);
        }
    }
}