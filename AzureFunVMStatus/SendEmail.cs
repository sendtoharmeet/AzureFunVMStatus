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
                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress("@gmail.com");
                    mail.To.Add("@yahoo.com");
                    mail.Subject = "<<Meeagse>> VM Is Running";
                    mail.Body = htmlString;
                    mail.IsBodyHtml = true;
                    
                    using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                    {
                        smtp.Credentials = new NetworkCredential("", "");
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
