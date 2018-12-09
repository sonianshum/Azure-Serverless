namespace EventReportingService.Models
{
    using System.Collections.Generic;

    public static class Services
    {
        public const string AccountSupervisor = "AccountSupervisor";

        public const string ClientProxy2fa = "2faClientProxy";

        public const string IariSupervisor = "IariSupervisor";

        public const string ConnectSupervisor = "ConnectSupervisor";

        public static readonly List<string> All = new List<string> { AccountSupervisor, ClientProxy2fa, IariSupervisor, ConnectSupervisor };
    }
}
