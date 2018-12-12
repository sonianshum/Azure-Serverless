namespace EventReportingService.Extensions
{
    using System;
    using Microsoft.Azure.WebJobs.Host.Config;
    using Microsoft.Extensions.DependencyInjection;
    using EventReportingService.Bindings;
    using EventReportingService.Monitoring;
    using EventReportingService.Storage;

    public class InjectConfiguration : IExtensionConfigProvider
    {
        private IServiceProvider serviceProvider;

        public void Initialize(ExtensionConfigContext context)
        {
            var services = new ServiceCollection();
            RegisterServices(services);
            serviceProvider = services.BuildServiceProvider(true);

            context
                .AddBindingRule<InjectAttribute>()
                .BindToInput<dynamic>(i => serviceProvider.GetRequiredService(i.Type));
        }

        private void RegisterServices(IServiceCollection services)
        {
            services.AddTransient<IDependencyTracker, AppInsightsDependencyTracker>();
            services.AddTransient(typeof(IStorageFactory<>), typeof(StorageFactory<>));
        }
    }
}
