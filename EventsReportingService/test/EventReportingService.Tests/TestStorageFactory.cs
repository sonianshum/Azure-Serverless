namespace EventReportingService.Tests
{
    using System.Collections.Generic;

    using Microsoft.WindowsAzure.Storage.Table;

    using EventReportingService.Storage;

    public class TestStorageFactory<TEntity> : IStorageFactory<TEntity>
        where TEntity : TableEntity, new()
    {
        private readonly Dictionary<string, IStorage<TEntity>> storageTables = new Dictionary<string, IStorage<TEntity>>();

        public void SetStorage(CloudTable cloudTable, IStorage<TEntity> storage)
        {
            storageTables.Add(cloudTable.Name, storage);
        }

        public IStorage<TEntity> Create(CloudTable cloudTable)
        {
            return storageTables[cloudTable.Name];
        }
    }
}
