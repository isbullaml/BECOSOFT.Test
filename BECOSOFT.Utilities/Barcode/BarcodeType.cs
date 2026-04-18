using System.Collections.Generic;
using System.Linq;

namespace BECOSOFT.Utilities.Barcode {
    /// <summary>
    /// The type of barcode
    /// </summary>
    public enum BarcodeType {
        /// <summary>
        /// EAN13
        /// </summary>
        Ean13,
        /// <summary>
        /// EAN8
        /// </summary>
        Ean8,
        /// <summary>
        /// Code11
        /// </summary>
        Code11,
        /// <summary>
        /// Code128 (Code 128 B)
        /// </summary>
        Code128,
        /// <summary>
        /// Code128 Auto (chooses best encoding for minimum length)
        /// </summary>
        Code128Auto,
        /// <summary>
        /// Code128C (Numeric only)
        /// </summary>
        Code128C,
        /// <summary>
        /// GTIN (EAN13 with leading zero)
        /// </summary>
        Gtin,
    }

    public static class BarcodeTypeExtensions {
        public static bool NumberSeriesUseable(this BarcodeType barcodeType) {
            return BarcodeTypeHelper.NumberSeriesUsableBarcodeTypes.Contains(barcodeType);
        }
    }

    public static class BarcodeTypeHelper {
        private static readonly List<BarcodeType> _numberSeriesUsableBarcodeTypes = new List<BarcodeType> {
            BarcodeType.Ean13, BarcodeType.Ean8, BarcodeType.Gtin,
        };

        public static IReadOnlyList<BarcodeType> NumberSeriesUsableBarcodeTypes = _numberSeriesUsableBarcodeTypes.AsReadOnly();
    }
}