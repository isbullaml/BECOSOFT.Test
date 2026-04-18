namespace BECOSOFT.Utilities.Printing.Models {
    public interface IPageSizeDefinition {
        /// <summary>
        /// Width in millimeter
        /// </summary>
        int Width { get; set; }

        /// <summary>
        /// Height in millimeter
        /// </summary>
        int Height { get; set; }

        int Dpi { get; set; }
        decimal? OffsetX { get; set; }
        decimal? OffsetY { get; set; }
        decimal? Gap { get; set; }
        bool? OverrulePageSizeDefinition { get; set; }
    }
}