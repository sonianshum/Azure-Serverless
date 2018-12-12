namespace EventReportingService.Tests.Functions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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

    public class StoreEventHistoryTests : StoreEventsTestsBase
    {
        private static readonly string TfaUserCreatedJson =
            "{" +
                "\"id\":\"b6ab05dc-3309-48a5-8e21-c9a329ef2100\"," +
                "\"eventTime\":" + TestNewEventDate.ToJson() + "," +
                "\"subject\":\"faClient\"," +
                "\"eventType\":\"UserCreated\"," +
                "\"data\":" +
                "{" +
                    "\"eventData\":" +
                    "{" +
                        "\"productName\":\"AccessManager\"," +
                        "\"userId\":1" +
                    "}," +
                    "\"organizationId\":\"4c5067a4-0893-4e9f-844e-bddda8a90900\"," +
                    "\"subscriptionId\":\"2d8949f0-0100-4b62-83b7-ef97729fc800\"" +
                "}" +
            "}";
       

        private readonly TestStorage<EventHistoryTableEntity> eventsStorage = new TestStorage<EventHistoryTableEntity>();

        public StoreEventHistoryTests(ITestOutputHelper output)
            : base(output)
        {
        }

        public static IEnumerable<object[]> AllServices() => Services.All.Select(s => new[] { s });

        [Theory]
        [InlineData("")]
        [InlineData("[]")]
        public async Task StoreEventHistory_should_return_400_bad_request_when_no_events_are_supplied(string json)
        {
            var result = await RunStoreEventHistory(json);

            result.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }

        [Theory]
        [InlineData("[{}]")] // Results in a GridEvent with null properties.
        [InlineData("[{ \"subject\": \"unknown\" }]")]
        [InlineData("[{ \"subject\": \"\" }]")]
        [InlineData("[{ \"subject\": null }]")]
        public async Task StoreEventHistory_should_ignore_unexpected_events_and_return_200_success(string json)
        {
            var result = await RunStoreEventHistory(json);

            Logger.ShouldContainLogMessage("Ignoring unsupported event");
            result.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        [Fact]
        public async Task StoreEventHistory_should_ignore_events_from_unsupported_services()
        {
            var gridEvent = CreateEvent("UnsupportedService", Events.StorageEvent, null, TestSubscriptionId, "Add", StorageOperation.Create, Collections.ContactsV1);

            var result = await RunStoreEventHistory(gridEvent);

            Logger.ShouldContainLogMessage("Ignoring unsupported event");
            result.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        [Fact]
        public async Task StoreEventHistory_should_store_event_if_name_field_is_missing()
        {
            var gridEvent = CreateEvent(Services.ASupervisor, Events.StorageEvent, TestOrganizationId, TestSubscriptionId, null, StorageOperation.Create, Collections.ContactsV1);

            await RunStoreEventHistory(gridEvent);

            AssertStorageContains(Events.StorageEvent, null, TestOrganizationId, TestSubscriptionId, gridEvent.ToJson());
        }

        [Fact]
        public async Task StoreEventHistory_should_store_event_if_operation_field_is_missing()
        {
            var gridEvent = CreateEvent(Services.ASupervisor, Events.StorageEvent, TestOrganizationId, TestSubscriptionId, "Add", null, Collections.ContactsV1);

            await RunStoreEventHistory(gridEvent);

            AssertStorageContains(Events.StorageEvent, "Add", TestOrganizationId, TestSubscriptionId, gridEvent.ToJson());
        }

        [Fact]
        public async Task StoreEventHistory_should_store_event_if_organization_id_field_is_missing()
        {
            var gridEvent = CreateEvent(Services.ASupervisor, Events.StorageEvent, null, TestSubscriptionId, "Add", StorageOperation.Create, Collections.ContactsV1);

            await RunStoreEventHistory(gridEvent);

            AssertStorageContains(Events.StorageEvent, "Add", null, TestSubscriptionId, gridEvent.ToJson());
        }

        [Fact]
        public async Task StoreEventHistory_should_store_event_if_subscription_id_field_is_missing()
        {
            var gridEvent = CreateEvent(Services.ASupervisor, Events.StorageEvent, TestOrganizationId, null, "Add", StorageOperation.Create, Collections.ContactsV1);

            await RunStoreEventHistory(gridEvent);

            AssertStorageContains(Events.StorageEvent, "Add", TestOrganizationId, null, gridEvent.ToJson());
        }
   
        [Theory]
        [MemberData(nameof(AllServices))]
        public async Task StoreEventHistory_should_store_events_from_all_known_services_in_the_event_history_table(string service)
        {
            var gridEvent = CreateEvent(service, Events.StorageEvent, TestOrganizationId, TestSubscriptionId, "Add", StorageOperation.Create, Collections.ContactsV1);

            await RunStoreEventHistory(gridEvent);

            AssertStorageContains(Events.StorageEvent, "Add", TestOrganizationId, TestSubscriptionId, gridEvent.ToJson());
        }
     
        private static GridEvent<JObject> CreateEvent(
            string subject,
            string eventType,
            Guid? organizationId,
            Guid? subscriptionId,
            string name,
            StorageOperation? operation,
            string collection,
            object eventData = null)
        {
            var data = new
            {
                OrganizationId = organizationId,
                SubscriptionId = subscriptionId,
                Name = name,
                Operation = operation?.ToString(),
                Collection = collection,
                Data = eventData,
            };

            return new GridEvent<JObject>(
                Guid.NewGuid(),
                TestNewEventDate,
                subject,
                eventType,
                Json.ConvertToJObject(data));
        }

        private async Task<HttpResponseMessage> RunStoreEventHistory(string json)
        {
            var gridEvents = Json.DeserializeObject<GridEvent<JObject>[]>(json);

            return await RunStoreEventHistory(gridEvents);
        }

        private async Task<HttpResponseMessage> RunStoreEventHistory(GridEvent<JObject> gridEvent)
        {
            return await RunStoreEventHistory(new[] { gridEvent });
        }

        private async Task<HttpResponseMessage> RunStoreEventHistory(GridEvent<JObject>[] gridEvents)
        {
            return await StoreEventHistory.Run(gridEvents, TestCloudTable.EventsTable, eventsStorage, Logger);
        }

        private void AssertStorageContains(string eventType, string operationName, Guid? organizationId, Guid? subscriptionId, string data)
        {
            eventsStorage.ShouldContain(e =>
                e.Created == TestNewEventDate
                && e.EventType == eventType
                && e.OperationName == operationName
                && e.OrganizationId == organizationId
                && e.SubscriptionId == subscriptionId
                && e.Data == data);
        }
    }
}