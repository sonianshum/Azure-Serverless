namespace EventReportingService.Models
{
    using System;

    using Microsoft.WindowsAzure.Storage.Table;

    public class MetricTableEntity : TableEntity
    {
        public MetricTableEntity(DateTime created, DateTime updated, string product, string name, Guid? organizationId, Guid subscriptionId, string instance, long count)
        {
            PartitionKey = subscriptionId.ToString();
            RowKey = Guid.NewGuid().ToString();
            Created = created;
            Updated = updated;
            Product = product;
            Name = name;
            OrganizationId = organizationId;
            SubscriptionId = subscriptionId;
            Instance = instance;
            Count = count;
        }

        public MetricTableEntity()
        {
        }

        public DateTime Created { get; set; }

        public DateTime Updated { get; set; }

        public string Product { get; set; }

        public string Name { get; set; }

        public Guid? OrganizationId { get; set; }

        public Guid? SubscriptionId { get; set; }

        public string Instance { get; set; }

        public long Count { get; set; }

        public override string ToString()
        {
            return $"{nameof(PartitionKey)}={PartitionKey}, {nameof(RowKey)}={RowKey}, {nameof(Created)}={Created}, {nameof(Updated)}={Updated}, {nameof(Product)}={Product}, {nameof(Name)}={Name}, {nameof(OrganizationId)}={OrganizationId}, {nameof(SubscriptionId)}={SubscriptionId}, {nameof(Instance)}={Instance}, {nameof(Count)}={Count}";
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

            return Equals((MetricTableEntity)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = PartitionKey?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (RowKey?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ Created.GetHashCode();
                hashCode = (hashCode * 397) ^ Updated.GetHashCode();
                hashCode = (hashCode * 397) ^ (Product?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Name?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (OrganizationId?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (SubscriptionId?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Instance?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ Count.GetHashCode();
                return hashCode;
            }
        }

        private bool Equals(MetricTableEntity other)
        {
            return string.Equals(PartitionKey, other.PartitionKey)
                   && string.Equals(RowKey, other.RowKey)
                   && Created.Equals(other.Created)
                   && Updated.Equals(other.Updated)
                   && string.Equals(Product, other.Product)
                   && string.Equals(Name, other.Name)
                   && OrganizationId.Equals(other.OrganizationId)
                   && SubscriptionId.Equals(other.SubscriptionId)
                   && string.Equals(Instance, other.Instance)
                   && Count == other.Count;
        }
    }
}
