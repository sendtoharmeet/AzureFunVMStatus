using System;
using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace AzureFunVMStatus
{
    /*
     https://docs.lacework.com/gather-the-required-azure-client-id-tenant-id-and-client-secret
     https://docs.microsoft.com/en-us/azure/storage/common/storage-account-keys-manage?tabs=azure-portal
    */
    
    public static class VirtualMachineStatus
    {
        static AzureCredentials MySecrate()
        {
            var clientId = "";
            var clientSecret = "";
            var tenantId = "";

            return SdkContext.AzureCredentialsFactory.FromServicePrincipal(clientId, clientSecret, tenantId, AzureEnvironment.AzureGlobalCloud);
        }

        [FunctionName("VirtualMachineStatus")]
        public async static void Run([TimerTrigger("0 0 * * * *", RunOnStartup = true)] TimerInfo myTimer, ILogger log, ExecutionContext context)
        {
            var azure = Microsoft.Azure.Management.Fluent.Azure
                        .Configure()
                        .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                        .Authenticate(MySecrate()).WithSubscription("<<SUBSCRIPTION ID>>");

            var resourcegroup = "";
            CreateQueueIfNotExists(log, context);

            foreach (var virtualMachine in await azure.VirtualMachines.ListByResourceGroupAsync(resourcegroup))
            {
                log.LogInformation($"Current VM {virtualMachine.Name}...");

                if(virtualMachine.PowerState == PowerState.Running)
                {
                    string randomStr = Guid.NewGuid().ToString();
                    var emailContent = $"<html><body><h2> Hi, Your Vm is runnig: " + virtualMachine.Name + " </h2></body></html>";

                    CloudStorageAccount storageAccount = GetCloudStorageAccount(context);
                    CloudQueueClient cloudQueueClient = storageAccount.CreateCloudQueueClient();
                    CloudQueue cloudQueue = cloudQueueClient.GetQueueReference("email-queue");
                    var cloudQueueMessage = new CloudQueueMessage(emailContent);
                    await cloudQueue.AddMessageAsync(cloudQueueMessage);
                }
            }

            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }

        private static void CreateQueueIfNotExists(ILogger logger, ExecutionContext executionContext)
        {
            CloudStorageAccount storageAccount = GetCloudStorageAccount(executionContext);
            CloudQueueClient cloudQueueClient = storageAccount.CreateCloudQueueClient();
            string[] queues = new string[] { "email-queue" };
            foreach (var item in queues)
            {
                CloudQueue cloudQueue = cloudQueueClient.GetQueueReference(item);
                cloudQueue.CreateIfNotExistsAsync();
            }
        }
        private static CloudStorageAccount GetCloudStorageAccount(ExecutionContext executionContext)
        {
            var config = new ConfigurationBuilder().SetBasePath(executionContext.FunctionAppDirectory)
                                                   .AddJsonFile("local.settings.json", true, true)
                                                   .AddEnvironmentVariables()
                                                   .Build();
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(config["CloudStorageAccount"]);
            return storageAccount;
        }
    }
}
