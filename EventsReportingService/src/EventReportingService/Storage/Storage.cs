namespace EventReportingService.Storage
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.Storage.Table;
    using EventReportingService.Models;
    using EventReportingService.Monitoring;

    public class Storage<TEntity> : IStorage<TEntity>
        where TEntity : TableEntity, new()
    {
        private readonly CloudTable cloudTable;
        private readonly IDependencyTracker dependencyTracker;

        public Storage(CloudTable cloudTable, IDependencyTracker dependencyTracker)
        {
            this.cloudTable = cloudTable;
            this.dependencyTracker = dependencyTracker;
        }

        public TEntity FirstOrDefault(Expression<Func<TEntity, bool>> expression)
        {
            return cloudTable.CreateQuery<TEntity>().Where(expression).FirstOrDefault();
        }

        public async Task Insert(TEntity tableEntity)
        {
            var insertEntityTask = cloudTable.ExecuteAsync(TableOperation.Insert(tableEntity));

            await TrackTableStorageTask(async () => await insertEntityTask, nameof(TableOperation.Insert));
        }

        public async Task Replace(TEntity tableEntity)
        {
            var replaceEntityTask = cloudTable.ExecuteAsync(TableOperation.Replace(tableEntity));

            await TrackTableStorageTask(async () => await replaceEntityTask, nameof(TableOperation.Replace));
        }

        private async Task TrackTableStorageTask(Func<Task> task, string taskName)
        {
            var startTime = DateTime.Now;
            var timer = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                await task();

                dependencyTracker.TrackDependency(Dependencies.TableStorage, taskName, startTime, timer.Elapsed, true);
            }
            catch (Exception)
            {
                dependencyTracker.TrackDependency(Dependencies.TableStorage, taskName, startTime, timer.Elapsed, false);

                throw;
            }
        }
    }
}
