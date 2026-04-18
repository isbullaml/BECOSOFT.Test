using BECOSOFT.Data.Models.Base;
using BECOSOFT.Utilities.Extensions;
using System.Diagnostics;

namespace BECOSOFT.Data.Models.Common {
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class Email : IValidatable {
        public string Address { get; }
        public string DisplayName { get; }

        public Email(string address) : this(address, null) {
        }

        public Email(string address, string displayName) {
            Address = address;
            DisplayName = displayName;
        }

        private string DebuggerDisplay => DisplayName.HasValue() ? $"{Address} ({DisplayName})" : Address;
    }
}
