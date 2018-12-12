namespace EventReportingService.Models
{
    using System.Collections.Generic;

    public static class Services
    {
        public const string ASupervisor = "ASupervisor";

        public const string faClientProxy = "faClientProxy";

        public const string ISupervisor = "ISupervisor";

        public const string CSupervisor = "CSupervisor";

        public static readonly List<string> All = new List<string> { ASupervisor, faClientProxy, ISupervisor, CSupervisor };
    }
}
