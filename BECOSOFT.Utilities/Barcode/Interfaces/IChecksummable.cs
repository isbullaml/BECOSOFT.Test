namespace BECOSOFT.Utilities.Barcode.Interfaces {
    /// <summary>
    /// This interface contains <see cref="Checksum"/>-related methods.
    /// </summary>
    public interface IChecksummable {
        /// <summary>
        /// Create a <see cref="Checksum"/> from a <see cref="value"/>.
        /// </summary>
        /// <param name="value">Value to get the checksum for</param>
        /// <returns></returns>
        Checksum GetChecksum(string value);
    }
}