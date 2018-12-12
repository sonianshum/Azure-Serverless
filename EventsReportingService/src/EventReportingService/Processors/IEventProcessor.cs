namespace EventReportingService.Processors
{
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    public interface IEventProcessor
    {
        ILogger Logger { get; }

        Task Run();
    }
}