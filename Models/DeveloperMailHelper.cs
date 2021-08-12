using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Api.Models
{
    internal class DeveloperMailHelper : MailHelper
    {
        internal DeveloperMailHelper() : base
            (
                new MailAddress("dev@bestsv.net", "Bestsv - Kỹ Thuật", Encoding.UTF8),
                new NetworkCredential("dev@bestsv.net", "NgR^nGg0@vL^"),
                new SmtpClient("sv3.tmail.vn", 587)
            )
        {
        }

        internal bool SendInformationInternalEmail(string sendTo, Email email, string lastName)
        {
            MailAddress destinationMail = new MailAddress(sendTo);
            var pairs = new Dictionary<string, string>
            {
                { "{LastName}", lastName},
                { "{Email}", email.Address},
                { "{Password}", email.Password},
                {"{CurrentYear}", DateTime.Now.Year.ToString() }
            };

            string subject = "[BestSV] Cấp Email Nội Bộ";

            StreamReader sr = new StreamReader("EmailTemplates/Cấp phát tài khoản nội bộ.html");           
            string content = sr.ReadToEnd().HtmlReplace(pairs);
            sr.Close();

            try
            {
                Send(destinationMail, subject, content);
            }
            catch (SmtpFailedRecipientException)
            {
                return false;
            }

            return true;
        }
    }
}
