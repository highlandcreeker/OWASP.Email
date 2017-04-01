using System;
using System.Net.Mail;

namespace OWASP.Email
{
	public static class Communications
	{
		public static void SendMessage(string recipient, string body)
		{
			var msg = new MailMessage();

			msg.Body = body;
			msg.IsBodyHtml = true;

			using (var client = new SmtpClient("10.10.211.22", 25))
			{
				msg.Sender = new MailAddress("owasp@blackbaud.com");
				msg.From = new MailAddress("owasp@blackbaud.com");
				msg.ReplyToList.Add(new MailAddress("john.young@blackbaud.com"));
				msg.Subject = "OWASP Secure Coding Quiz";

				string recipientValue = System.Configuration.ConfigurationManager.AppSettings.Get("Bcc");

				if (recipientValue.Length > 0)
				{
					var recipients = recipientValue.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

					foreach (var r in recipients)
					{
						try
						{
							msg.Bcc.Add(new MailAddress(recipient));
						}
						catch (FormatException)
						{
							throw new ArgumentException("Invalid email address format for the recipient. Please check the app config file (key=Bcc) for improperly formatted recipients.");
						}
					}
				}

				client.DeliveryFormat = SmtpDeliveryFormat.SevenBit;
				client.DeliveryMethod = SmtpDeliveryMethod.Network;

#if DEBUG
				//using (var f = System.IO.File.CreateText(@"D:\Temp\owasp\" + recipient + ".html"))
				//{
				//	f.Write(body.ToCharArray());
				//}
				msg.To.Add(new MailAddress("scott.carnley@blackbaud.com"));
				client.Send(msg);
#else
			msg.To.Add(new MailAddress(recipient));
			client.Send(msg);
#endif
			}
		}
	}
}
