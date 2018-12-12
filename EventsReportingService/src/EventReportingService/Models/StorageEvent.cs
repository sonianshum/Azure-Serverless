namespace EventReportingService.Models
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public class StorageEvent
    {
        public StorageEvent(string name, StorageOperation operation, string collection, dynamic data, Guid? organizationId, Guid? subscriptionId, Guid? userId)
        {
            Name = name;
            Operation = operation;
            Collection = collection;
            Data = data;
            OrganizationId = organizationId;
            SubscriptionId = subscriptionId;
            UserId = userId;
        }

        public string Name { get; }

        [JsonConverter(typeof(StringEnumConverter))]
        public StorageOperation Operation { get; }

        public string Collection { get; }

        public dynamic Data { get; }

        public Guid? OrganizationId { get; }

        public Guid? SubscriptionId { get; }

        public Guid? UserId { get; }
    }
}
