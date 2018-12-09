namespace EventReportingService.Tests.Functions
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Shouldly;

    using EventReportingService.Functions;
    using EventReportingService.Models;
    using EventReportingService.Storage;
    using EventReportingService.Utilities;

    using Xunit;
    using Xunit.Abstractions;

    public class StoreStorageEventsTests : StoreEventsTestsBase
    {
        private const string CreateAccountJson =
            "{" +
                "\"id\":\"15247381-e0ff-4aeb-b0ee-ddbb75157be2\"," +
                "\"validationCode\":\"oS2eNNU9aMcSbiOYWKmAFhsTDN7AKFAzBzpiSiQ5aFY\"," +
                "\"registrationTime\":\"2017-12-01T17:56:46.222Z\"," +
                "\"registration\":{" +
                    "\"companyName\":\"Acme\"," +
                    "\"firstName\":\"Jon\"," +
                    "\"lastName\":\"Doe\"," +
                    "\"email\":\"jon.doe@acme.com\"," +
                    "\"phoneNumber\":\"+12015555555\"," +
                    "\"termsAccepted\":true," +
                    "\"privacyAccepted\":true," +
                    "\"staAccepted\":true," +
                    "\"validated\":false" +
                "}," +
                "\"subscriptions\":{}" +
            "}";

        private const string TransformedCreateAccountJson =
            "{" +
                "\"id\":\"15247381-e0ff-4aeb-b0ee-ddbb75157be2\"," +
                "\"validationCode\":\"oS2eNNU9aMcSbiOYWKmAFhsTDN7AKFAzBzpiSiQ5aFY\"," +
                "\"registrationTime\":\"2017-12-01T17:56:46.222Z\"," +
                "\"registration\":{" +
                    "\"companyName\":\"Acme\"," +
                    "\"firstName\":\"Jon\"," +
                    "\"lastName\":\"Doe\"," +
                    "\"email\":\"jon.doe@acme.com\"," +
                    "\"phoneNumber\":\"+12015555555\"," +
                    "\"termsAccepted\":true," +
                    "\"privacyAccepted\":true," +
                    "\"staAccepted\":true," +
                    "\"validated\":false" +
                "}," +
                "\"subscriptions\":[]" +
            "}";

        private const string UpdateAccountJson =
            "{" +
                "\"id\":\"15247381-e0ff-4aeb-b0ee-ddbb75157be2\"," +
                "\"validationCode\":\"oS2eNNU9aMcSbiOYWKmAFhsTDN7AKFAzBzpiSiQ5aFY\"," +
                "\"registrationTime\":\"2017-12-01T17:56:46.222Z\"," +
                "\"registration\":{" +
                    "\"companyName\":\"Acme\"," +
                    "\"firstName\":\"Jon\"," +
                    "\"lastName\":\"Doe\"," +
                    "\"email\":\"jon.doe@acme.com\"," +
                    "\"phoneNumber\":\"+12015555555\"," +
                    "\"termsAccepted\":true," +
                    "\"privacyAccepted\":true," +
                    "\"staAccepted\":true," +
                    "\"validated\":true" +
                "}," +
                "\"subscriptions\":{" +
                    "\"fce41130-9947-48b0-a8cc-745c224510d6\":{" +
                        "\"trialExtensions\":0," +
                        "\"createdDate\":\"2017-10-19T02:00:55.050Z\"," +
                        "\"id\":\"fce41130-9947-48b0-a8cc-745c224510d6\"," +
                        "\"product\":\"marvel\"," +
                        "\"type\":1," +
                        "\"expiryDate\":\"2017-11-18T02:00:55.050Z\"," +
                        "\"trialCanBeExtended\":false" +
                    "}," +
                    "\"c1e9bb0c-c84f-44b8-87b7-f4a67a9a7184\":{" +
                        "\"trialExtensions\":0," +
                        "\"createdDate\":\"2017-10-19T02:01:01.036Z\"," +
                        "\"id\":\"c1e9bb0c-c84f-44b8-87b7-f4a67a9a7184\"," +
                        "\"product\":\"IAI\"," +
                        "\"type\":1," +
                        "\"expiryDate\":\"2017-11-18T02:01:01.036Z\"," +
                        "\"trialCanBeExtended\":false" +
                    "}" +
                "}" +
            "}";

        private const string TransformedUpdateAccountJson =
            "{" +
                "\"id\":\"15247381-e0ff-4aeb-b0ee-ddbb75157be2\"," +
                "\"validationCode\":\"oS2eNNU9aMcSbiOYWKmAFhsTDN7AKFAzBzpiSiQ5aFY\"," +
                "\"registrationTime\":\"2017-12-01T17:56:46.222Z\"," +
                "\"registration\":{" +
                    "\"companyName\":\"Acme\"," +
                    "\"firstName\":\"Jon\"," +
                    "\"lastName\":\"Doe\"," +
                    "\"email\":\"jon.doe@acme.com\"," +
                    "\"phoneNumber\":\"+12015555555\"," +
                    "\"termsAccepted\":true," +
                    "\"privacyAccepted\":true," +
                    "\"staAccepted\":true," +
                    "\"validated\":true" +
                "}," +
                "\"subscriptions\":[" +
                    "{" +
                    "\"trialExtensions\":0," +
                    "\"createdDate\":\"2017-10-19T02:00:55.050Z\"," +
                    "\"id\":\"fce41130-9947-48b0-a8cc-745c224510d6\"," +
                    "\"product\":\"marvel\"," +
                    "\"type\":1," +
                    "\"expiryDate\":\"2017-11-18T02:00:55.050Z\"," +
                    "\"trialCanBeExtended\":false" +
                    "}," +
                    "{" +
                    "\"trialExtensions\":0," +
                    "\"createdDate\":\"2017-10-19T02:01:01.036Z\"," +
                    "\"id\":\"c1e9bb0c-c84f-44b8-87b7-f4a67a9a7184\"," +
                    "\"product\":\"IAI\"," +
                    "\"type\":1," +
                    "\"expiryDate\":\"2017-11-18T02:01:01.036Z\"," +
                    "\"trialCanBeExtended\":false" +
                    "}" +
                "]" +
            "}";

        private const string CreateUserJson =
            "{" +
                "\"id\":\"65c6db73-0a66-42c2-81fe-f951af3bda11\"," +
                "\"accountIds\":[" +
                    "\"4dda7fdb-c45a-4d60-8388-c91c9559ff56\"" +
                "]," +
                "\"authenticationUserId\":\"188065cd-a930-46d6-a61e-82141e33ce7d\"," +
                "\"email\":\"jon.doe@acme.com\"," +
                "\"firstName\":\"Jon\"," +
                "\"lastName\":\"Doe\"," +
                "\"phoneNumber\":\"+12015555555\"," +
                "\"termsAccepted\":true," +
                "\"privacyAccepted\":true," +
                "\"staAccepted\":true," +
                "\"isWorkAccount\":false" +
            "}";

        private const string UpdateUserJson =
            "{" +
                "\"id\":\"65c6db73-0a66-42c2-81fe-f951af3bda11\"," +
                "\"accountIds\":[" +
                    "\"4dda7fdb-c45a-4d60-8388-c91c9559ff56\"" +
                "]," +
                "\"authenticationUserId\":\"188065cd-a930-46d6-a61e-82141e33ce7d\"," +
                "\"email\":\"jon.doe@acme.com\"," +
                "\"firstName\":\"Jon\"," +
                "\"lastName\":\"Doe\"," +
                "\"phoneNumber\":\"+12015555555\"," +
                "\"termsAccepted\":true," +
                "\"privacyAccepted\":true," +
                "\"staAccepted\":true," +
                "\"isWorkAccount\":false," +
                "\"country\":\"United States\"," +
                "\"state\":\"Arizona\"" +
            "}";

        private const string CreateContactJson =
            "{" +
            "\"organizationId\":\"84278019-5220-4986-864f-163cc76cf407\"," +
            "\"id\":\"0308a75e-eec2-4756-8013-b9426130ca9f\"," +
            "\"firstName\":\"ansh\"," +
            "\"lastName\":\"soni\"," +
            "\"email\":\"ansh.soni@test.com\"," +
            "\"contactTypes\":[\"SubProcessorChange\"]" +
            "}";

        private const string UpdateContactJson =
            "{" +
            "\"organizationId\":\"84278019-5220-4986-864f-163cc76cf407\"," +
            "\"id\":\"0308a75e-eec2-4756-8013-b9426130ca9f\"," +
            "\"firstName\":\"ansh\"," +
            "\"lastName\":\"soni\"," +
            "\"email\":\"ansh.soni@mail.com\"," +
            "\"contactTypes\":[\"SubProcessorChange\",\"DataBreach\"]" +
            "}";

        private static readonly Guid TestAccountId = new Guid("15247381-e0ff-4aeb-b0ee-ddbb75157be2");
        private static readonly Guid TestUserId = new Guid("65c6db73-0a66-42c2-81fe-f951af3bda11");
        private static readonly Guid TestContactId = new Guid("0308a75e-eec2-4756-8013-b9426130ca9f");

        private readonly TestStorage<StorageEventTableEntity> accountsStorage = new TestStorage<StorageEventTableEntity>();
        private readonly TestStorage<StorageEventTableEntity> UsersStorage = new TestStorage<StorageEventTableEntity>();
        private readonly TestStorage<StorageEventTableEntity> contactsStorage = new TestStorage<StorageEventTableEntity>();

        public StoreStorageEventsTests(ITestOutputHelper output)
            : base(output)
        {
            PopulateStorage().Wait();
        }

        [Theory]
        [InlineData("")]
        [InlineData("[]")]
        public async Task StoreStorageEvents_should_return_400_bad_request_when_no_events_are_supplied(string json)
        {
            var result = await RunStorageEvent(json);

            result.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }

        [Theory]
        [InlineData("[{}]")] // Results in a GridEvent with null properties.
        [InlineData("[{ \"subject\": \"unknown\" }]")]
        [InlineData("[{ \"subject\": \"\" }]")]
        [InlineData("[{ \"subject\": null }]")]
        public async Task StoreStorageEvents_should_ignore_unexpected_events_and_return_200_success(string json)
        {
            var result = await RunStorageEvent(json);

            Logger.ShouldContainLogMessage("Ignoring unsupported event");
            result.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        [Fact]
        public async Task StoreStorageEvents_should_ignore_events_from_unsupported_services()
        {
            var gridEvent = CreateEvent("Add", StorageOperation.Create, Collections.AccountsV1, CreateAccountJson, eventType: "UnsupportedEvent");

            var result = await RunStorageEvent(gridEvent);

            Logger.ShouldContainLogMessage("Ignoring unsupported event");
            result.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        [Fact]
        public async Task StoreStorageEvents_should_ignore_events_of_an_unsupported_type()
        {
            var gridEvent = CreateEvent("Add", StorageOperation.Create, Collections.AccountsV1, CreateAccountJson, subject: "UnsupportedService");

            var result = await RunStorageEvent(gridEvent);

            Logger.ShouldContainLogMessage("Ignoring unsupported event");
            result.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        [Fact]
        public async Task StoreStorageEvents_should_ignore_unsupported_collections()
        {
            var gridEvent = CreateEvent("Add", StorageOperation.Create, "UnsupportedCollection", CreateAccountJson);

            var result = await RunStorageEvent(gridEvent);

            Logger.ShouldContainLogMessage("Ignoring unsupported collection: UnsupportedCollection");
            result.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("12345")]
        [InlineData(12345)]
        public async Task StoreStorageEvents_should_return_bad_request_when_events_are_missing_a_valid_data_model_id(object id)
        {
            var gridEvent = CreateEvent("Delete", StorageOperation.Delete, Collections.AccountsV1, Json.SerializeObject(id));

            HttpResponseMessage result = await RunStorageEvent(gridEvent);

            Logger.ShouldContainLogMessage($"Invalid request: StorageEvent.Data must be an id guid but was: {id}");
            result.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task StoreStorageEvents_should_store_create_account_events_in_the_accounts_table()
        {
            accountsStorage.Delete(TestAccountId);

            var gridEvent = CreateEvent("Add", StorageOperation.Create, Collections.AccountsV1, CreateAccountJson);

            var result = await RunStorageEvent(gridEvent);

            result.StatusCode.ShouldBe(HttpStatusCode.OK);
            AssertStorageContains(accountsStorage, TestAccountId, TransformedCreateAccountJson, TestNewEventDate);
        }

        [Fact]
        public async Task StoreStorageEvents_should_store_update_account_events_in_the_accounts_table()
        {
            var gridEvent = CreateEvent("SetValidated", StorageOperation.Update, Collections.AccountsV1, UpdateAccountJson);

            var result = await RunStorageEvent(gridEvent);

            result.StatusCode.ShouldBe(HttpStatusCode.OK);
            AssertStorageContains(accountsStorage, TestAccountId, TransformedUpdateAccountJson);
        }

        [Fact]
        public async Task StoreStorageEvents_should_store_delete_account_events_in_the_accounts_table()
        {
            var gridEvent = CreateEvent("Delete", StorageOperation.Delete, Collections.AccountsV1, Json.SerializeObject(TestAccountId));

            var result = await RunStorageEvent(gridEvent);

            result.StatusCode.ShouldBe(HttpStatusCode.OK);
            AssertStorageContains(accountsStorage, TestAccountId, TransformedCreateAccountJson, deleted: TestNewEventDate);
        }

        [Fact]
        public async Task StoreStorageEvents_should_store_create_User_events_in_the_Users_table()
        {
            UsersStorage.Delete(TestUserId);

            var gridEvent = CreateEvent("Add", StorageOperation.Create, Collections.UsersV1, CreateUserJson);

            var result = await RunStorageEvent(gridEvent);

            result.StatusCode.ShouldBe(HttpStatusCode.OK);
            AssertStorageContains(UsersStorage, TestUserId, CreateUserJson, TestNewEventDate);
        }

        [Fact]
        public async Task StoreStorageEvents_should_store_update_User_events_in_the_Users_table()
        {
            var gridEvent = CreateEvent("SetCountryAndState", StorageOperation.Update, Collections.UsersV1, UpdateUserJson);

            var result = await RunStorageEvent(gridEvent);

            result.StatusCode.ShouldBe(HttpStatusCode.OK);
            AssertStorageContains(UsersStorage, TestUserId, UpdateUserJson);
        }

        [Fact]
        public async Task StoreStorageEvents_should_store_delete_User_events_in_the_Users_table()
        {
            var gridEvent = CreateEvent("Delete", StorageOperation.Delete, Collections.UsersV1, Json.SerializeObject(TestUserId));

            var result = await RunStorageEvent(gridEvent);

            result.StatusCode.ShouldBe(HttpStatusCode.OK);
            AssertStorageContains(UsersStorage, TestUserId, CreateUserJson, deleted: TestNewEventDate);
        }

        [Fact]
        public async Task StoreStorageEvents_should_store_create_contact_events_in_the_contacts_table()
        {
            contactsStorage.Delete(TestContactId);

            var gridEvent = CreateEvent("Add", StorageOperation.Create, Collections.ContactsV1, CreateContactJson);

            var result = await RunStorageEvent(gridEvent);

            result.StatusCode.ShouldBe(HttpStatusCode.OK);
            AssertStorageContains(contactsStorage, TestContactId, CreateContactJson, TestNewEventDate);
        }

        [Fact]
        public async Task StoreStorageEvents_should_store_update_contact_events_in_the_contacts_table()
        {
            var gridEvent = CreateEvent("SetCountryAndState", StorageOperation.Update, Collections.ContactsV1, UpdateContactJson);

            var result = await RunStorageEvent(gridEvent);

            result.StatusCode.ShouldBe(HttpStatusCode.OK);
            AssertStorageContains(contactsStorage, TestContactId, UpdateContactJson);
        }

        [Fact]
        public async Task StoreStorageEvents_should_store_delete_contact_events_in_the_contacts_table()
        {
            var gridEvent = CreateEvent("Delete", StorageOperation.Delete, Collections.ContactsV1, Json.SerializeObject(TestContactId));

            var result = await RunStorageEvent(gridEvent);

            result.StatusCode.ShouldBe(HttpStatusCode.OK);
            AssertStorageContains(contactsStorage, TestContactId, CreateContactJson, deleted: TestNewEventDate);
        }

        [Fact]
        public async Task StoreStorageEvents_should_store_updates_even_when_the_item_to_update_doesnt_exist()
        {
            accountsStorage.Delete(TestAccountId);

            var gridEvent = CreateEvent("SetValidated", StorageOperation.Update, Collections.AccountsV1, UpdateAccountJson);

            await RunStorageEvent(gridEvent);

            AssertStorageContains(accountsStorage, TestAccountId, TransformedUpdateAccountJson, TestNewEventDate);
        }

        [Fact]
        public async Task StoreStorageEvents_should_ignore_updates_if_they_are_older_than_the_version_stored()
        {
            var gridEvent = CreateEvent("SetValidated", StorageOperation.Update, Collections.AccountsV1, UpdateAccountJson, eventTime: TestCreateDate);

            await RunStorageEvent(gridEvent);

            AssertStorageContains(accountsStorage, TestAccountId, TransformedCreateAccountJson, TestCreateDate, modified: TestUpdateDate);
        }

        [Fact]
        public async Task StoreStorageEvents_should_store_deletes_even_when_the_item_to_delete_doesnt_exist()
        {
            accountsStorage.Delete(TestAccountId);

            var gridEvent = CreateEvent("Delete", StorageOperation.Delete, Collections.AccountsV1, Json.SerializeObject(TestAccountId));

            var result = await RunStorageEvent(gridEvent);

            result.StatusCode.ShouldBe(HttpStatusCode.OK);
            AssertStorageContains(accountsStorage, TestAccountId, null, TestNewEventDate, TestNewEventDate);
        }

        private static GridEvent<StorageEvent> CreateEvent(
            string name,
            StorageOperation operation,
            string collection,
            string data,
            string subject = Services.AccountSupervisor,
            string eventType = Events.StorageEvent,
            DateTime? eventTime = null,
            Guid? organizationId = null,
            Guid? subscriptionId = null,
            Guid? userId = null)
        {
            return new GridEvent<StorageEvent>(
                Guid.NewGuid(),
                eventTime ?? TestNewEventDate,
                subject,
                eventType,
                new StorageEvent(
                    name,
                    operation,
                    collection,
                    data == null ? null : Json.DeserializeObject(data),
                    organizationId ?? TestOrganizationId,
                    subscriptionId ?? TestSubscriptionId,
                    userId ?? TestUserId));
        }

        private async Task<HttpResponseMessage> RunStorageEvent(string json)
        {
            var gridEvents = Json.DeserializeObject<GridEvent<StorageEvent>[]>(json);

            return await RunStorageEvent(gridEvents);
        }

        private async Task<HttpResponseMessage> RunStorageEvent(GridEvent<StorageEvent> gridEvent)
        {
            return await RunStorageEvent(Json.SerializeObject(new[] { gridEvent }));
        }

        private async Task<HttpResponseMessage> RunStorageEvent(GridEvent<StorageEvent>[] gridEvents)
        {
            var storageFactory = new TestStorageFactory<StorageEventTableEntity>();

            storageFactory.SetStorage(TestCloudTable.AccountsV1Table, accountsStorage);
            storageFactory.SetStorage(TestCloudTable.UsersV1Table, UsersStorage);
            storageFactory.SetStorage(TestCloudTable.ContactsV1Table, contactsStorage);

            return await StoreStorageEvents.Run(gridEvents, TestCloudTable.AccountsV1Table, TestCloudTable.UsersV1Table, TestCloudTable.ContactsV1Table, storageFactory, Logger);
        }

        private void AssertStorageContains(TestStorage<StorageEventTableEntity> storage, Guid id, string data, DateTime? created = null, DateTime? deleted = null, DateTime? modified = null)
        {
            created = created ?? TestCreateDate;
            modified = modified ?? TestNewEventDate;

            storage.ShouldContain(e =>
                e.RowKey == id.ToString()
                && e.Created == created
                && e.Modified == modified
                && e.Deleted == deleted
                && e.Data == data);
        }

        private async Task PopulateStorage()
        {
            await Add(accountsStorage, TestAccountId, TestCreateDate, TestUpdateDate, TransformedCreateAccountJson);
            await Add(UsersStorage, TestUserId, TestCreateDate, TestUpdateDate, CreateUserJson);
            await Add(contactsStorage, TestContactId, TestCreateDate, TestUpdateDate, CreateContactJson);

            async Task Add(IStorage<StorageEventTableEntity> storage, Guid id, DateTime? createTime, DateTime modifiedTime, string data)
            {
                await storage.Insert(new StorageEventTableEntity(
                    id.ToString(),
                    createTime,
                    modifiedTime,
                    null,
                    data));
            }
        }
    }
}