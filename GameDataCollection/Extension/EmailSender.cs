﻿using System.Net.Mail;
using System.Net;

namespace GameDataCollection.Extension
{
    public class EmailSender
    {
        public static void EmailSend(string emailTo, string subject, string body)
        {
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            MailMessage mail = new MailMessage
            {
                From = new MailAddress("info@aliyagaming.com"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mail.To.Add(emailTo);

            SmtpClient smtp = new SmtpClient("mail.aliyagaming.com", 8889)
            {
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("info@aliyagaming.com", "fuckthisshit!123"), // Replace this
                EnableSsl = false,
                Port = 8889
            };

            try
            {
                smtp.Send(mail);
            }
            catch (SmtpFailedRecipientException ex)
            {
                // Handle recipient errors (like throttling)
                Console.WriteLine("SMTP Recipient Error: " + ex.Message);
                // You could log this instead
            }
            catch (SmtpException ex)
            {
                // General SMTP issues
                Console.WriteLine("SMTP Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                // Unexpected errors
                Console.WriteLine("General Email Error: " + ex.Message);
            }
        }
        //public static void EmailSend(string emailTo,string subject, string body)
        //{
        //    System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
        //    //create the mail message 
        //    MailMessage mail = new MailMessage();

        //    //set the addresses 
        //    mail.From = new MailAddress("info@aliyagaming.com"); //IMPORTANT: This must be same as your smtp authentication address.
        //    mail.To.Add(emailTo);

        //    //set the content 
        //    mail.Subject = subject;
        //    mail.Body = body;
        //    mail.IsBodyHtml = true;
        //    //send the message 
        //    SmtpClient smtp = new SmtpClient("mail.aliyagaming.com", 8889);

        //    //IMPORANT:  Your smtp login email MUST be same as your FROM address. 
        //    NetworkCredential Credentials = new NetworkCredential("info@aliyagaming.com", "fuckthisshit!123");
        //    smtp.UseDefaultCredentials = false;
        //    smtp.Credentials = Credentials;
        //    smtp.Port = 8889;    //alternative port number is 8889/25
        //    smtp.EnableSsl = false;
        //    smtp.Send(mail);
        //}
        public static string RegisterTemplate(string name)
        {
            return $"Congratulations {name}, You have successfully registered on Aliyah’s Platform. You are now eligible for Monthly bonus and Many more. Terms and Conditions applied. Free free to text us on our Facebook page or Messenger if any confusion. Hope you have a wonderful here ❤️";
        }
        public static string RegisterVerify(string name)
        {
            return $"“{name}” have been successfully registered on Aliyah Gaming.";
        }
    }
}
