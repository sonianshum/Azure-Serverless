namespace UserActionsFunctionApp.DependencyInjection
{
    using System;
    using Microsoft.Azure.WebJobs.Host;
    using Microsoft.Azure.WebJobs.Host.Config;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;    

    /// <summary>
    /// Standard class for setting up DependencyInjection for Azure Functions
    /// </summary>
    /// <typeparam name="TAttribute">Attribute Type used for parameter injection</typeparam>
    public abstract class ServiceProviderExtensionConfig<TAttribute>
        where TAttribute : Attribute
    {
        private static IConfigurationRoot Configuration { get; set; }

        /// <summary>
        /// Initialize the IServiceProvider and add binding rule for the function framework. This is called by the framework.
        /// </summary>
        /// <param name="context">Context for function execution</param>
        public virtual void Initialize(ExtensionConfigContext context)
        {
            var path = System.Reflection.Assembly.GetExecutingAssembly().Location.Split(new[] { @"\bin" }, StringSplitOptions.None)[0];

            var builder = new ConfigurationBuilder()
            .SetBasePath(path)
            .AddJsonFile("settings.json");
            Configuration = builder.Build();

            IServiceCollection services = new ServiceCollection();
            services.AddDbContext<DatabaseContext>(options => options.UseSqlServer(Configuration["Storage:ConnectionString"]));
            RegisterServices(services);
            IServiceProvider serviceProvider = services.BuildServiceProvider(true);

            context
                .AddBindingRule<TAttribute>()
                .Bind(new InjectBindingProvider(serviceProvider));

            IExtensionRegistry registry = context.Config.GetService<IExtensionRegistry>();
            ScopeCleanupFilter filter = new ScopeCleanupFilter();
            registry.RegisterExtension(typeof(IFunctionInvocationFilter), filter);
            registry.RegisterExtension(typeof(IFunctionExceptionFilter), filter);
        }

        /// <summary>
        /// Registers services for dependency injection
        /// </summary>
        /// <param name="services">Services collection to register services into</param>
        protected abstract void RegisterServices(IServiceCollection services);
    }
}
