namespace EventReportingService.Processors
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    public static class ProcessorRunner
    {
        public static async Task<HttpResponseMessage> Run(IEventProcessor eventProcessor)
        {
            try
            {
                await eventProcessor.Run();
            }
            catch (ValidationException e)
            {
                eventProcessor.Logger.LogError($"Invalid request: {e.Message}");
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
            catch (Exception e)
            {
                eventProcessor.Logger.LogError($"An error occured: {e}");
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
