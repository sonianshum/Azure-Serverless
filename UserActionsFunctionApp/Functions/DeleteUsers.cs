namespace UserActionsFunctionApp.Functions
{
    using System;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Logging;    
    using System.Threading.Tasks;    

    public static class DeleteUsers
    {
        [FunctionName("DeleteUsers")]
        public static async Task Run([TimerTrigger("0 0 0 * * *", RunOnStartup = false, UseMonitor = true)]TimerInfo myTimer, 
            [Inject]IUserStorage userStorage,
            [Inject]IClientFactory clientFactory,
            ILogger logger)
        {
            try
            {
                logger.LogInformation($"DeleteUsers Timer trigger function start executing at: {DateTime.Now}");

                var action = new Actions.Delete(logger, userStorage, clientFactory);

                logger.LogInformation($"Start deleting users at : {DateTime.Now}");

                await action.Execute();

                logger.LogInformation($"DeleteUser Timer trigger function executed successfully at: {DateTime.Now}");
            }
            catch (Exception ex)
            {
                logger.LogError($"Failure with database update while deleting users {ex.Message}.");
            }
        }
    }
}
