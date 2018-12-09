
namespace UserActionsFunctionApp
{
    using Microsoft.Azure.WebJobs.Host.Config;
    using Microsoft.Extensions.DependencyInjection;       

    /// <summary>
    /// Initializes Dependency Injection
    /// </summary>
    public class Startup : ServiceProviderExtensionConfig<InjectAttribute>, IExtensionConfigProvider
    {
        /// <summary>
        /// Registers services for dependency injection
        /// </summary>
        /// <param name="services">Services collection to register services into</param>
        protected override void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<IStorageInitializer, SqlStorageInitializer>();            
            services.AddScoped<IUserStorage, UserStorage>();
            services.AddScoped<IClientFactory, ClientFactory>();
            services.AddScoped<Client>();
        }
    }
}
