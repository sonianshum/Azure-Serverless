namespace UserActionsFunctionApp.Functions
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Logging;   

    public static class CreateUsers
    {
        [FunctionName("CreateUsers")]
        public static async Task Run([ServiceBusTrigger("usersqueue", Connection = "ServiceBusConnectionString")]string queueItem,
            [Inject]IUserStorage userStorage,
            [Inject]IClientFactory clientFactory,
            ILogger logger)
        {
            try
            {
                logger.LogInformation($"Start creating users at : {DateTime.Now} {queueItem}");
                
                if (queueItem != null)
                {
                    var items = queueItem.Split(',');

                    var action = new Actions.Create(logger, userStorage, clientFactory);

                    await action.Execute(items[0], items[1], items[2], items[3]);
                }

                logger.LogInformation($"Users created successfully at: {DateTime.Now}");
            }
            catch (Exception ex)
            {
                logger.LogError($"Failure with database update while creating users {ex.Message}.");
            }
        }
    }
}
