namespace EventReportingService.Metrics
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Linq;
    using EventReportingService.Models;
    using EventReportingService.Storage;

    public abstract class MetricBase : IMetric
    {
        private readonly IStorage<MetricTableEntity> metricStorage;

        protected MetricBase(GridEvent<Event<JToken>> gridEvent, string product, string name, string instance, long count, IStorage<MetricTableEntity> metricStorage, ILogger logger)
        {
            GridEvent = gridEvent;
            Product = product;
            Name = name;
            Instance = instance ?? string.Empty;
            Count = count;
            this.metricStorage = metricStorage;
            Logger = logger;
        }

        protected GridEvent<Event<JToken>> GridEvent { get; }

        protected string Product { get; }

        protected string Name { get; }

        protected string Instance { get; }

        protected long Count { get; }

        protected ILogger Logger { get; }

        public async Task Update()
        {
            var metric = Get(GridEvent.Data.SubscriptionId, Name, Instance);

            if (metric == null)
            {
                Logger.LogInformation("Creating Metric {Metric} {Instance} with count of {NewValue}", Name, Instance, Count);

                await Create();
                return;
            }

            var oldValue = metric.Count;

            var newValue = UpdateCount(metric);

            if (newValue == null)
            {
                Logger.LogInformation("No update required for Metric {Metric} {Instance}", Name, Instance);
                return;
            }

            metric.Count = newValue.Value;

            metric.Updated = GridEvent.EventTime;

            Logger.LogInformation("Updating Metric {Metric} {Instance} from {OldValue} to {NewValue}", Name, Instance, oldValue, metric.Count);

            await metricStorage.Replace(metric);
        }

        protected abstract long? UpdateCount(MetricTableEntity tableEntity);

        private async Task Create()
        {
            var tableEntity = new MetricTableEntity(
                GridEvent.EventTime,
                GridEvent.EventTime,
                Product,
                Name,
                GridEvent.Data.OrganizationId,
                GridEvent.Data.SubscriptionId,
                Instance,
                Count);

            await metricStorage.Insert(tableEntity);
        }

        private MetricTableEntity Get(Guid subscriptionId, string name, string instance)
        {
            return metricStorage.FirstOrDefault(m =>
                m.PartitionKey == subscriptionId.ToString()
                && m.Name == name
                && m.Instance == instance);
        }
    }
}
