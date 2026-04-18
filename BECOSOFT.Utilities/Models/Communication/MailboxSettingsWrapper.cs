namespace BECOSOFT.Utilities.Models.Communication {
    public class MailboxSettingsWrapper {
        public string Username { get; set; }
        public string Password { get; set; }
        public string EndPoint { get; set; }
        public int Port { get; set; }
        public bool EnableSsl { get; set; }
    }
}