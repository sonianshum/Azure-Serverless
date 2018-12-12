namespace EventReportingService.Models
{
    using System;

    public class GridEvent<T>
    {
        public GridEvent(Guid id, DateTime eventTime, string subject, string eventType, T data)
        {
            Id = id.ToString();
            EventTime = eventTime;
            Subject = subject;
            EventType = eventType;
            Data = data;
        }

        public string Id { get; }

        public DateTime EventTime { get; }

        public string Subject { get; }

        public string EventType { get; }

        public T Data { get; }

        public override string ToString()
        {
            return $"{nameof(Id)}={Id}, {nameof(EventTime)}={EventTime}, {nameof(Subject)}={Subject}, {nameof(EventType)}={EventType}, {nameof(Data)}={Data}";
        }
    }
}
