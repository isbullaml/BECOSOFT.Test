using BECOSOFT.Data.Attributes;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Utilities.IO;

namespace BECOSOFT.Data.Models.QueryData {
    public class DatabaseSize : BaseResult {
        [Column]
        public double RawDatabaseSize { get; set; }

        [Column]
        public double RawDatabaseFreeSpace { get; set; }

        [Column]
        public double RawLogSize { get; set; }

        [Column]
        public double RawLogFreeSpace { get; set; }

        /// <summary>
        /// Data file size (on disk)
        /// </summary>
        public ByteSize DataFile => ByteSize.From(ByteUnit.Mega, RawDatabaseSize, ByteStandard.Binary);

        /// <summary>
        /// Log file size (on disk)
        /// </summary>
        public ByteSize LogFile => ByteSize.From(ByteUnit.Mega, RawLogSize, ByteStandard.Binary);

        /// <summary>
        /// Data file free space 
        /// </summary>
        public ByteSize DataFreeSpace => ByteSize.From(ByteUnit.Mega, RawDatabaseFreeSpace, ByteStandard.Binary);

        /// <summary>
        /// Log file free space
        /// </summary>
        public ByteSize LogFreeSpace => ByteSize.From(ByteUnit.Mega, RawLogFreeSpace, ByteStandard.Binary);

        /// <summary>
        /// Total file size (data + log) on disk
        /// </summary>
        public ByteSize Total => DataFile + LogFile;

        /// <summary>
        /// Total file free space (data + log) on disk
        /// </summary>
        public ByteSize TotalFreeSpace => DataFreeSpace + LogFreeSpace;

        /// <summary>
        /// Actual free space remaining
        /// </summary>
        public ByteSize DataActualSize => DataFile - DataFreeSpace;

        /// <summary>
        /// Actual log space remaining
        /// </summary>
        public ByteSize LogActualSize => LogFile - LogFreeSpace;

        /// <summary>
        /// Actual total space remaining
        /// </summary>
        public ByteSize TotalActualSize => DataActualSize + LogActualSize;

        /// <summary>
        /// Indicates that the current instance of <see cref="DatabaseSize"/> is empty.
        /// </summary>
        public bool IsEmpty => RawLogSize == 0 && RawLogFreeSpace == 0 && RawDatabaseSize == 0 && RawDatabaseFreeSpace == 0;

        public static DatabaseSize Empty = new DatabaseSize();
    }
}