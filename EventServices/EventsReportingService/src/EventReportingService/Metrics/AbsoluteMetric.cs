namespace EventReportingService.Metrics
{
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Linq;
    using EventReportingService.Models;
    using EventReportingService.Storage;

    public class AbsoluteMetric : MetricBase
    {
        public AbsoluteMetric(GridEvent<Event<JToken>> gridEvent, string product, string name, string instance, long count, IStorage<MetricTableEntity> metricStorage, ILogger logger)
            : base(gridEvent, product, name, instance, count, metricStorage, logger)
        {
        }

        protected override long? UpdateCount(MetricTableEntity tableEntity)
        {
            // Ignore old events.
            if (GridEvent.EventTime < tableEntity.Updated)
            {
                Logger.LogWarning("Skipping superseded update for Metric {Metric} {Instance}.", Name, Instance);
                return null;
            }

            // Ignore events with the same count.
            if (tableEntity.Count == Count)
            {
                Logger.LogInformation("Skipping update for Metric {Metric} {Instance}. Metric already has the correct count.", Name, Instance);
                return null;
            }

            return Count;
        }
    }
}
