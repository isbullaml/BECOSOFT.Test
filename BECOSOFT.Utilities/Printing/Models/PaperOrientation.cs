using BECOSOFT.Utilities.Attributes;

namespace BECOSOFT.Utilities.Printing.Models {
    public enum PaperOrientation {
        /// <summary>
        /// Portrait (0)
        /// </summary>
        [LocalizedEnum("Orientation_Portrait", NameResourceType = typeof(Resources))]
        Portrait = 0,
        /// <summary>
        /// Landscape (1)
        /// </summary>
        [LocalizedEnum("Orientation_Landscape", NameResourceType = typeof(Resources))]
        Landscape = 1,
    }
}