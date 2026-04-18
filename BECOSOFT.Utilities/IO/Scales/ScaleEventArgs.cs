using BECOSOFT.Utilities.Models;
using System;

namespace BECOSOFT.Utilities.IO.Scales {
    public class ScaleEventArgs : EventArgs {
        public decimal? Weight { get; }
        public MetricWeightUnit Unit { get; }
        public decimal? Price { get; }

        public ScaleEventArgs(decimal? weight, MetricWeightUnit unit) {
            Weight = weight;
            Unit = unit;
        }

        public ScaleEventArgs(decimal? weight, MetricWeightUnit unit, decimal? price) {
            Weight = weight;
            Unit = unit;
            Price = price;
        }
    }
}