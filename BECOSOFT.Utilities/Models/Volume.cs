namespace BECOSOFT.Utilities.Models {
    public class Volume {
        public Area Area { get; }
        public decimal Height { get; }
        public decimal Value => Area.Value * Height;
        public MetricCubicVolumeUnit? Unit { get; }

        public Volume(Area area, decimal height, MetricCubicVolumeUnit? unit) {
            Area = area;
            Height = height;
            Unit = unit;
        }

        public Volume(decimal depth, decimal width, decimal height, MetricCubicVolumeUnit? unit) {
            Area = new Area(depth, width, unit?.GetLengthUnit().GetAreaUnit());
            Height = height;
            Unit = unit;
        }

        public Volume(Dimension dimension)
            : this(new Area(dimension), dimension.Height, dimension.DimensionUnit.GetCubicUnit()) {
        }

        public override string ToString() {
            return $"{Value} {Unit?.GetAbbreviation() ?? ""} ({Area.Depth} x {Area.Width} x {Height} {Unit?.GetLengthUnit().GetAbbreviation()})";
        }
    }
}