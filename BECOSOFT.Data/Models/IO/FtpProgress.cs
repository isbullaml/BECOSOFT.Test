namespace BECOSOFT.Data.Models.IO {
    public class FtpProgress {
        public long BytesTransferred { get; private set; }
        public long TotalBytes { get; }
        public int FilesTransferred { get; private set; }
        public int TotalFiles { get; }
        public bool SizeProgress { get; set; }

        public FtpProgress(long totalBytes, int totalFiles, bool sizeProgress) {
            TotalBytes = totalBytes;
            TotalFiles = totalFiles;
            SizeProgress = sizeProgress;
        }

        public void UpdateByteProgress(long bytesTransferred) {
            BytesTransferred += bytesTransferred;
        }

        public void UpdateFileProgress() {
            FilesTransferred += 1;
        }
    }
}
