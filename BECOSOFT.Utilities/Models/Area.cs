namespace BECOSOFT.Utilities.Models {
    public class Area {
        public decimal Depth { get; }
        public decimal Width { get; }
        public decimal Value => Depth * Width;
        public MetricAreaUnit? Unit { get; }

        public Area(decimal depth, decimal width, MetricAreaUnit? unit) {
            Depth = depth;
            Width = width;
            Unit = unit;
        }

        public Area(Dimension dimension)
            : this(dimension.Depth, dimension.Width, dimension.DimensionUnit.GetAreaUnit()) {
        }

        public override string ToString() {
            return $"{Value} {Unit?.GetAbbreviation() ?? ""} ({Depth} x {Width} {Unit?.GetLengthUnit().GetAbbreviation()})";
        }
    }
}