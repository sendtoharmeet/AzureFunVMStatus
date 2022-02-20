using System;
using System.Net;
using System.Net.Mail;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace AzureFunVMStatus
{
    public static class SendEmail
    {
        [FunctionName("SendEmail")]
        public static void Run([QueueTrigger("email-queue", Connection = "CloudStorageAccount")]string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");
            try
            {
                string emailBody = myQueueItem.ToString();
                Email(emailBody);
            }
            catch (Exception ex)
            {
                log.LogError($"Error occured while processing QueueItem {myQueueItem} , Exception - {ex.InnerException}");
            }
        }

        public static void Email(string htmlString)
        {
            try
            {
                var fromEmailAddress = "";
                var fromEmailPassword = "";
                var toEmailAddress = "";
                var host = "smtp.gmail.com";  // This is gmail host
                var port = 587; //Email sending port. Gmail is using 587 PORT
                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(fromEmailAddress);
                    mail.To.Add(toEmailAddress);
                    mail.Subject = "Azure VM Is Running";
                    mail.Body = htmlString;
                    mail.IsBodyHtml = true;
                    
                    using (SmtpClient smtp = new SmtpClient(host, port))
                    {
                        smtp.Credentials = new NetworkCredential(fromEmailAddress, fromEmailPassword);
                        smtp.EnableSsl = true;
                        smtp.UseDefaultCredentials = false;
                        smtp.Send(mail);
                    }
                }
            }
            catch (Exception ex) {
            
            }
        }
    }
}
