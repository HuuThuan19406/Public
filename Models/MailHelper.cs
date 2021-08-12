using System.Net;
using System.Net.Mail;
using System.Text;

namespace Api.Models
{
    internal class MailHelper
    {
        protected readonly MailAddress senderMail;
        protected readonly NetworkCredential credentials;
        protected MailMessage message;
        protected SmtpClient smtpClient;

        public MailHelper(MailAddress senderMail, NetworkCredential credentials, SmtpClient smtpClient)
        {
            this.senderMail = senderMail;
            this.credentials = credentials;
            this.smtpClient = smtpClient;

            message = new MailMessage();
            message.SubjectEncoding = Encoding.UTF8;
            message.IsBodyHtml = true;
            message.BodyEncoding = Encoding.UTF8;
        }

        public void Send(MailAddress receiverMail, string subject, string content)
        {
            try
            {
                using (var client = smtpClient)
                {
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.UseDefaultCredentials = false;
                    client.Credentials = credentials;
                    client.EnableSsl = true;

                    message.From = senderMail;
                    message.To.Clear();
                    message.To.Add(receiverMail);
                    message.Subject = subject;
                    message.Body = content;

                    client.Send(message);
                }
            }
            catch { throw; }
        }
    }
}