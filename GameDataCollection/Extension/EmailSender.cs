using System.Net.Mail;
using System.Net;
using System.Text;

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
        public static string AccountCreatedTemplate(string name, string email, string password, string? loginUrl)
        {
            return $@"<p>{WebUtility.HtmlEncode(RegisterTemplate(name))}</p>
<p>We have created an account for you. Use these details to log in:</p>
<p><strong>Username:</strong> {WebUtility.HtmlEncode(email)}<br/>
<strong>Password:</strong> {WebUtility.HtmlEncode(password)}</p>
<p><a href='{loginUrl}'>Log in here</a></p>
<p>Please keep this password safe and do not share it with anyone. You can change it any time from <strong>Change Password</strong> after logging in.</p>";
        }
        public static string AdminMessageTemplate(string name, string? message, string? imageUrl, string? chatUrl)
        {
            var body = new StringBuilder();
            body.Append($"<p>Hi {WebUtility.HtmlEncode(name)},</p>");
            body.Append("<p>You have a new message from Aliyah Gaming:</p>");

            if (!string.IsNullOrWhiteSpace(message))
            {
                body.Append("<blockquote style='margin:0 0 1em;padding:10px 15px;border-left:4px solid #007bff;background:#f6f8fa;'>");
                body.Append(WebUtility.HtmlEncode(message).Replace("\n", "<br/>"));
                body.Append("</blockquote>");
            }

            if (!string.IsNullOrWhiteSpace(imageUrl))
                body.Append($"<p><a href='{WebUtility.HtmlEncode(imageUrl)}'>View the image</a></p>");

            if (!string.IsNullOrWhiteSpace(chatUrl))
                body.Append($"<p><a href='{WebUtility.HtmlEncode(chatUrl)}'>Reply in your messages</a></p>");

            body.Append("<p>Please do not reply to this email — we will not see it. Use the messages page instead.</p>");
            return body.ToString();
        }
        public static string PasswordResetByAdminTemplate(string name, string email, string password, string? loginUrl)
        {
            return $@"<p>Hi {WebUtility.HtmlEncode(name)},</p>
<p>Your Aliyah Gaming password has been reset. Use these details to log in:</p>
<p><strong>Username:</strong> {WebUtility.HtmlEncode(email)}<br/>
<strong>Password:</strong> {WebUtility.HtmlEncode(password)}</p>
<p><a href='{loginUrl}'>Log in here</a></p>
<p>Please keep this password safe. You can change it any time from <strong>Change Password</strong> after logging in.</p>";
        }
        public static string PasswordResetTemplate(string name, string? resetUrl)
        {
            return $@"<p>Hi {WebUtility.HtmlEncode(name)},</p>
<p>We received a request to reset your Aliyah Gaming password. Click the link below to choose a new password:</p>
<p><a href='{WebUtility.HtmlEncode(resetUrl)}'>Reset my password</a></p>
<p>This link expires in 24 hours. If you didn't ask to reset your password, you can ignore this email.</p>";
        }
        public static string RegisterVerify(string name)
        {
            return $"“{name}” have been successfully registered on Aliyah Gaming.";
        }
    }
}
