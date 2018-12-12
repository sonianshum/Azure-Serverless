namespace EventReportingService.Storage
{
    using System;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using Microsoft.WindowsAzure.Storage.Table;

    public interface IStorage<TEntity>
        where TEntity : TableEntity, new()
    {
        TEntity FirstOrDefault(Expression<Func<TEntity, bool>> expression);

        Task Insert(TEntity tableEntity);

        Task Replace(TEntity tableEntity);
    }
}
