using location_sharing_backend.Models.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace location_sharing_backend
{
	public class MailSender
	{
		private SmtpClient smtpClient;

		public MailSender()
		{
			smtpClient = new SmtpClient("smtp.gmail.com", 587);
			smtpClient.EnableSsl = true;
			smtpClient.Credentials = new NetworkCredential(Assets.Secrets.SmtpAuth.Email, Assets.Secrets.SmtpAuth.Password);
		}

		public bool SendLetter(string email, string subject, string body)
		{
			MailAddress maFrom = new MailAddress(Assets.Secrets.SmtpAuth.Email, Assets.Secrets.SmtpAuth.Email, Encoding.UTF8);
			MailAddress maTo = new MailAddress(email, email, Encoding.UTF8);
			MailMessage mmsg = new MailMessage(maFrom, maTo);
			mmsg.SubjectEncoding = Encoding.UTF8;
			mmsg.Subject = subject;
			mmsg.IsBodyHtml = true;
			mmsg.BodyEncoding = Encoding.UTF8;
			mmsg.Body = body;

			try
			{
				smtpClient.Send(mmsg);
				return true;
			}
			catch
			{
				return false;
			}
		}
	}
}
