namespace EventReportingService.Storage
{
    using Microsoft.WindowsAzure.Storage.Table;

    public interface IStorageFactory<TEntity>
        where TEntity : TableEntity, new()
    {
        IStorage<TEntity> Create(CloudTable cloudTable);
    }
}
