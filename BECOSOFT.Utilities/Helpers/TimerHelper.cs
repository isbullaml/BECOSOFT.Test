using BECOSOFT.Utilities.Models;
using System.Timers;

namespace BECOSOFT.Utilities.Helpers {
    public static class TimerHelper {
        public static Timer CreateWith(double interval, TimeInterval intervalUnit, Month? month = null, int? year = null) {
            var intervalInMilliseconds = TimeHelper.GetInterval(interval, intervalUnit, month, year);
            return new Timer(intervalInMilliseconds);
        }
    }
}