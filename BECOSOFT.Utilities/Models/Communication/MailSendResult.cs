using System;

namespace BECOSOFT.Utilities.Models.Communication {
    public class MailSendResult {
        public bool Success { get; set; }
        public string Error { get; set; }
        public Exception Exception { get; set; }

        public bool HasError => Error != null || Exception != null;
    }
}
