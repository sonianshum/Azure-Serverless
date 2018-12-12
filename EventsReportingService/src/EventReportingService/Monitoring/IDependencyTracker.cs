namespace EventReportingService.Monitoring
{
    using System;

    public interface IDependencyTracker
    {
        void TrackDependency(string dependencyName, string commandName, DateTime startTime, TimeSpan duration, bool isSuccess);
    }
}