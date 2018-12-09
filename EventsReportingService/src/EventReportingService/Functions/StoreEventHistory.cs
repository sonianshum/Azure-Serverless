namespace EventReportingService.Functions
{
    using System.Net.Http;
    using System.Threading.Tasks;

    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.WindowsAzure.Storage.Table;

    using Newtonsoft.Json.Linq;

    using EventReportingService.Bindings;
    using EventReportingService.Models;
    using EventReportingService.Processors;
    using EventReportingService.Storage;

    public static class StoreEventHistory
    {
        private const string AppSettingContainingConnectionString = "TableStorageConnectionString";

        [FunctionName("StoreEventHistory")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] GridEvent<JObject>[] gridEvents,
            [Table(nameof(Collections.Events), Connection = AppSettingContainingConnectionString)] CloudTable eventsTable,
            [Inject(typeof(IStorageFactory<EventHistoryTableEntity>))] IStorageFactory<EventHistoryTableEntity> storageFactory,
            ILogger logger)
        {
            var eventProcessor = new EventHistoryProcessor(gridEvents, storageFactory.Create(eventsTable), logger);

            return await ProcessorRunner.Run(eventProcessor);
        }
    }
}