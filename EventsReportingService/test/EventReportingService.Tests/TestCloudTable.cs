namespace EventReportingService.Tests
{
    using System;

    using Microsoft.WindowsAzure.Storage.Auth;
    using Microsoft.WindowsAzure.Storage.Table;

    using EventReportingService.Models;

    public class TestCloudTable : CloudTable
    {
        private TestCloudTable(string tableName)
            : base(new Uri($"https://localhost/{tableName}"), new StorageCredentials("test", "test"))
        {
        }

        public static CloudTable MetricsTable { get; } = new TestCloudTable(Collections.Metrics);

        public static CloudTable EventsTable { get; } = new TestCloudTable(Collections.Events);

        public static CloudTable AccountsV1Table { get; } = new TestCloudTable(Collections.AccountsV1);

        public static CloudTable UsersV1Table { get; } = new TestCloudTable(Collections.UsersV1);

        public static CloudTable ContactsV1Table { get; } = new TestCloudTable(Collections.ContactsV1);
    }
}
