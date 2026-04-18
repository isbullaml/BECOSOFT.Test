using BECOSOFT.Utilities.Converters;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace BECOSOFT.Utilities.IO {
    public static class NativeMethods {
        public static class Win32 {
            /// <summary>
            /// Queries the process handle and returns the actual file path of the process.
            /// Even if the executable is renamed, it'll will return the renamed process path.
            /// </summary>
            /// <param name="hProcess"></param>
            /// <param name="dwFlags"></param>
            /// <param name="lpExeName"></param>
            /// <param name="lpdwSize"></param>
            /// <returns></returns>
            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            private static extern bool QueryFullProcessImageName(IntPtr hProcess, uint dwFlags,
                                                                 [Out, MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpExeName,
                                                                 ref uint lpdwSize);

            /// <summary>
            /// Returns the actual executable path of the process.
            /// Even if the executable is renamed, it'll will return the renamed process path.
            /// </summary>
            /// <returns></returns>
            public static string GetExecutablePathOfCurrentProcess() {
                var process = Process.GetCurrentProcess();
                return GetExecutablePathOfProcess(process);
            }

            /// <summary>
            /// Returns the actual executable path of the process.
            /// Even if the executable is renamed, it'll will return the renamed process path.
            /// </summary>
            /// <param name="process"></param>
            /// <returns></returns>
            public static string GetExecutablePathOfProcess(Process process) {
                uint capacity = 1024;
                var capacityAsInt = capacity.To<int>();
                var sb = new StringBuilder(capacityAsInt);
                QueryFullProcessImageName(process.Handle, 0, sb, ref capacity);
                var fullPath = sb.ToString(0, capacityAsInt);
                return fullPath;
            }
        }
    }
}