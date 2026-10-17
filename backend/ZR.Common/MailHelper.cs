using Infrastructure;
using Infrastructure.Model;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ZR.Common
{
    public class MailHelper
    {
        /// <summary>
        /// Sender email
        /// </summary>
        private readonly MailOptions mailOptions = new();

        public MailHelper()
        {
            List<MailOptions> options = new();

            AppSettings.Bind("MailOptions", options);
            mailOptions = options.First();
        }
        public MailHelper(MailOptions _mailOptions)
        {
            mailOptions = _mailOptions;
        }
        /// <summary>
        /// Send one email
        /// </summary>
        /// <param name="toAddress"></param>
        /// <param name="subject"></param>
        /// <param name="text"></param>
        /// <param name="path"></param>
        public string SendMail(string toAddress, string subject, string text, string path = "", string html = "")
        {
            IEnumerable<MailboxAddress> mailboxes = new List<MailboxAddress>() {
                new MailboxAddress(toAddress, toAddress)
            };

            return SendMail(mailboxes, subject, text, path, html);
        }

        /// <summary>
        /// Send multiple emails
        /// </summary>
        /// <param name="toAddress"></param>
        /// <param name="subject"></param>
        /// <param name="text"></param>
        /// <param name="path"></param>
        public string SendMail(string[] toAddress, string subject, string text, string path = "", string html = "")
        {
            IList<MailboxAddress> mailboxes = new List<MailboxAddress>() { };
            foreach (var item in toAddress)
            {
                mailboxes.Add(new MailboxAddress(item, item));
            }

            return SendMail(mailboxes, subject, text, path, html);
        }

        /// <summary>
        /// Send email
        /// </summary>
        /// <param name="toAddress"></param>
        /// <param name="subject">Subject</param>
        /// <param name="text"></param>
        /// <param name="path">Attachment url address</param>
        /// <param name="html">Web HTML content</param>
        private string SendMail(IEnumerable<MailboxAddress> toAddress, string subject, string text, string path = "", string html = "")
        {
            MimeMessage message = new MimeMessage();
            //Sender
            message.From.Add(new MailboxAddress(mailOptions.FromName, mailOptions.FromEmail));
            //Recipient
            message.To.AddRange(toAddress);
            message.Subject = subject;
            message.Date = DateTime.Now;

            //Create attachment Multipart
            Multipart multipart = new Multipart("mixed");
            var alternative = new MultipartAlternative();

            // IMPORTANT: For multipart/alternative, parts must be ordered from
            // lowest to highest fidelity. Email clients display the LAST part they can render.
            // Order: Plain text FIRST, then HTML LAST

            // 1. Plain text content (lowest fidelity - fallback)
            var plain = new TextPart(TextFormat.Plain)
            {
                Text = text + "\r\n\n\n" + mailOptions.Signature
            };
            alternative.Add(plain);

            // 2. HTML content (highest fidelity - preferred)
            if (!string.IsNullOrEmpty(html))
            {
                var Html = new TextPart(TextFormat.Html)
                {
                    Text = html
                };
                alternative.Add(Html);
            }

            //??
            if (!string.IsNullOrEmpty(path))
            {
                string[] files = path.Split(",");
                foreach (var file in files)
                {
                    MimePart attachment = new()
                    {
                        Content = new MimeContent(File.OpenRead(file), ContentEncoding.Default),
                        //????,???????
                        ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                        ContentTransferEncoding = ContentEncoding.Base64,
                        //????
                        FileName = Path.GetFileName(path)
                    };
                    alternative.Add(attachment);
                }
            }
            multipart.Add(alternative);
            //??????
            message.Body = multipart;

            //Connect to SMTP server
            using var client = new SmtpClient();
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;
            //client.CheckCertificateRevocation = false;

            // Use proper SSL options based on port
            // Port 465 = SSL on connect, Port 587 = STARTTLS
            MailKit.Security.SecureSocketOptions sslOptions;
            if (mailOptions.Port == 465)
            {
                sslOptions = MailKit.Security.SecureSocketOptions.SslOnConnect;
            }
            else if (mailOptions.UseSsl)
            {
                sslOptions = MailKit.Security.SecureSocketOptions.StartTls;
            }
            else
            {
                sslOptions = MailKit.Security.SecureSocketOptions.None;
            }

            client.Connect(mailOptions.Smtp, mailOptions.Port, sslOptions);

            //??,??
            //????,???????????,Exception??????,?????
            client.Authenticate(System.Text.Encoding.UTF8, mailOptions.FromEmail, mailOptions.Password);

            try
            {
                var result = client.Send(message);
                //??
                client.Disconnect(true);
                Console.WriteLine($"[{DateTime.Now}] Email sending result: {result}");
                return result;
            }
            catch (Exception ex)
            {
                client.Disconnect(true);
                Log.WriteLine(ConsoleColor.Red, "Email sending failed: " + ex.Message);
                return "fail";
            }
        }
    }
}
