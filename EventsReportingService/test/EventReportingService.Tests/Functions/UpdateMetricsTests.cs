namespace EventReportingService.Tests.Functions
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Newtonsoft.Json.Linq;

    using Shouldly;

    using EventReportingService.Functions;
    using EventReportingService.Models;
    using EventReportingService.Utilities;

    using Xunit;
    using Xunit.Abstractions;

    public class UpdateMetricsTests : StoreEventsTestsBase
    {
        private const string TestNewProduct = "Product-id-new";
        private const string TestProduct1 = "Product-id-1";
        private const string TestProduct2 = "Product-id-2";
        private const string TestNewConnectorId = "Connector-id-new";
        private const string TestConnectorId1 = "Connector-id-1";
        private const string TestConnectorId2 = "Connector-id-2";
        private const string TestConnectorIdEmpty = "";
        private const string TestIariNewDataSourceId = "3107cdb8-083c-46ee-8362-15b6677b50ef";
        private const string TestIariDataSourceId1 = "f7d59ef3-fc4f-4cc3-88fd-f5866d084d0d";
        private const string TestIariDataSourceId2 = "d4a14ad0-5fa7-4b98-bdb0-5c507a2a64f3";
        private const long InitialCount = 10;

        private static readonly ProductMetric[] ProductMetrics =
        {
            new ProductMetric(Products.Connect, Services.ConnectSupervisor, Events.ObjectCreated, Metrics.ObjectsSynchronized),
            new ProductMetric(Products.Connect, Services.ConnectSupervisor, Events.ObjectModified, Metrics.ObjectsSynchronized),
            new ProductMetric(Products.Connect, Services.ConnectSupervisor, Events.ObjectDeleted, Metrics.ObjectsSynchronized),
            new ProductMetric(Products.Connect, Services.ConnectSupervisor, Events.ConnectorProvisioned, Metrics.ConnectorsProvisioned),
            new ProductMetric(Products.Tfa, Services.ClientProxy2fa, Events.UserCreated, Metrics.TotalUsers),
        };

        private readonly JToken testEventData = CreateInstanceEventData(Products.Tfa, TestProduct1);
        private readonly JToken testIariEventData = CreateInstanceEventData(Products.Iari, TestIariDataSourceId1, 1000);

        private readonly TestStorage<MetricTableEntity> metricsStorage = new TestStorage<MetricTableEntity>();

        public UpdateMetricsTests(ITestOutputHelper output)
            : base(output)
        {
            PopulateStorage().Wait();
        }

        [Theory]
        [InlineData("")]
        [InlineData("[]")]
        public async Task UpdateMetrics_should_return_400_bad_request_when_no_events_are_supplied(string json)
        {
            var result = await RunUpdateMetrics(json);

            result.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }

        [Theory]
        [InlineData("[{}]")] // Results in a GridEvent with null properties.
        [InlineData("[{ \"subject\": \"unknown\" }]")]
        [InlineData("[{ \"subject\": \"\" }]")]
        [InlineData("[{ \"subject\": null }]")]
        public async Task UpdateMetrics_should_ignore_events_with_unknown_subjects_and_return_200_success(string json)
        {
            var result = await RunUpdateMetrics(json);

            Logger.ShouldContainLogMessage("Ignoring unsupported event");
            result.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        [Theory]
        [InlineData("[{ \"subject\": \"2faClientProxy\", \"eventType\":\"Unknown\" }]")]
        [InlineData("[{ \"subject\": \"2faClientProxy\", \"eventType\": \"\" }]")]
        [InlineData("[{ \"subject\": \"2faClientProxy\", \"eventType\": null }]")]
        [InlineData("[{ \"subject\": \"2faClientProxy\" }]")]
        public async Task UpdateMetrics_should_ignore_events_with_unknown_event_types_and_return_200_success(string json)
        {
            var result = await RunUpdateMetrics(json);

            Logger.ShouldContainLogMessage("Ignoring unsupported event");
            result.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        [Theory]
        [InlineData(Events.OtpVerification)]
        [InlineData(Events.PushNotificationSent)]
        public async Task UpdateMetrics_should_create_a_new_2FA_Total2fas_metric_if_it_does_not_already_exist(string eventType)
        {
            var gridEvent = CreateEvent(Services.ClientProxy2fa, eventType, eventData: CreateInstanceEventData(Products.Tfa, TestNewProduct));

            await RunUpdateMetrics(gridEvent);

            AssertStorageContains(Products.Tfa, Metrics.Total2fas, TestNewProduct, 1, created: TestNewEventDate);
            AssertStorageContains(Products.Tfa, Metrics.Total2fas, TestProduct2, InitialCount, updated: TestUpdateDate);
        }

        [Theory]
        [InlineData(Events.OtpVerification)]
        [InlineData(Events.PushNotificationSent)]
        public async Task UpdateMetrics_should_increment_the_2FA_Total2fas_metric_if_it_already_exist(string eventType)
        {
            var gridEvent = CreateEvent(Services.ClientProxy2fa, eventType, eventData: testEventData);

            await RunUpdateMetrics(gridEvent);

            AssertStorageContains(Products.Tfa, Metrics.Total2fas, TestProduct1, InitialCount + 1);
            AssertStorageContains(Products.Tfa, Metrics.Total2fas, TestProduct2, InitialCount, updated: TestUpdateDate);
        }

        [Theory]
        [InlineData(Events.CollectorModuleDataCollected, 1000)]
        [InlineData(Events.DataSourceDisabled, 0)]
        [InlineData(Events.DataSourceEnabled, 1000)]
        [InlineData(Events.DataSourceDeleted, 0)]
        public async Task UpdateMetrics_should_create_a_new_Iari_ManagedAccounts_metric_if_it_does_not_already_exist(string eventType, long managedAccounts)
        {
            var gridEvent = CreateEvent(Services.IariSupervisor, eventType, eventData: CreateInstanceEventData(Products.Iari, TestIariNewDataSourceId, managedAccounts));

            await RunUpdateMetrics(gridEvent);

            AssertStorageContains(Products.Iari, Metrics.ManagedAccounts, TestIariNewDataSourceId, managedAccounts, created: TestNewEventDate);
            AssertStorageContains(Products.Iari, Metrics.ManagedAccounts, TestIariDataSourceId2, InitialCount, updated: TestUpdateDate);
        }

        [Theory]
        [InlineData(Events.CollectorModuleDataCollected, 1000)]
        [InlineData(Events.DataSourceDisabled, 0)]
        [InlineData(Events.DataSourceEnabled, 1000)]
        [InlineData(Events.DataSourceDeleted, 0)]
        public async Task UpdateMetrics_should_update_the_Iari_ManagedAccounts_metric_if_it_already_exist(string eventType, long managedAccounts)
        {
            var gridEvent = CreateEvent(Services.IariSupervisor, eventType, eventData: CreateInstanceEventData(Products.Iari, TestIariDataSourceId1, managedAccounts));

            await RunUpdateMetrics(gridEvent);

            AssertStorageContains(Products.Iari, Metrics.ManagedAccounts, TestIariDataSourceId1, managedAccounts);
            AssertStorageContains(Products.Iari, Metrics.ManagedAccounts, TestIariDataSourceId2, InitialCount, updated: TestUpdateDate);
        }

        [Fact]
        public async Task UpdateMetrics_should_not_update_the_Iari_ManagedAccounts_metric_if_the_event_time_is_older_than_the_last_updated_time()
        {
            var gridEvent = CreateEvent(Services.IariSupervisor, Events.CollectorModuleDataCollected, eventData: testIariEventData, eventTime: DateTime.UtcNow.AddDays(-10));

            await RunUpdateMetrics(gridEvent);

            AssertStorageContains(Products.Iari, Metrics.ManagedAccounts, TestIariDataSourceId1, InitialCount, updated: TestUpdateDate);
            Logger.ShouldContainLogMessage($"Skipping superseded update for Metric {Metrics.ManagedAccounts} {TestIariDataSourceId1}.");
        }

        [Fact]
        public async Task UpdateMetrics_should_not_update_the_Iari_ManagedAccounts_metric_if_it_already_has_the_same_count()
        {
            var gridEvent = CreateEvent(Services.IariSupervisor, Events.CollectorModuleDataCollected, eventData: CreateInstanceEventData(Products.Iari, TestIariDataSourceId1, InitialCount));

            await RunUpdateMetrics(gridEvent);

            AssertStorageContains(Products.Iari, Metrics.ManagedAccounts, TestIariDataSourceId1, InitialCount, updated: TestUpdateDate);
            Logger.ShouldContainLogMessage($"Skipping update for Metric {Metrics.ManagedAccounts} {TestIariDataSourceId1}. Metric already has the correct count.");
        }

        [Theory]
        [InlineData(Products.Connect, Services.ConnectSupervisor, Events.ObjectCreated, Metrics.ObjectsSynchronized, TestNewConnectorId, TestConnectorId2)]
        [InlineData(Products.Connect, Services.ConnectSupervisor, Events.ObjectModified, Metrics.ObjectsSynchronized, TestNewConnectorId, TestConnectorId2)]
        [InlineData(Products.Connect, Services.ConnectSupervisor, Events.ObjectDeleted, Metrics.ObjectsSynchronized, TestNewConnectorId, TestConnectorId2)]
        [InlineData(Products.Connect, Services.ConnectSupervisor, Events.ConnectorProvisioned, Metrics.ConnectorsProvisioned, TestConnectorIdEmpty, TestConnectorId2)]
        [InlineData(Products.Tfa, Services.ClientProxy2fa, Events.UserCreated, Metrics.TotalUsers, TestNewProduct, TestProduct2)]
        public async Task UpdateMetrics_should_create_a_new_metric_if_it_does_not_already_exist(
            string product,
            string service,
            string eventType,
            string metrics,
            string instance,
            string secondInstance)
        {
            var gridEvent = CreateEvent(service, eventType, subscriptionId: TestSecondSubscriptionId, eventData: CreateInstanceEventData(product, instance));

            await RunUpdateMetrics(gridEvent);

            AssertStorageContains(product, metrics, instance, 1, created: TestNewEventDate, subscriptionId: TestSecondSubscriptionId);
            AssertStorageContains(product, metrics, secondInstance, InitialCount, updated: TestUpdateDate);
        }

        [Theory]
        [InlineData(Products.Connect, Services.ConnectSupervisor, Events.ObjectCreated, Metrics.ObjectsSynchronized, TestConnectorId1, TestConnectorId2)]
        [InlineData(Products.Connect, Services.ConnectSupervisor, Events.ObjectModified, Metrics.ObjectsSynchronized, TestConnectorId1, TestConnectorId2)]
        [InlineData(Products.Connect, Services.ConnectSupervisor, Events.ObjectDeleted, Metrics.ObjectsSynchronized, TestConnectorId1, TestConnectorId2)]
        [InlineData(Products.Connect, Services.ConnectSupervisor, Events.ConnectorProvisioned, Metrics.ConnectorsProvisioned, TestConnectorIdEmpty, TestConnectorId2)]
        [InlineData(Products.Tfa, Services.ClientProxy2fa, Events.UserCreated, Metrics.TotalUsers, TestProduct1, TestProduct2)]
        public async Task UpdateMetrics_should_increment_the_metric_if_it_already_exists(
            string product,
            string service,
            string eventType,
            string metrics,
            string instance,
            string secondInstance)
        {
            var gridEvent = CreateEvent(service, eventType, eventData: CreateInstanceEventData(product, instance));

            await RunUpdateMetrics(gridEvent);

            AssertStorageContains(product, metrics, instance, InitialCount + 1);
            AssertStorageContains(product, metrics, secondInstance, InitialCount, updated: TestUpdateDate);
        }

        [Theory]
        [InlineData(Products.Connect, Services.ConnectSupervisor, Events.ObjectCreated, Metrics.ObjectsSynchronized, TestConnectorId1, TestConnectorId2)]
        [InlineData(Products.Connect, Services.ConnectSupervisor, Events.ObjectModified, Metrics.ObjectsSynchronized, TestConnectorId1, TestConnectorId2)]
        [InlineData(Products.Connect, Services.ConnectSupervisor, Events.ObjectDeleted, Metrics.ObjectsSynchronized, TestConnectorId1, TestConnectorId2)]
        [InlineData(Products.Connect, Services.ConnectSupervisor, Events.ConnectorProvisioned, Metrics.ConnectorsProvisioned, TestConnectorIdEmpty, TestConnectorId2)]
        [InlineData(Products.Tfa, Services.ClientProxy2fa, Events.UserCreated, Metrics.TotalUsers, TestProduct1, TestProduct2)]
        public async Task UpdateMetrics_should_increment_the_metric_if_it_already_exists_and_no_organizationId(
            string product,
            string service,
            string eventType,
            string metrics,
            string instance,
            string secondInstance)
        {
            var gridEvent = CreateEvent(service, eventType, eventData: CreateInstanceEventData(product, instance), nullOrganizationId: true);

            await RunUpdateMetrics(gridEvent);

            AssertStorageContains(product, metrics, instance, InitialCount + 1);
            AssertStorageContains(product, metrics, secondInstance, InitialCount, updated: TestUpdateDate);
        }

        [Theory]
        [InlineData(Products.Tfa, Services.ClientProxy2fa, Events.UserDeleted, Metrics.TotalUsers)]
        public async Task UpdateMetrics_should_decrement_the_metric_if_it_already_exists(
            string product,
            string service,
            string eventType,
            string metrics)
        {
            var gridEvent = CreateEvent(service, eventType, eventData: testEventData);

            await RunUpdateMetrics(gridEvent);

            AssertStorageContains(product, metrics, TestProduct1, InitialCount - 1);
            AssertStorageContains(product, metrics, TestProduct2, InitialCount, updated: TestUpdateDate);
        }

        [Theory]
        [CombinatorialData]
        public async Task UpdateMetrics_should_create_a_new_metric_with_an_empty_instance_when_the_product_is_not_supplied(
            [CombinatorialValues(0, 1, 2)] int productIndex,
            [CombinatorialValues(null, "", "12345", " \"12345\" ", "{}", "{ \"productName\": \"\" }", "{ \"productName\": null }", "{ \"connectorId\": \"\" }", "{ \"connectorId\": null }")] string eventData)
        {
            var gridEvent = CreateEvent(ProductMetrics[productIndex].ServiceName, ProductMetrics[productIndex].EventName, eventData: CreateEventData(eventData));

            await RunUpdateMetrics(gridEvent);

            AssertStorageContains(ProductMetrics[productIndex].ProductName, ProductMetrics[productIndex].MetricName, string.Empty, 1, created: TestNewEventDate);
        }

        private static GridEvent<Event<JToken>> CreateEvent(
            string subject,
            string eventType,
            DateTime? eventTime = null,
            Guid? organizationId = null,
            Guid? subscriptionId = null,
            Guid? userId = null,
            JToken eventData = null,
            bool nullOrganizationId = false)
        {
            return new GridEvent<Event<JToken>>(
                Guid.NewGuid(),
                eventTime ?? TestNewEventDate,
                subject,
                eventType,
                new Event<JToken>(
                    nullOrganizationId ? (Guid?)null : organizationId ?? TestOrganizationId,
                    subscriptionId ?? TestSubscriptionId,
                    userId ?? TestUserID,
                    eventData));
        }

        private static JToken CreateEventData(string json)
        {
            return json == null ? null : Json.DeserializeObject<JToken>(json);
        }

        private static JToken CreateInstanceEventData(string product, string instanceId, long managedAccounts = 0)
        {
            if (string.IsNullOrEmpty(instanceId))
            {
                return null;
            }

            switch (product)
            {
                case Products.Tfa:
                    return CreateEventData($"{{\"productName\": \"{instanceId}\" }}");
                case Products.Iari:
                    return CreateEventData($"{{\"dataSourceId\": \"{instanceId}\", \"managedAccounts\": \"{managedAccounts}\" }}");
                case Products.Connect:
                    return CreateEventData($"{{\"connectorId\": \"{instanceId}\" }}");
                default:
                    return null;
            }
        }

        private void AssertStorageContains(string product, string name, string instance, long count, DateTime? created = null, DateTime? updated = null, Guid? organizationId = null, Guid? subscriptionId = null)
        {
            created = created ?? TestCreateDate;
            updated = updated ?? TestNewEventDate;
            organizationId = organizationId ?? TestOrganizationId;
            subscriptionId = subscriptionId ?? TestSubscriptionId;

            metricsStorage.ShouldContain(m =>
                m.Product == product
                && m.Name == name
                && m.Instance == instance
                && m.Count == count
                && m.Created == created
                && m.Updated == updated
                && m.OrganizationId == organizationId
                && m.SubscriptionId == subscriptionId);
        }

        private async Task<HttpResponseMessage> RunUpdateMetrics(string json)
        {
            var gridEvents = Json.DeserializeObject<GridEvent<Event<JToken>>[]>(json);

            return await RunUpdateMetrics(gridEvents);
        }

        private async Task<HttpResponseMessage> RunUpdateMetrics(GridEvent<Event<JToken>> gridEvent)
            => await RunUpdateMetrics(new[] { gridEvent });

        private async Task<HttpResponseMessage> RunUpdateMetrics(GridEvent<Event<JToken>>[] gridEvents)
        {
            return await UpdateMetrics.Run(gridEvents, TestCloudTable.MetricsTable, metricsStorage, Logger);
        }

        private async Task PopulateStorage()
        {
            await Add(Products.Tfa, Metrics.TotalUsers, TestProduct1);
            await Add(Products.Tfa, Metrics.TotalUsers, TestProduct2);
            await Add(Products.Tfa, Metrics.Total2fas, TestProduct1);
            await Add(Products.Tfa, Metrics.Total2fas, TestProduct2);
            await Add(Products.Iari, Metrics.ManagedAccounts, TestIariDataSourceId1);
            await Add(Products.Iari, Metrics.ManagedAccounts, TestIariDataSourceId2);
            await Add(Products.Connect, Metrics.ObjectsSynchronized, TestConnectorId1);
            await Add(Products.Connect, Metrics.ObjectsSynchronized, TestConnectorId2);
            await Add(Products.Connect, Metrics.ConnectorsProvisioned, TestConnectorIdEmpty);
            await Add(Products.Connect, Metrics.ConnectorsProvisioned, TestConnectorId2);

            async Task Add(string product, string name, string instance, long count = InitialCount)
            {
                await metricsStorage.Insert(new MetricTableEntity(
                    TestCreateDate,
                    TestUpdateDate,
                    product,
                    name,
                    TestOrganizationId,
                    TestSubscriptionId,
                    instance,
                    count));
            }
        }

        private class ProductMetric
        {
            public ProductMetric(
                string productName,
                string serviceName,
                string eventName,
                string metricName)
            {
                ProductName = productName;
                ServiceName = serviceName;
                EventName = eventName;
                MetricName = metricName;
            }

            public string ProductName { get; }

            public string ServiceName { get; }

            public string EventName { get; }

            public string MetricName { get; }
        }
    }
}