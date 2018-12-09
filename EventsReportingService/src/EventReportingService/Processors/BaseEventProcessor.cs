namespace EventReportingService.Processors
{
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Logging;

    using EventReportingService.Models;

    public abstract class BaseEventProcessor<TEvent> : IEventProcessor
    {
        private readonly GridEvent<TEvent>[] gridEvents;

        protected BaseEventProcessor(GridEvent<TEvent>[] gridEvents, ILogger logger)
        {
            this.gridEvents = gridEvents;
            Logger = logger;
        }

        public ILogger Logger { get; }

        public async Task Run()
        {
            if (gridEvents == null || !gridEvents.Any())
            {
                throw new ValidationException("No Grid Events found.");
            }

            foreach (var gridEvent in gridEvents)
            {
                await ProcessEvent(gridEvent);
            }
        }

        protected abstract Task ProcessEvent(GridEvent<TEvent> gridEvent);
    }
}
