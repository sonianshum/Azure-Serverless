namespace EventReportingService.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using Shouldly;
    using Xunit.Abstractions;

    public class TestLogger : ILogger
    {
        private readonly ITestOutputHelper output;
        private readonly List<KeyValuePair<LogLevel, string>> logMessages = new List<KeyValuePair<LogLevel, string>>();

        public TestLogger(ITestOutputHelper output)
        {
            this.output = output;
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var message = formatter != null ? formatter(state, exception) : state?.ToString();

            logMessages.Add(new KeyValuePair<LogLevel, string>(logLevel, message));

            output.WriteLine($"{logLevel}: {message}");
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        public void ShouldContainLogMessage(string message)
        {
            logMessages.Any(l => l.Value.StartsWith(message)).ShouldBeTrue($"Should contain message: {message}");
        }
    }
}
