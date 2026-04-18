using System;

namespace BECOSOFT.Utilities.Extensions {
    public static class ExceptionExtensions {
        public static long ConvertHResult(this Exception ex) {
            return (long) int.MaxValue + ex.HResult;
        }

        public static bool IsErrorCode(this Exception ex, int errorCode) {
            return ConvertHResult(ex) == errorCode;
        }

        public static bool IsErrorCode(this Exception ex, uint errorCode) {
            return ConvertHResult(ex) == errorCode;
        }
    }
}
