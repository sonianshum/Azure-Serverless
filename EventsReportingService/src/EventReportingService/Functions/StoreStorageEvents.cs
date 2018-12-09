namespace EventReportingService.Functions
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.WindowsAzure.Storage.Table;

    using EventReportingService.Bindings;
    using EventReportingService.Models;
    using EventReportingService.Processors;
    using EventReportingService.Storage;

    public static class StoreStorageEvents
    {
        private const string AppSettingContainingConnectionString = "TableStorageConnectionString";

        [FunctionName("StoreStorageEvents")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] GridEvent<StorageEvent>[] gridEvents,
            [Table(nameof(Collections.AccountsV1), Connection = AppSettingContainingConnectionString)] CloudTable accountsV1,
            [Table(nameof(Collections.UsersV1), Connection = AppSettingContainingConnectionString)] CloudTable UsersV1,
            [Table(nameof(Collections.ContactsV1), Connection = AppSettingContainingConnectionString)] CloudTable contactsV1,
            [Inject(typeof(IStorageFactory<StorageEventTableEntity>))] IStorageFactory<StorageEventTableEntity> storageFactory,
            ILogger logger)
        {
            var storageTables = new Dictionary<string, IStorage<StorageEventTableEntity>>
            {
                { Collections.AccountsV1, storageFactory.Create(accountsV1) },
                { Collections.UsersV1, storageFactory.Create(UsersV1) },
                { Collections.ContactsV1, storageFactory.Create(contactsV1) },
            };

            var eventProcessor = new StorageEventProcessor(gridEvents, storageTables, logger);

            return await ProcessorRunner.Run(eventProcessor);
        }
    }
}