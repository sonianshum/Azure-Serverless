namespace UserActionsFunctionApp.Functions
{
    using System;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Logging;    
    using System.Threading.Tasks;
    using UserActionsFunctionApp.Actions;

    public static class SyncronizeUsers
    {
        [FunctionName("SyncronizeUsers")]
        public static async Task Run([TimerTrigger("0 0 0/22 * * *", RunOnStartup = false, UseMonitor = true)]TimerInfo myTimer,
            [Inject]IUserStorage userStorage,
            [Inject]IClientFactory clientFactory,
            ILogger logger)
        {
            try
            {
                logger.LogInformation($"Start user sync at : {DateTime.Now}");

                var action = new Syncronize(logger, userStorage, clientFactory);

                await action.Execute();

                logger.LogInformation($"User sync timer function executed successfully at: {DateTime.Now}");
            }
            catch (Exception ex)
            {
                logger.LogError($"Failure with database update while syncronizing users {ex.Message}.");
            }
        }
    }
}
