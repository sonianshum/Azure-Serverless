namespace EventReportingService.Models
{
    public static class Events
    {
        public const string StorageEvent = "StorageEvent";

        public const string UserCreated = "UserCreated";

        public const string UserDeleted = "UserDeleted";

        public const string PushNotificationSent = "PushNotificationSent";

        public const string OtpVerification = "OtpVerification";

        public const string CollectorModuleDataCollected = "CollectorModuleDataCollected";

        public const string DataSourceDisabled = "DataSourceDisabled";

        public const string DataSourceDeleted = "DataSourceDeleted";

        public const string DataSourceEnabled = "DataSourceEnabled";

        public const string ConnectorProvisioned = "ConnectorProvisioned";

        public const string ConnectorDeleted = "ConnectorDeleted";

        public const string ObjectCreated = "ObjectCreated";

        public const string ObjectModified = "ObjectModified";

        public const string ObjectDeleted = "ObjectDeleted";
    }
}
