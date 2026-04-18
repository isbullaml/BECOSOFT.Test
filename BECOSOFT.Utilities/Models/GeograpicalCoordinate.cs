using System;

namespace BECOSOFT.Utilities.Models {
    public readonly struct GeographicalCoordinate : IEquatable<GeographicalCoordinate> {
        public double Degrees { get; }
        public double Minutes { get; }
        public double Seconds { get; }

        public GeographicalCoordinate(double degrees, double minutes, double seconds) {
            Degrees = degrees;
            Minutes = minutes;
            Seconds = seconds;
        }

        public override string ToString() {
            // ReSharper disable UseFormatSpecifierInFormatString
            return $"{Degrees:F0}° {Minutes:F0}' {Seconds:F0}\"";
        }

        public bool Equals(GeographicalCoordinate other) {
            return Degrees.Equals(other.Degrees) && Minutes.Equals(other.Minutes) && Seconds.Equals(other.Seconds);
        }

        public override bool Equals(object obj) {
            return obj is GeographicalCoordinate other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = Degrees.GetHashCode();
                hashCode = (hashCode * 397) ^ Minutes.GetHashCode();
                hashCode = (hashCode * 397) ^ Seconds.GetHashCode();
                return hashCode;
            }
        }
    }
}