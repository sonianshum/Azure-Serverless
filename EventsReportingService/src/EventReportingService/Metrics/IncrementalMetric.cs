namespace EventReportingService.Metrics
{
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Linq;
    using EventReportingService.Models;
    using EventReportingService.Storage;

    public class IncrementalMetric : MetricBase
    {
        public IncrementalMetric(GridEvent<Event<JToken>> gridEvent, string product, string name, string instance, long count, IStorage<MetricTableEntity> metricStorage, ILogger logger)
            : base(gridEvent, product, name, instance, count, metricStorage, logger)
        {
        }

        protected override long? UpdateCount(MetricTableEntity tableEntity)
        {
            return tableEntity.Count += Count;
        }
    }
}
