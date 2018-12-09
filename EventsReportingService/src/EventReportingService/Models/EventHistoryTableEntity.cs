namespace EventReportingService.Models
{
    using System;

    using Microsoft.WindowsAzure.Storage.Table;

    public class EventHistoryTableEntity : TableEntity
    {
        public EventHistoryTableEntity(DateTime created, string eventType, string operationName, Guid? organizationId, Guid? subscriptionId, string data)
        {
            PartitionKey = Guid.NewGuid().ToString();
            RowKey = PartitionKey;
            Created = created;
            EventType = eventType;
            OperationName = operationName;
            OrganizationId = organizationId;
            SubscriptionId = subscriptionId;
            Data = data;
        }

        public EventHistoryTableEntity()
        {
        }

        public DateTime Created { get; set; }

        public string EventType { get; set; }

        public string OperationName { get; set; }

        public Guid? OrganizationId { get; set; }

        public Guid? SubscriptionId { get; set; }

        public string Data { get; set; }

        public override string ToString()
        {
            return $"{nameof(PartitionKey)}={PartitionKey}, {nameof(RowKey)}={RowKey}, {nameof(Created)}={Created}, {nameof(EventType)}={EventType}, {nameof(OperationName)}={OperationName}, {nameof(OrganizationId)}={OrganizationId}, {nameof(SubscriptionId)}={SubscriptionId}, {nameof(Data)}={Data}";
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((EventHistoryTableEntity)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = PartitionKey?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (RowKey?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ Created.GetHashCode();
                hashCode = (hashCode * 397) ^ (EventType?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (OperationName?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (OrganizationId?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (SubscriptionId?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Data?.GetHashCode() ?? 0);

                return hashCode;
            }
        }

        private bool Equals(EventHistoryTableEntity other)
        {
            return string.Equals(PartitionKey, other.PartitionKey)
                   && string.Equals(RowKey, other.RowKey)
                   && Created.Equals(other.Created)
                   && string.Equals(EventType, other.EventType)
                   && string.Equals(OperationName, other.OperationName)
                   && OrganizationId.Equals(other.OrganizationId)
                   && SubscriptionId.Equals(other.SubscriptionId)
                   && string.Equals(Data, other.Data);
        }
    }
}
