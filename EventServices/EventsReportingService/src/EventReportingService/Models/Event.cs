namespace EventReportingService.Models
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class Event<T>
    {
        public Event(Guid? organizationId, Guid subscriptionId, Guid? userId, T eventData)
        {
            OrganizationId = organizationId;
            SubscriptionId = subscriptionId;
            UserId = userId;
            EventData = eventData;
        }

        [JsonProperty]
        public Guid? OrganizationId { get; private set; }

        [JsonProperty]
        public Guid SubscriptionId { get; private set; }

        [JsonProperty]
        public Guid? UserId { get; private set; }

        [JsonProperty]
        public T EventData { get; private set; }

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

            return Equals((Event<T>)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = SubscriptionId.GetHashCode();
                hashCode = (hashCode * 397) ^ (OrganizationId?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (UserId?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (EventData?.GetHashCode() ?? 0);

                return hashCode;
            }
        }

        private bool Equals(Event<T> other)
        {
            return OrganizationId.Equals(other.OrganizationId)
                   && SubscriptionId.Equals(other.SubscriptionId)
                   && UserId.Equals(other.UserId)
                   && EqualityComparer<T>.Default.Equals(EventData, other.EventData);
        }
    }
}