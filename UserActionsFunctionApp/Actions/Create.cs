namespace UserActionsFunctionApp.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;        

    public class Create
    {
        private readonly ILogger _logger;
        private readonly IUserStorage _userStorage;
        private readonly IClientFactory _clientFactory;
        private static IConfigurationRoot Configuration => BuildConfiguration.Build();

        public Create(ILogger logger, IUserStorage userStorage, IClientFactory clientFactory)
        {
            _logger = logger;
            _userStorage = userStorage;
            _clientFactory = clientFactory;
        }

        public async Task Execute(string appApiKey, string ownerAccessKey, string signingKey, string subscriptionId)
        {
            var userRecords = new List<UserRecord>();

            _logger.LogInformation("Initialize the dashboard client and create users in Users Database.");

            var client = _clientFactory.Create(Convert.ToBoolean(Configuration["Settings.Sandbox"]), appApiKey,
                signingKey, ownerAccessKey);

            var activeUsers = client.ListUsers(null, StatusFilter.Active);

            if (activeUsers.Count <= 0)
            {
                _logger.LogInformation($"No active or suspended users exist for ({subscriptionId}).");
            }
            else
            {
                var activeUsersInDb = _userStorage.GetUsersBySubscriptionId(subscriptionId).Result;

                var usersToCreate = activeUsers.Where(u =>
                        !activeUsersInDb.Any(x =>
                            x.UserId == u.user_id.ToString() && x.SubscriptionId.Equals(subscriptionId)))
                    .ToList();

                if (usersToCreate.Count > 0)
                {
                    foreach (var user in usersToCreate)
                    {
                        userRecords.Add(new UserRecord
                        {
                            UserId = user.user_id.ToString(),
                            Email = user.email,
                            Phone = user.cellphone
                        });
                    }

                    await _userStorage.AddUsers(userRecords, subscriptionId);
                }

                _logger.LogInformation($" {usersToCreate.Count} users part of ({subscriptionId}) are created.");
            }
        }
    }
}


