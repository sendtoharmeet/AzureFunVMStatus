using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using SendGrid.Helpers.Mail;

namespace AzureFunVMStatus
{
    public static class SendEmail
    {
        [FunctionName("SendEmail")]
        public static void Run([QueueTrigger("email-queue", Connection = "CloudStorageAccount")]string myQueueItem,
                                [SendGrid(ApiKey = "CustomSendGridKeyAppSettingName")] out SendGridMessage sendGridMessage, ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");
            sendGridMessage = new SendGridMessage();
            try
            {
                string emailBody = myQueueItem.ToString();

                sendGridMessage.From = new EmailAddress("no-reply@myazureapp.com", "AzureFuncApps");
                sendGridMessage.AddTo("sendtoharmeet@yahoo.com");
                sendGridMessage.SetSubject("Azure VM is in Running status");
                sendGridMessage.AddContent("text/html", emailBody);
            }
            catch (Exception ex)
            {
                log.LogError($"Error occured while processing QueueItem {myQueueItem} , Exception - {ex.InnerException}");
            }
        }
    }
}
