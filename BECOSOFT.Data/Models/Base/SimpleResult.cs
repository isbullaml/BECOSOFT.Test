using BECOSOFT.Data.Attributes;
using System.Diagnostics;

namespace BECOSOFT.Data.Models.Base {
    [ResultTable]
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class SimpleResult<T> : BaseResult {
        [Column]
        public int ID { get; set; }

        [Column]
        public T Value { get; set; }

        private string DebuggerDisplay => $"{ID}, Value: {Value} (Type: {nameof(T)})";
    }
}
