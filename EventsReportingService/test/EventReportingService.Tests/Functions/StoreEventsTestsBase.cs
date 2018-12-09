namespace EventReportingService.Tests.Functions
{
    using System;

    using Xunit.Abstractions;

    public class StoreEventsTestsBase
    {
        protected static readonly Guid TestOrganizationId = Guid.Parse("4c5067a4-0893-4e9f-844e-bddda8a90971");
        protected static readonly Guid TestSubscriptionId = Guid.Parse("2d8949f0-0100-4b62-83b7-ef97729fc8b3");
        protected static readonly Guid TestSecondSubscriptionId = Guid.Parse("d3d8078b-0f6a-4aa0-86c1-1c3cfd16ddc9");
        protected static readonly Guid TestUserID = Guid.Parse("b8fcbfd2-1db0-4819-8cef-185858c0c876");
        protected static readonly DateTime TestCreateDate = DateTime.UtcNow.AddMonths(-1).Normalize();
        protected static readonly DateTime TestUpdateDate = DateTime.UtcNow.AddDays(-1).Normalize();
        protected static readonly DateTime TestNewEventDate = DateTime.UtcNow.Normalize();

        public StoreEventsTestsBase(ITestOutputHelper output)
        {
            Logger = new TestLogger(output);
        }

        protected TestLogger Logger { get; }
    }
}