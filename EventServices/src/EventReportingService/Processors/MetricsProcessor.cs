namespace EventReportingService.Processors
{
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Linq;
    using EventReportingService.Metrics;
    using EventReportingService.Models;
    using EventReportingService.Storage;

    public class MetricsProcessor : BaseEventProcessor<Event<JToken>>
    {
        private readonly MetricFactory metricFactory;

        public MetricsProcessor(GridEvent<Event<JToken>>[] gridEvents, IStorage<MetricTableEntity> metricStorage, ILogger logger)
            : base(gridEvents, logger)
        {
            metricFactory = new MetricFactory(metricStorage, logger);
        }

        protected override async Task ProcessEvent(GridEvent<Event<JToken>> gridEvent)
        {
            var metric = metricFactory.GetMetricForEvent(gridEvent);

            if (metric == null)
            {
                Logger.LogWarning("Ignoring unsupported event: Subject={Subject}, EventType={EventType}", gridEvent.Subject, gridEvent.EventType);
                return;
            }

            await metric.Update();
        }
    }
}
