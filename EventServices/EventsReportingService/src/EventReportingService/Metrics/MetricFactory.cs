namespace EventReportingService.Metrics
{
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Linq;
    using EventReportingService.Models;
    using EventReportingService.Storage;

    public class MetricFactory
    {
        private const string ProductName = "productName";

        private const string ConnectorId = "connectorId";

        private readonly IStorage<MetricTableEntity> metricStorage;

        private readonly ILogger logger;

        public MetricFactory(IStorage<MetricTableEntity> metricStorage, ILogger logger)
        {
            this.metricStorage = metricStorage;
            this.logger = logger;
        }

        public IMetric GetMetricForEvent(GridEvent<Event<JToken>> gridEvent)
        {
            switch (gridEvent.Subject)
            {
                case Services.faClientProxy:
                    return Get2faMetric(gridEvent);

                case Services.ISupervisor:
                    return GetIariMetric(gridEvent);

                case Services.CSupervisor:
                    return GetConnectMetric(gridEvent);

                default:
                    return null;
            }
        }

        private static JToken GetEventDataProperty(GridEvent<Event<JToken>> gridEvent, string propertyName)
        {
            return gridEvent.Data.EventData?.SelectToken(propertyName);
        }

        private IMetric Get2faMetric(GridEvent<Event<JToken>> gridEvent)
        {
            switch (gridEvent.EventType)
            {
                case Events.UserCreated:
                    return Create2faIncrementalMetric(Metrics.TotalUsers, gridEvent);

                case Events.UserDeleted:
                    return Create2faIncrementalMetric(Metrics.TotalUsers, gridEvent, decrement: true);

                case Events.OtpVerification:
                case Events.PushNotificationSent:
                    return Create2faIncrementalMetric(Metrics.Total2fas, gridEvent);

                default:
                    return null;
            }
        }

        private IMetric GetIariMetric(GridEvent<Event<JToken>> gridEvent)
        {
            switch (gridEvent.EventType)
            {
                case Events.CollectorModuleDataCollected:
                case Events.DataSourceDisabled:
                case Events.DataSourceEnabled:
                case Events.DataSourceDeleted:
                    return CreateManagedAccountsMetric(gridEvent);

                default:
                    return null;
            }
        }

        private IMetric GetConnectMetric(GridEvent<Event<JToken>> gridEvent)
        {
            switch (gridEvent.EventType)
            {
                case Events.ObjectCreated:
                case Events.ObjectDeleted:
                case Events.ObjectModified:
                    return CreateConnectIncrementalMetric(Metrics.ObjectsSynchronized, gridEvent, ConnectorId);

                case Events.ConnectorProvisioned:
                    return CreateConnectIncrementalMetric(Metrics.ConnectorsProvisioned, gridEvent);

                case Events.ConnectorDeleted:
                    return CreateConnectIncrementalMetric(Metrics.ConnectorsProvisioned, gridEvent, decrement: true);

                default:
                    return null;
            }
        }

        private IMetric Create2faIncrementalMetric(string name, GridEvent<Event<JToken>> gridEvent, bool decrement = false)
        {
            return CreateIncrementalMetric(name, gridEvent, Products.Tfa, ProductName, decrement);
        }

        private IMetric CreateConnectIncrementalMetric(string name, GridEvent<Event<JToken>> gridEvent, string instanceId = "", bool decrement = false)
        {
            return CreateIncrementalMetric(name, gridEvent, Products.Connect, instanceId, decrement);
        }

        private IMetric CreateManagedAccountsMetric(GridEvent<Event<JToken>> gridEvent)
        {
            var dataSourceId = (string)GetEventDataProperty(gridEvent, "dataSourceId");
            var managedAccounts = (long)GetEventDataProperty(gridEvent, "managedAccounts");

            return new AbsoluteMetric(gridEvent, Products.Iari, Metrics.ManagedAccounts, dataSourceId, managedAccounts, metricStorage, logger);
        }

        private IMetric CreateIncrementalMetric(string name, GridEvent<Event<JToken>> gridEvent, string product, string instanceName, bool decrement = false)
        {
            var count = decrement ? -1 : 1;

            var instanceId = string.IsNullOrEmpty(instanceName) ? string.Empty : (string)GetEventDataProperty(gridEvent, instanceName);

            return new IncrementalMetric(gridEvent, product, name, instanceId, count, metricStorage, logger);
        }
    }
}
