using BECOSOFT.Utilities.Attributes;
using BECOSOFT.Utilities.Converters;
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Helpers;
using System;
using System.Linq;

namespace BECOSOFT.Utilities.Models {
    public enum MetricUnit {
        /// <summary>
        /// Pico (10^-12)
        /// </summary>
        Pico = -12,

        /// <summary>
        /// Nano (10^-9)
        /// </summary>
        Nano = -9,

        /// <summary>
        /// Micro (10^-6)
        /// </summary>
        Micro = -6,

        /// <summary>
        /// Milli (10^-3)
        /// </summary>
        Milli = -3,

        /// <summary>
        /// Centi (10^-2)
        /// </summary>
        Centi = -2,

        /// <summary>
        /// Deci (10^-1)
        /// </summary>
        Deci = -1,

        /// <summary>
        /// Base (10^0)
        /// </summary>
        Base = 0,

        /// <summary>
        /// Deca (10^1)
        /// </summary>
        Deca = 1,

        /// <summary>
        /// Hecto (10^2)
        /// </summary>
        Hecto = 2,

        /// <summary>
        /// Kilo (10^3)
        /// </summary>
        Kilo = 3,

        /// <summary>
        /// Mega (10^6)
        /// </summary>
        Mega = 6,

        /// <summary>
        /// Giga (10^9)
        /// </summary>
        Giga = 9,

        /// <summary>
        /// Tera (10^12)
        /// </summary>
        Tera = 12,
    }

    public static class MetricUnitHelper {
        public static decimal GetMultiplier(MetricUnit source, MetricUnit target) {
            if (source == target) { return 1m; }
            var distance = GetDistance(source, target);
            return (decimal) Math.Pow(10, distance);
        }

        public static decimal GetMultiplier(MetricLengthUnit source, MetricLengthUnit target) {
            if (source == target) { return 1m; }
            var distance = GetDistance(source, target);
            return (decimal) Math.Pow(10, distance);
        }

        public static decimal GetMultiplier(MetricWeightUnit source, MetricWeightUnit target) {
            if (source == target) { return 1m; }
            var distance = GetDistance(source, target);
            return (decimal) Math.Pow(10, distance);
        }

        public static decimal GetMultiplier(MetricCubicVolumeUnit source, MetricCubicVolumeUnit target) {
            if (source == target) { return 1m; }
            var distance = GetDistance(source, target);
            return (decimal) Math.Pow(10, distance);
        }

        public static decimal GetMultiplier(MetricAreaUnit source, MetricAreaUnit target) {
            if (source == target) { return 1m; }
            var distance = GetDistance(source, target);
            return (decimal) Math.Pow(10, distance);
        }

        public static int GetDistance(MetricUnit source, MetricUnit target) {
            return GetDistance((int) source, (int) target);
        }

        public static int GetDistance(MetricLengthUnit source, MetricLengthUnit target) {
            return GetDistance((int) source, (int) target);
        }

        public static int GetDistance(MetricCubicVolumeUnit source, MetricCubicVolumeUnit target) {
            return GetDistance((int) source, (int) target);
        }

        public static int GetDistance(MetricWeightUnit source, MetricWeightUnit target) {
            return GetDistance((int) source, (int) target);
        }

        public static int GetDistance(MetricAreaUnit source, MetricAreaUnit target) {
            return GetDistance((int) source, (int) target);
        }

        private static int GetDistance(int source, int target) {
            if (source == target) { return 0; }
            var distance = source - target;
            return distance;
        }

        /// <summary>
        /// Converts a <see cref="value"/> from the specified <see cref="source"/> to <see cref="target"/> unit.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="value"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static decimal Convert(MetricUnit source, decimal value, MetricUnit target) {
            var multiplier = source.GetMultiplier(target);
            return value * multiplier;
        }

        /// <summary>
        /// Converts a <see cref="value"/> from the specified <see cref="source"/> to <see cref="target"/> unit.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="value"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static decimal Convert(MetricLengthUnit source, decimal value, MetricLengthUnit target) {
            var multiplier = source.GetMultiplier(target);
            return value * multiplier;
        }

        /// <summary>
        /// Converts a <see cref="value"/> from the specified <see cref="source"/> to <see cref="target"/> unit.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="value"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static decimal Convert(MetricWeightUnit source, decimal value, MetricWeightUnit target) {
            var multiplier = source.GetMultiplier(target);
            return value * multiplier;
        }

        /// <summary>
        /// Converts a <see cref="value"/> from the specified <see cref="source"/> to <see cref="target"/> unit.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="value"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static decimal Convert(MetricCubicVolumeUnit source, decimal value, MetricCubicVolumeUnit target) {
            var multiplier = source.GetMultiplier(target);
            return value * multiplier;
        }

        /// <summary>
        /// Converts a <see cref="value"/> from the specified <see cref="source"/> to <see cref="target"/> unit.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="value"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static decimal Convert(MetricAreaUnit source, decimal value, MetricAreaUnit target) {
            var multiplier = source.GetMultiplier(target);
            return value * multiplier;
        }

        public static MetricLengthUnit? ParseLengthUnit(string value) {
            if (value.IsNullOrWhiteSpace()) { return null; }
            var temp = string.Join("", value.Where(char.IsLetter));
            var values = EnumHelper.GetValues<MetricLengthUnit>();
            foreach (var unit in values) {
                if (temp.EqualsIgnoreCase(unit.GetAbbreviation())) {
                    return unit;
                }
            }
            var parsed = Converter.GetValue<MetricLengthUnit>(temp);
            return parsed;
        }

        public static MetricWeightUnit? ParseWeightUnit(string value) {
            if (value.IsNullOrWhiteSpace()) { return null; }
            var temp = string.Join("", value.Where(char.IsLetter));
            var values = EnumHelper.GetValues<MetricWeightUnit>();
            foreach (var unit in values) {
                if (temp.EqualsIgnoreCase(unit.GetAbbreviation())) {
                    return unit;
                }
            }
            var parsed = Converter.GetValue<MetricWeightUnit>(temp);
            return parsed;
        }

        /// <summary>
        /// Returns the smaller unit of the given <see cref="MetricLengthUnit"/>. If no smaller unit exists, the same unit is returned.
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static MetricLengthUnit GetSmallerUnit(MetricLengthUnit unit) {
            return GetSmallerUnit<MetricLengthUnit>(unit);
        }

        /// <summary>
        /// Returns the larger unit of the given <see cref="MetricLengthUnit"/>. If no larger unit exists, the same unit is returned.
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static MetricLengthUnit GetLargerUnit(MetricLengthUnit unit) {
            return GetLargerUnit<MetricLengthUnit>(unit);
        }

        /// <summary>
        /// Returns the smaller unit of the given <see cref="MetricWeightUnit"/>. If no smaller unit exists, the same unit is returned.
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static MetricWeightUnit GetSmallerUnit(MetricWeightUnit unit) {
            return GetSmallerUnit<MetricWeightUnit>(unit);
        }

        /// <summary>
        /// Returns the larger unit of the given <see cref="MetricWeightUnit"/>. If no larger unit exists, the same unit is returned.
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static MetricWeightUnit GetLargerUnit(MetricWeightUnit unit) {
            return GetLargerUnit<MetricWeightUnit>(unit);
        }

        /// <summary>
        /// Returns the smaller unit of the given <see cref="MetricCubicVolumeUnit"/>. If no smaller unit exists, the same unit is returned.
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static MetricCubicVolumeUnit GetSmallerUnit(MetricCubicVolumeUnit unit) {
            return GetSmallerUnit<MetricCubicVolumeUnit>(unit);
        }

        /// <summary>
        /// Returns the larger unit of the given <see cref="MetricCubicVolumeUnit"/>. If no larger unit exists, the same unit is returned.
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static MetricCubicVolumeUnit GetLargerUnit(MetricCubicVolumeUnit unit) {
            return GetLargerUnit<MetricCubicVolumeUnit>(unit);
        }

        /// <summary>
        /// Returns the smaller unit of the given <see cref="MetricAreaUnit"/>. If no smaller unit exists, the same unit is returned.
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static MetricAreaUnit GetSmallerUnit(MetricAreaUnit unit) {
            return GetSmallerUnit<MetricAreaUnit>(unit);
        }

        /// <summary>
        /// Returns the larger unit of the given <see cref="MetricAreaUnit"/>. If no larger unit exists, the same unit is returned.
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static MetricAreaUnit GetLargerUnit(MetricAreaUnit unit) {
            return GetLargerUnit<MetricAreaUnit>(unit);
        }

        private static T GetSmallerUnit<T>(T unit) where T : Enum {
            var values = EnumHelper.GetValues<T>().OrderBy(e => e.ToInt()).ToList();
            var indexOf = values.IndexOf(unit);
            if (indexOf <= 0) { return unit; }
            return values[indexOf - 1];
        }

        private static T GetLargerUnit<T>(T unit) where T : Enum {
            var values = EnumHelper.GetValues<T>().OrderBy(e => e.ToInt()).ToList();
            var indexOf = values.IndexOf(unit);
            if (indexOf == values.Count - 1) { return unit; }
            return values[indexOf + 1];
        }
    }

    public static class MetricUnitExtensions {
        public static decimal GetMultiplier(this MetricUnit source, MetricUnit target) {
            return MetricUnitHelper.GetMultiplier(source, target);
        }

        public static decimal GetMultiplier(this MetricLengthUnit source, MetricLengthUnit target) {
            return MetricUnitHelper.GetMultiplier(source, target);
        }

        public static decimal GetMultiplier(this MetricWeightUnit source, MetricWeightUnit target) {
            return MetricUnitHelper.GetMultiplier(source, target);
        }

        public static decimal GetMultiplier(this MetricCubicVolumeUnit source, MetricCubicVolumeUnit target) {
            return MetricUnitHelper.GetMultiplier(source, target);
        }

        public static decimal GetMultiplier(this MetricAreaUnit source, MetricAreaUnit target) {
            return MetricUnitHelper.GetMultiplier(source, target);
        }

        public static decimal GetDistance(this MetricUnit source, MetricUnit target) {
            return MetricUnitHelper.GetDistance(source, target);
        }

        public static decimal GetDistance(this MetricLengthUnit source, MetricLengthUnit target) {
            return MetricUnitHelper.GetDistance(source, target);
        }

        public static decimal GetDistance(this MetricWeightUnit source, MetricWeightUnit target) {
            return MetricUnitHelper.GetDistance(source, target);
        }

        public static decimal GetDistance(this MetricCubicVolumeUnit source, MetricCubicVolumeUnit target) {
            return MetricUnitHelper.GetDistance(source, target);
        }

        public static decimal GetDistance(this MetricAreaUnit source, MetricAreaUnit target) {
            return MetricUnitHelper.GetDistance(source, target);
        }

        public static string GetAbbreviation(this MetricLengthUnit unit) {
            var abbr = unit.GetAttribute<AbbreviationAttribute>();
            return abbr?.Abbreviation ?? unit.ToString();
        }

        public static string GetAbbreviation(this MetricWeightUnit unit) {
            var abbr = unit.GetAttribute<AbbreviationAttribute>();
            return abbr?.Abbreviation ?? unit.ToString();
        }

        public static string GetAbbreviation(this MetricCubicVolumeUnit unit) {
            var abbr = unit.GetAttribute<AbbreviationAttribute>();
            return abbr?.Abbreviation ?? unit.ToString();
        }

        public static string GetAbbreviation(this MetricAreaUnit unit) {
            var abbr = unit.GetAttribute<AbbreviationAttribute>();
            return abbr?.Abbreviation ?? unit.ToString();
        }

        public static MetricCubicVolumeUnit? GetCubicUnit(this MetricLengthUnit unit) {
            var value = unit.To<int>();
            var convertedValue = value * 3;
            return convertedValue.To<MetricCubicVolumeUnit?>();
        }

        public static MetricAreaUnit? GetAreaUnit(this MetricLengthUnit unit) {
            var value = unit.To<int>();
            var convertedValue = value * 2;
            return convertedValue.To<MetricAreaUnit?>();
        }

        public static MetricLengthUnit GetLengthUnit(this MetricCubicVolumeUnit unit) {
            var value = unit.To<int>();
            var convertedValue = value / 3;
            return convertedValue.To<MetricLengthUnit>();
        }

        public static MetricLengthUnit GetLengthUnit(this MetricAreaUnit unit) {
            var value = unit.To<int>();
            var convertedValue = value / 2;
            return convertedValue.To<MetricLengthUnit>();
        }
    }
}