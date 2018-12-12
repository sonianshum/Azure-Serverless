namespace EventReportingService.Storage
{
    using Microsoft.WindowsAzure.Storage.Table;
    using EventReportingService.Monitoring;

    public class StorageFactory<TEntity> : IStorageFactory<TEntity>
        where TEntity : TableEntity, new()
    {
        private readonly IDependencyTracker dependencyTracker;

        public StorageFactory(IDependencyTracker dependencyTracker)
        {
            this.dependencyTracker = dependencyTracker;
        }

        public IStorage<TEntity> Create(CloudTable cloudTable)
        {
            return new Storage<TEntity>(cloudTable, dependencyTracker);
        }
    }
}
