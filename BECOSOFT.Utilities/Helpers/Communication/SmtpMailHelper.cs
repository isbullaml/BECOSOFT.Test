using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Models.Communication;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace BECOSOFT.Utilities.Helpers.Communication {
    public static class SmtpMailHelper {
        private const int DefaultPort = 25;
        private const int DefaultTimeout = 240000;

        public static MailSendResult Send(MailboxSettingsWrapper settings, MailMessage message) {
            using (var client = GetPreparedClient(settings)) {
                var sendResult = new MailSendResult();
                try {
                    client.Send(message);
                    sendResult.Success = true;
                } catch (Exception ex) {
                    sendResult.Exception = ex;
                    sendResult.Success = false;
                }
                return sendResult;
            }
        }

        public static async Task<MailSendResult> SendAsync(MailboxSettingsWrapper settings, MailMessage message) {
            using (var client = GetPreparedClient(settings)) {
                var sendResult = new MailSendResult();
                try {
                    await client.SendMailAsync(message);
                    sendResult.Success = true;
                } catch (Exception ex) {
                    sendResult.Exception = ex;
                    sendResult.Success = false;
                }
                return sendResult;
            }
        }

        private static SmtpClient GetPreparedClient(MailboxSettingsWrapper settings) {
            var client = new SmtpClient {
                Host = settings.EndPoint,
                Port = settings.Port == 0 ? DefaultPort : settings.Port,
                EnableSsl = settings.EnableSsl,
                Timeout = DefaultTimeout
            };
            if (!settings.Username.IsNullOrWhiteSpace()) {
                client.Credentials = new NetworkCredential(settings.Username, settings.Password);
            }

            return client;
        }
    }
}