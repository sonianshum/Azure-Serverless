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

    public static class UpdateMetrics
    {
        private const string AppSettingContainingConnectionString = "TableStorageConnectionString";

        [FunctionName("UpdateMetrics")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] GridEvent<Event<JToken>>[] gridEvents,
            [Table(nameof(Collections.Metrics), Connection = AppSettingContainingConnectionString)] CloudTable metricsTable,
            [Inject(typeof(IStorageFactory<MetricTableEntity>))] IStorageFactory<MetricTableEntity> storageFactory,
            ILogger logger)
        {
            var eventProcessor = new MetricsProcessor(gridEvents, storageFactory.Create(metricsTable), logger);

            return await ProcessorRunner.Run(eventProcessor);
        }
    }
}