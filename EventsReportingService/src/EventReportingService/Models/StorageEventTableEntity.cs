namespace EventReportingService.Models
{
    using System;

    using Microsoft.WindowsAzure.Storage.Table;

    public class StorageEventTableEntity : TableEntity
    {
        public StorageEventTableEntity(string id, DateTime? created, DateTime modified, DateTime? deleted, string data)
        {
            PartitionKey = id;
            RowKey = id;
            Created = created;
            Modified = modified;
            Deleted = deleted;
            Data = data;
        }

        public StorageEventTableEntity()
        {
        }

        public DateTime? Created { get; set; }

        public DateTime Modified { get; set; }

        public DateTime? Deleted { get; set; }

        public string Data { get; set; }

        public override string ToString()
        {
            return $"{nameof(PartitionKey)}={PartitionKey}, {nameof(RowKey)}={RowKey}, {nameof(Created)}={Created}, {nameof(Modified)}={Modified}, {nameof(Deleted)}={Deleted}, {nameof(Data)}={Data}";
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

            return Equals((StorageEventTableEntity)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = PartitionKey?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (RowKey?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Created?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ Modified.GetHashCode();
                hashCode = (hashCode * 397) ^ (Deleted?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Data?.GetHashCode() ?? 0);

                return hashCode;
            }
        }

        private bool Equals(StorageEventTableEntity other)
        {
            return string.Equals(PartitionKey, other.PartitionKey)
                   && string.Equals(RowKey, other.RowKey)
                   && Created.Equals(other.Created)
                   && Modified.Equals(other.Modified)
                   && Deleted.Equals(other.Deleted)
                   && string.Equals(Data, other.Data);
        }
    }
}
