using BECOSOFT.Utilities.Extensions.Numeric;
using System.Collections.Generic;
using System.Linq;

namespace BECOSOFT.Utilities.Models {
    public class Dimension {
        public decimal Depth { get; set; }
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        public MetricLengthUnit DimensionUnit { get; set; }
        public bool Dimensionless => Depth == 0 || Width == 0 || Height == 0;

        private List<decimal> Dimensions => new[] { Depth, Width, Height, }.OrderBy(d => d).ToList();
        public decimal SmallestDimension => Dimensions[0];
        public decimal MedianDimension => Dimensions[1];
        public decimal LargestDimension => Dimensions[2];

        /// <summary>
        /// <see cref="Area"/> = <see cref="Depth"/> * <see cref="Width"/>.
        /// </summary>
        public Area Area => new Area(this);

        /// <summary>
        /// <see cref="Volume"/> = <see cref="Area"/> * <see cref="Width"/>.
        /// </summary>
        public Volume Volume => new Volume(this);

        public bool FitsIn(Dimension other) {
            var b = ToBase();
            var ob = other.ToBase();
            var fits = b.SmallestDimension < ob.SmallestDimension
                       && b.MedianDimension < ob.MedianDimension
                       && b.LargestDimension < ob.LargestDimension;
            return fits;
        }

        public Dimension ToBase() {
            var dimension = CopyDimension();
            dimension.ConvertToBase();
            return dimension;
        }

        protected virtual void ConvertToBase() {
            if (DimensionUnit == MetricLengthUnit.Metre) { return; }
            Depth = MetricUnitHelper.Convert(DimensionUnit, Depth, MetricLengthUnit.Metre);
            Width = MetricUnitHelper.Convert(DimensionUnit, Width, MetricLengthUnit.Metre);
            Height = MetricUnitHelper.Convert(DimensionUnit, Height, MetricLengthUnit.Metre);
            DimensionUnit = MetricLengthUnit.Metre;
        }

        public Dimension CopyDimension() {
            var dimension = new Dimension {
                DimensionUnit = DimensionUnit,
                Depth = Depth,
                Width = Width,
                Height = Height,
            };
            return dimension;
        }

        /// <summary>
        /// Increase a dimension (indicated by <paramref name="measurement"/>) with a given <paramref name="percentage" />.
        /// </summary>
        /// <param name="percentage"></param>
        /// <param name="measurement"></param>
        /// <returns></returns>
        public void Increase(decimal percentage, DimensionMeasurement measurement) {
            var increase = (1m + (percentage / 100));
            Depth *= (measurement == DimensionMeasurement.Depth ? increase : 1);
            Width *= (measurement == DimensionMeasurement.Width ? increase : 1);
            Height *= (measurement == DimensionMeasurement.Height ? increase : 1);
        }

        public override string ToString() {
            var baseDim = ToBase();
            return $"{baseDim.LargestDimension.RemoveTrailingZeros()} x {baseDim.MedianDimension.RemoveTrailingZeros()} x {baseDim.SmallestDimension.RemoveTrailingZeros()} ({baseDim.DimensionUnit.GetAbbreviation()})";
        }
    }
}