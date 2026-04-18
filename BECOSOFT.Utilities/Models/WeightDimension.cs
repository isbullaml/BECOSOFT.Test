using BECOSOFT.Utilities.Extensions.Numeric;

namespace BECOSOFT.Utilities.Models {
    /// <summary>
    /// This class extends <see cref="Dimension"/> and adds weight properties
    /// </summary>
    public class WeightDimension : Dimension {
        public decimal Weight { get; set; }
        public MetricWeightUnit WeightUnit { get; set; }
        public bool Weightless => Weight == 0;

        public bool FitsIn(WeightDimension other) {
            var b = ToBaseWithWeight();
            var ob = other.ToBaseWithWeight();
            var fits = b.FitsIn((Dimension) ob);
            if (!fits) { return false; }
            if (b.Weightless || ob.Weightless) { return true; }
            return b.Weight < ob.Weight;
        }

        public WeightDimension ToBaseWithWeight() {
            var weightDimension = CopyWeightDimension();
            weightDimension.ConvertToBase();
            return weightDimension;
        }

        protected override void ConvertToBase() {
            base.ConvertToBase();
            if (WeightUnit == MetricWeightUnit.Gram) { return; }
            if (!Weightless) {
                Weight = MetricUnitHelper.Convert(WeightUnit, Weight, MetricWeightUnit.Gram);
            }
            WeightUnit = MetricWeightUnit.Gram;
        }

        public WeightDimension CopyWeightDimension() {
            var dimension = new WeightDimension {
                DimensionUnit = DimensionUnit,
                Depth = Depth,
                Width = Width,
                Height = Height,
                WeightUnit = WeightUnit,
                Weight = Weight,
            };
            return dimension;
        }

        /// <summary>
        /// Increase the weight with a given <paramref name="percentage" />.
        /// </summary>
        /// <param name="percentage"></param>
        /// <returns></returns>
        public void IncreaseWeight(decimal percentage) {
            Weight *= (1m + (percentage / 100));
        }

        public override string ToString() {
            var baseDim = ToBaseWithWeight();
            return $"{baseDim.DimensionToString()}, {baseDim.Weight.RemoveTrailingZeros()} ({baseDim.WeightUnit.GetAbbreviation()})";
        }

        private string DimensionToString() => base.ToString();
    }
}