namespace EventReportingService.Monitoring
{
    using System;

    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Extensibility;

    // ReSharper disable once ClassNeverInstantiated.Global
    public class AppInsightsDependencyTracker : IDependencyTracker
    {
        private static string key = TelemetryConfiguration.Active.InstrumentationKey = GetAppInsightsKey();

        private static TelemetryClient telemetryClient;

        public AppInsightsDependencyTracker()
        {
            telemetryClient = new TelemetryClient() { InstrumentationKey = key };
        }

        public void TrackDependency(string dependencyName, string commandName, DateTime startTime, TimeSpan duration, bool isSuccess)
        {
            telemetryClient.TrackDependency(dependencyName, commandName, startTime, duration, isSuccess);
        }

        private static string GetAppInsightsKey()
        {
            return Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY", EnvironmentVariableTarget.Process);
        }
    }
}