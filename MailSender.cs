using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Api
{
	public class MailSender
	{
		public static bool SendLetter(string email, string subject, string body)
		{
			var message = new MimeMessage();
			var bodyBuilder = new BodyBuilder();

			message.From.Add(new MailboxAddress(Assets.Secrets.SmtpAuth.Email, Assets.Secrets.SmtpAuth.Email));
			message.To.Add(new MailboxAddress(email, email));
			//message.ReplyTo.Add(new MailboxAddress("reply_name", "reply_email@example.com"));

			message.Subject = subject;
			bodyBuilder.HtmlBody = body;
			message.Body = bodyBuilder.ToMessageBody();

			bool successful = true;
			using (var smtpClient = new SmtpClient())
			{
				smtpClient.ServerCertificateValidationCallback = (s, c, h, e) => true;
				smtpClient.Connect("smtp.gmail.com", 465, SecureSocketOptions.SslOnConnect);
				smtpClient.Authenticate(Assets.Secrets.SmtpAuth.Email, Assets.Secrets.SmtpAuth.Password);

				try
				{
					smtpClient.Send(message);
				}
				catch
				{
					successful = false;
				}

				smtpClient.Disconnect(true);
			}
			return successful;
		}
	}
}
