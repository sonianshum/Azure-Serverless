namespace EventReportingService.Processors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Logging;

    using Newtonsoft.Json.Linq;

    using EventReportingService.Models;
    using EventReportingService.Storage;
    using EventReportingService.Utilities;

    public class StorageEventProcessor : BaseEventProcessor<StorageEvent>
    {
        private readonly Dictionary<string, IStorage<StorageEventTableEntity>> tables;

        public StorageEventProcessor(GridEvent<StorageEvent>[] gridEvents, Dictionary<string, IStorage<StorageEventTableEntity>> tables, ILogger logger)
            : base(gridEvents, logger)
        {
            this.tables = tables;
        }

        protected override async Task ProcessEvent(GridEvent<StorageEvent> gridEvent)
        {
            if (!IsValidEvent(gridEvent))
            {
                Logger.LogWarning("Ignoring unsupported event: Subject={Subject}, EventType={EventType}", gridEvent.Subject, gridEvent.EventType);
                return;
            }

            if (!tables.ContainsKey(gridEvent.Data.Collection))
            {
                Logger.LogWarning("Ignoring unsupported collection: {Collection}", gridEvent.Data.Collection);
                return;
            }

            var collectionId = GetCollectionId(gridEvent.Data);

            Logger.LogInformation(
                "Processing StorageEvent: Name={Name}, Collection={Collection}, Operation={Operation}, CollectionId={CollectionId}, UserId={UserId}",
                gridEvent.Data.Name,
                gridEvent.Data.Collection,
                gridEvent.Data.Operation,
                collectionId,
                gridEvent.Data.UserId);

            var tableEntity = GetTableEntity(gridEvent.Data.Collection, collectionId);

            if (tableEntity == null)
            {
                await CreateTableEntity(collectionId, gridEvent);
                return;
            }

            if (gridEvent.EventTime <= tableEntity.Modified)
            {
                Logger.LogWarning("Skipping superseded update: Name={Name}, Collection={Collection}.", gridEvent.Data.Name, gridEvent.Data.Collection);
                return;
            }

            await UpdateTableEntity(tableEntity, gridEvent);
        }

        private static bool IsValidEvent(GridEvent<StorageEvent> gridEvent)
        {
            return gridEvent.Subject == Services.ASupervisor
                && gridEvent.EventType == nameof(StorageEvent);
        }

        private static string GetCollectionId(StorageEvent storageEvent)
        {
            return storageEvent.Operation == StorageOperation.Delete
                ? TryParseGuid(storageEvent.Data, "StorageEvent.Data must be an id guid")
                : TryParseGuid(storageEvent.Data?.id, "StorageEvent.Data must contain an id guid property");
        }

        private static string TryParseGuid(dynamic id, string error)
        {
            string idString = id?.ToString();

            if (idString == null || !Guid.TryParse(idString, out var _))
            {
                throw new ValidationException($"{error} but was: {id}");
            }

            return idString;
        }

        private static string TransformModelData(JObject data, string collection)
        {
            const string SubscriptionsKey = "subscriptions";

            if (collection == Collections.AccountsV1 && data[SubscriptionsKey] is JObject oldSubscriptionModel)
            {
                data[SubscriptionsKey] = new JArray(oldSubscriptionModel.Properties().Select(s => s.Value));
            }

            return Json.SerializeObject(data);
        }

        private StorageEventTableEntity GetTableEntity(string collection, string entityId)
        {
            return tables[collection].FirstOrDefault(e =>
                e.PartitionKey == entityId
                && e.RowKey == entityId);
        }

        private async Task CreateTableEntity(string entityId, GridEvent<StorageEvent> gridEvent)
        {
            var deletedTime = gridEvent.Data.Operation == StorageOperation.Delete ? (DateTime?)gridEvent.EventTime : null;

            var data = gridEvent.Data.Operation == StorageOperation.Delete ? null : TransformModelData(gridEvent.Data.Data, gridEvent.Data.Collection);

            var entity = new StorageEventTableEntity(entityId, gridEvent.EventTime, gridEvent.EventTime, deletedTime, data);

            await tables[gridEvent.Data.Collection].Insert(entity);
        }

        private async Task UpdateTableEntity(StorageEventTableEntity entity, GridEvent<StorageEvent> gridEvent)
        {
            entity.Modified = gridEvent.EventTime;

            if (gridEvent.Data.Operation == StorageOperation.Delete)
            {
                entity.Deleted = gridEvent.EventTime;
            }
            else
            {
                entity.Data = TransformModelData(gridEvent.Data.Data, gridEvent.Data.Collection);
            }

            await tables[gridEvent.Data.Collection].Replace(entity);
        }
    }
}
