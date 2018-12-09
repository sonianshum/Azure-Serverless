namespace UserActionsFunctionApp.Actions
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;    
    using Microsoft.Azure.ServiceBus;
    using UserActionsFunctionApp.DependencyInjection;

    public class Syncronize
    {
        private readonly ILogger _logger;
        private readonly IUserStorage _userStorage;
        private readonly IClientFactory _clientFactory;
        private IQueueClient _queueClient;
        private static IConfigurationRoot Configuration => BuildConfiguration.Build();

        public Syncronize(ILogger logger, IUserStorage userStorage, IClientFactory clientFactory)
        {
            _logger = logger;
            _userStorage = userStorage;
            _clientFactory = clientFactory;
        }

        public async Task Execute()
        {
            _logger.LogInformation($"Connect queue client.{Configuration["ServiceBus:ConnectionString"]} and bus name {Configuration["ServiceBus:Name"]} ");

            _queueClient = new QueueClient(Configuration["ServiceBus:ConnectionString"], Configuration["ServiceBus:Name"]);

            _logger.LogInformation("Get all active users from database.");
            var activeUserData = _userStorage.GetUsers().Result;

            _logger.LogInformation($"Total {activeUserData.Count} active users exist in database.");
            var activeSubscriptionGroups = activeUserData.GroupBy(x => new { x.SubscriptionId, x.AppApiKey, x.OwnerAccessKey, x.SigningKey });

            _logger.LogInformation("Initialize the dashboard client and start user sync from Users Database.");

            foreach (var group in activeSubscriptionGroups)
            {                
                await SendMessagesAsync(group.Key.AppApiKey, group.Key.SigningKey, group.Key.OwnerAccessKey, group.Key.SubscriptionId);
            }

            _logger.LogInformation($"Close queue client at {DateTime.Now}");

            await _queueClient.CloseAsync();

            _logger.LogInformation("Users created sucessfully in the database.");
        }

        public async Task SendMessagesAsync(string appApiKey, string signingKey, string ownerAccessKey, string subscriptionId)
        {
            try
            { 
                var messageBody = $" {appApiKey} , {ownerAccessKey} , {signingKey} , {subscriptionId} ";
                
                var message = new Message(Encoding.UTF8.GetBytes(messageBody));
             
                _logger.LogInformation($"Sending message: {messageBody}");
                
                await _queueClient.SendAsync(message);
            }
            catch (Exception exception)
            {
                _logger.LogInformation($"{DateTime.Now} :: Exception: {exception.Message}");
            }
        }
    }
}
