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

    public class Delete
    {
        private readonly ILogger _logger;
        private readonly IUserStorage _userStorage;
        private readonly IClientFactory _clientFactory;
        private static IConfigurationRoot Configuration => BuildConfiguration.Build();

        public Delete(ILogger logger, IUserStorage userStorage, IClientFactory clientFactory)
        {
            _logger = logger;
            _userStorage = userStorage;
            _clientFactory = clientFactory;
        }

        public async Task Execute()
        {
            var totalDeletedUsers = 0;

            _logger.LogInformation("Get all users marked for deletion in database.");
            var purgedUserData = _userStorage.GetPurgedUsers().Result;

            _logger.LogInformation($"Total {purgedUserData.Count} user records are marked for deletion.");
            var purgedUsers = purgedUserData.GroupBy(x => new { x.SubscriptionId, x.AppApiKey, x.OwnerAccessKey, x.SigningKey });

            _logger.LogInformation("Initialize the dashboard client and, retrieve all the purged users.");

            foreach (var usersToDelete in from user in purgedUsers
                                          let authyClient = _clientFactory.Create(Convert.ToBoolean(Configuration["Settings.Sandbox"]), user.Key.AppApiKey, user.Key.SigningKey, user.Key.OwnerAccessKey)
                                          let authyPurgedUsers = authyClient.ListUsers(null, StatusFilter.Deleted)
                                          select user.Where(x => !authyPurgedUsers.Select(y => y.authy_id.ToString()).Contains(x.AuthyId)).ToList() into usersToDelete
                                          where usersToDelete.Count > 0
                                          select usersToDelete)
            {
                await _userStorage.DeleteUsers(usersToDelete);
                totalDeletedUsers += usersToDelete.Count;

                _logger.LogInformation($" {usersToDelete.Count} users part of ({usersToDelete.First().SubscriptionId}) marked for clean up.");
            }

            _logger.LogInformation($"{totalDeletedUsers} users deleted sucessfully from the database.");
        }
    }
}
