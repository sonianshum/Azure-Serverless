namespace EventReportingService.Tests
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using Microsoft.WindowsAzure.Storage.Table;

    using Shouldly;

    using EventReportingService.Storage;
    using EventReportingService.Utilities;

    public class TestStorage<TEntity> : IStorage<TEntity>, IStorageFactory<TEntity>
        where TEntity : TableEntity, new()
    {
        private readonly ConcurrentDictionary<string, TEntity> entities = new ConcurrentDictionary<string, TEntity>();

        public IStorage<TEntity> Create(CloudTable cloudTable)
        {
            return this;
        }

        public TEntity FirstOrDefault(Expression<Func<TEntity, bool>> expression)
        {
            return entities.Values.AsQueryable().Where(expression).FirstOrDefault();
        }

        public async Task Insert(TEntity tableEntity)
        {
            await Task.CompletedTask;

            entities.TryAdd(tableEntity.RowKey, tableEntity);
        }

        public async Task Replace(TEntity tableEntity)
        {
            await Task.CompletedTask;

            entities.TryRemove(tableEntity.RowKey, out TEntity _);

            entities.TryAdd(tableEntity.RowKey, tableEntity);
        }

        public void ShouldContain(Expression<Func<TEntity, bool>> expression)
        {
            entities.Values.ShouldContain(expression, $"Matching table entity not found in {Json.SerializeObject(entities.Values)}");
        }

        public void Delete(Guid id)
        {
            entities.TryRemove(id.ToString(), out TEntity _);
        }
    }
}
