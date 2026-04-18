namespace BECOSOFT.Utilities.Barcode.Interfaces {
    /// <summary>
    /// This interface provides the necessary methods for barcode creation and validation
    /// </summary>
    public interface IBarcode {
        bool IsValid(string value);
        string Encode(string value);
    }

    public interface IBarcodeNumberSeriesEnabled {
        int GetLength();
        int GetLengthWithoutCheckDigit();
    }

    public class Checksum {
        public bool IsValid { get; set; }
        public string CheckDigit { get; set; }
    }
}