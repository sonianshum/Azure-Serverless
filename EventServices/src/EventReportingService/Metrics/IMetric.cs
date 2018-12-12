namespace EventReportingService.Metrics
{
    using System.Threading.Tasks;

    public interface IMetric
    {
        Task Update();
    }
}
