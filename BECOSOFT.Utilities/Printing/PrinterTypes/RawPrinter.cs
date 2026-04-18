using BECOSOFT.Utilities.Extensions;
using Microsoft.Win32.SafeHandles;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace BECOSOFT.Utilities.Printing.PrinterTypes {
    /// <summary>
    /// https://github.com/frogmorecs/RawPrint
    /// </summary>
    public sealed class RawPrinter : IRawPrinter {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        public event JobCreatedHandler OnJobCreated;

        public void PrintRawBytes(string printer, byte[] bytes, bool paused = false) {
            using (var ms = new MemoryStream(bytes)) {
                PrintRawStream(printer, ms, "byte content", paused);
            }
        }

        public void PrintRawBytes(string printer, byte[] bytes, string documentName, bool paused = false) {
            using (var ms = new MemoryStream(bytes)) {
                PrintRawStream(printer, ms, documentName, paused);
            }
        }

        public void PrintRawFile(string printer, string path, bool paused = false) {
            PrintRawFile(printer, path, path, paused);
        }

        public void PrintRawFile(string printer, string path, string documentName, bool paused = false) {
            using (var stream = File.OpenRead(path)) {
                PrintRawStream(printer, stream, documentName, paused);
            }
        }

        public void PrintRawStream(string printer, Stream stream, string documentName, bool paused = false) {
            PrintRawStream(printer, stream, documentName, 1, paused);
        }

        public void PrintRawStream(string printer, Stream stream, string documentName, int pagecount, bool paused = false) {
            if (printer.IsNullOrWhiteSpace()) {
                throw new ArgumentException("An empty printer name is not allowed", nameof(printer));
            }
            var defaults = new PrinterDefaults {
                DesiredPrinterAccess = PrinterAccessMask.PrinterAccessUse
            };

            using (var safePrinter = SafePrinter.OpenPrinter(printer, ref defaults)) {
                DocPrinter(safePrinter, documentName, IsXPSDriver(safePrinter) ? "XPS_PASS" : "RAW", stream, paused, pagecount, printer);
            }
        }

        private static bool IsXPSDriver(SafePrinter printer) {
            var files = printer.GetPrinterDriverDependentFiles();

            return files.Any(f => f.EndsWith("pipelineconfig.xml", StringComparison.InvariantCultureIgnoreCase));
        }

        private void DocPrinter(SafePrinter printer, string documentName, string dataType, Stream stream, bool paused, int pagecount, string printerName) {
            var di1 = new DocumentInfo {
                pDataType = dataType,
                pDocName = documentName,
            };

            var id = printer.StartDocPrinter(di1);

            if (paused) {
                NativeMethods.SetJob(printer.DangerousGetHandle(), id, 0, IntPtr.Zero, (int) JobControl.Pause);
            }

            OnJobCreated?.Invoke(this, new JobCreatedEventArgs { Id = id, PrinterName = printerName });

            try {
                PagePrinter(printer, stream, pagecount);
            } finally {
                printer.EndDocPrinter();
            }
        }

        private static void PagePrinter(SafePrinter printer, Stream stream, int pagecount) {
            printer.StartPagePrinter();

            try {
                WritePrinter(printer, stream);
            } finally {
                printer.EndPagePrinter();
            }

            // Fix the page count in the final document
            for (var i = 1; i < pagecount; i++) {
                printer.StartPagePrinter();
                printer.EndPagePrinter();
            }
        }

        private static void WritePrinter(SafePrinter printer, Stream stream) {
            stream.Seek(0, SeekOrigin.Begin);

            const int bufferSize = 1048576;
            var buffer = new byte[bufferSize];

            int read;
            while ((read = stream.Read(buffer, 0, bufferSize)) != 0) {
                printer.WritePrinter(buffer, read);
            }
        }

        // Structure and API declarions:
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct DocumentInfo {
            public string pDocName;
            public string pOutputFile;
            public string pDataType;
        }

        [Flags]
        internal enum PrinterAccessMask : uint {
            PrinterAccessAdminister = 0x00000004,
            PrinterAccessUse = 0x00000008,
            PrinterAccessManageLimited = 0x00000040,
            PrinterAllAccess = 0x000F000C,
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct PrinterDefaults {
            public string pDatatype;
            private IntPtr pDevMode;
            public PrinterAccessMask DesiredPrinterAccess;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct DriverInfo {
            public uint cVersion;
            public string pName;
            public string pEnvironment;
            public string pDriverPath;
            public string pDataFile;
            public string pConfigFile;
            public string pHelpFile;
            public IntPtr pDependentFiles;
            public string pMonitorName;
            public string pDefaultDataType;
        }

        public enum JobControl {
            Pause = 0x01,
            Resume = 0x02,
            Cancel = 0x03,
            Restart = 0x04,
            Delete = 0x05,
            Retain = 0x08,
            Release = 0x09,
        }

        internal class NativeMethods {
            [DllImport("winspool.drv", SetLastError = true)]
            public static extern int ClosePrinter(IntPtr hPrinter);

            [DllImport("winspool.drv", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern int GetPrinterDriver(IntPtr hPrinter, string pEnvironment, int level, IntPtr pDriverInfo, int cbBuf, ref int pcbNeeded);

            [DllImport("winspool.drv", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern uint StartDocPrinterW(IntPtr hPrinter, uint level, [MarshalAs(UnmanagedType.Struct)] ref DocumentInfo di1);

            [DllImport("winspool.drv", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern int EndDocPrinter(IntPtr hPrinter);

            [DllImport("winspool.drv", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern int StartPagePrinter(IntPtr hPrinter);

            [DllImport("winspool.drv", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern int EndPagePrinter(IntPtr hPrinter);

            [DllImport("winspool.drv", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern int WritePrinter(IntPtr hPrinter, [In, Out] byte[] pBuf, int cbBuf, ref int pcWritten);

            [DllImport("winspool.drv", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern int OpenPrinterW(string pPrinterName, out IntPtr phPrinter, ref PrinterDefaults pDefault);

            [DllImport("winspool.drv", EntryPoint = "SetJobA", SetLastError = true)]
            public static extern int SetJob(IntPtr hPrinter, uint jobId, uint level, IntPtr pJob, uint commandRenamed);
        }


        internal class SafePrinter : SafeHandleZeroOrMinusOneIsInvalid {
            private SafePrinter(IntPtr hPrinter)
                : base(true) {
                handle = hPrinter;
            }

            protected override bool ReleaseHandle() {
                if (IsInvalid) {
                    return false;
                }

                var result = NativeMethods.ClosePrinter(handle) != 0;
                handle = IntPtr.Zero;

                return result;
            }

            public uint StartDocPrinter(DocumentInfo documentInfo) {
                var id = NativeMethods.StartDocPrinterW(handle, 1, ref documentInfo);
                if (id == 0) {
                    if (Marshal.GetLastWin32Error() == 1804) {
                        throw new Exception("The specified datatype is invalid, try setting 'Enable advanced printing features' in printer properties.", new Win32Exception());
                    }
                    throw new Win32Exception();
                }

                return id;
            }

            public void EndDocPrinter() {
                if (NativeMethods.EndDocPrinter(handle) == 0) {
                    throw new Win32Exception();
                }
            }

            public void StartPagePrinter() {
                if (NativeMethods.StartPagePrinter(handle) == 0) {
                    throw new Win32Exception();
                }
            }

            public void EndPagePrinter() {
                if (NativeMethods.EndPagePrinter(handle) == 0) {
                    throw new Win32Exception();
                }
            }

            public void WritePrinter(byte[] buffer, int size) {
                var written = 0;
                if (NativeMethods.WritePrinter(handle, buffer, size, ref written) == 0) {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }

            public IEnumerable<string> GetPrinterDriverDependentFiles() {
                var bufferSize = 0;
                const int errorInsufficientBuffer = 122;
                if (NativeMethods.GetPrinterDriver(handle, null, 3, IntPtr.Zero, 0, ref bufferSize) != 0 || Marshal.GetLastWin32Error() != errorInsufficientBuffer) {
                    throw new Win32Exception();
                }

                var ptr = Marshal.AllocHGlobal(bufferSize);

                try {
                    if (NativeMethods.GetPrinterDriver(handle, null, 3, ptr, bufferSize, ref bufferSize) == 0) {
                        throw new Win32Exception();
                    }

                    var di3 = (DriverInfo) Marshal.PtrToStructure(ptr, typeof(DriverInfo));

                    return ReadMultiSz(di3.pDependentFiles).ToList(); // We need a list because FreeHGlobal will be called on return
                } finally {
                    Marshal.FreeHGlobal(ptr);
                }
            }

            private static IEnumerable<string> ReadMultiSz(IntPtr ptr) {
                if (ptr == IntPtr.Zero) {
                    yield break;
                }

                var builder = new StringBuilder();
                var pos = ptr;

                while (true) {
                    var c = (char) Marshal.ReadInt16(pos);

                    if (c == '\0') {
                        if (builder.Length == 0) {
                            break;
                        }

                        yield return builder.ToString();
                        builder = new StringBuilder();
                    } else {
                        builder.Append(c);
                    }

                    pos += 2;
                }
            }

            public static SafePrinter OpenPrinter(string printerName, ref PrinterDefaults defaults) {
                IntPtr hPrinter;

                if (NativeMethods.OpenPrinterW(printerName, out hPrinter, ref defaults) == 0) {
                    throw new Win32Exception();
                }

                return new SafePrinter(hPrinter);
            }
        }
    }

    public class JobCreatedEventArgs : EventArgs {
        public uint Id { get; set; }
        public string PrinterName { get; set; }
    }

    public delegate void JobCreatedHandler(object sender, JobCreatedEventArgs e);

    public interface IRawPrinter {
        void PrintRawBytes(string printer, byte[] bytes, bool paused = false);
        void PrintRawBytes(string printer, byte[] bytes, string documentName, bool paused = false);
        void PrintRawFile(string printer, string path, bool paused = false);
        void PrintRawFile(string printer, string path, string documentName, bool paused = false);
        void PrintRawStream(string printer, Stream stream, string documentName, bool paused = false);
        void PrintRawStream(string printer, Stream stream, string documentName, int pagecount, bool paused = false);

        event JobCreatedHandler OnJobCreated;
    }
}