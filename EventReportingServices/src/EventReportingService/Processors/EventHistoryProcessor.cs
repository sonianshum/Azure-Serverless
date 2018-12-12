namespace EventReportingService.Processors
{
    using System;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Logging;

    using Newtonsoft.Json.Linq;

    using EventReportingService.Models;
    using EventReportingService.Storage;
    using EventReportingService.Utilities;

    public class EventHistoryProcessor : BaseEventProcessor<JObject>
    {
        private readonly IStorage<EventHistoryTableEntity> eventsStorage;

        public EventHistoryProcessor(GridEvent<JObject>[] gridEvents, IStorage<EventHistoryTableEntity> eventsStorage, ILogger logger)
            : base(gridEvents, logger)
        {
            this.eventsStorage = eventsStorage;
        }

        protected override async Task ProcessEvent(GridEvent<JObject> gridEvent)
        {
            if (!IsValidEvent(gridEvent))
            {
                Logger.LogWarning("Ignoring unsupported event: Subject={Subject}, EventType={EventType}", gridEvent.Subject, gridEvent.EventType);
                return;
            }

            Logger.LogInformation("Processing Event {EventType} from {Subject}", gridEvent.EventType, gridEvent.Subject);

            var tableEntity = CreateTableEntity(gridEvent);

            await eventsStorage.Insert(tableEntity);
        }

        private static bool IsValidEvent(GridEvent<JObject> gridEvent)
        {
            return Services.All.Contains(gridEvent.Subject);
        }

        private static EventHistoryTableEntity CreateTableEntity(GridEvent<JObject> gridEvent)
        {
            var eventData = Json.SerializeObject(gridEvent);

            return new EventHistoryTableEntity(gridEvent.EventTime, gridEvent.EventType, DataToJObject("name")?.ToString(), DataToGuid("organizationId"), DataToGuid("subscriptionId"), eventData);

            JToken DataToJObject(string name) => gridEvent.Data.GetValue(name, StringComparison.OrdinalIgnoreCase);

            Guid? DataToGuid(string name) => DataToJObject(name)?.ToObject<Guid?>();
        }
    }
}
