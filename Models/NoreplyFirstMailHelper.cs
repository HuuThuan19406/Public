using Api.Entities;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Api.Models
{
    internal class NoreplyFirstMailHelper : MailHelper, IAccountAutoMail
    {
        public NoreplyFirstMailHelper() : base
            (
                new MailAddress("noreply-user@bestsv.net", "BestSV", Encoding.UTF8),
                new NetworkCredential("noreply-user@bestsv.net", "5OF*^L6ds2Z2"),
                new SmtpClient("sv3.tmail.vn", 587)
            )
        {
        }

        public bool NotifyAccountGenerated(Account account)
        {
            throw new System.NotImplementedException();
        }

        public bool SendPin(Identification identification)
        {
            MailAddress destinationMail = new MailAddress(identification.IdentificationId);
            string subject = "[Bestsv] Mã Xác Thực Tài Khoản";
            string content = $"Mã của bạn là <b>{identification.Pin}</b>. Mã có hiệu lực đến {identification.Expired.AddHours(7).ToString("HH:mm:ss dd/MM/yy")}";

            try
            {
                Send(destinationMail, subject, content);
            }
            catch(SmtpFailedRecipientException)
            {
                return false;
            }

            return true;
        }
    }
}