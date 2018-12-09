namespace UserActionsFunctionApp.DependencyInjection
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs.Host.Bindings;
    using Microsoft.Azure.WebJobs.Host.Protocols;
    using Microsoft.Extensions.DependencyInjection;

    public class InjectBinding : IBinding
    {
        private readonly Type type;
        private readonly IServiceProvider serviceProvider;

        public InjectBinding(IServiceProvider serviceProvider, Type type)
        {
            this.type = type;
            this.serviceProvider = serviceProvider;
        }

        public bool FromAttribute => true;

        public Task<IValueProvider> BindAsync(object value, ValueBindingContext context) =>
            Task.FromResult((IValueProvider)new InjectValueProvider(value));

        public async Task<IValueProvider> BindAsync(BindingContext context)
        {
            await Task.Yield();
            var scope = InjectBindingProvider.Scopes.GetOrAdd(context.FunctionInstanceId, (_) => serviceProvider.CreateScope());
            var value = scope.ServiceProvider.GetRequiredService(type);
            return await BindAsync(value, context.ValueContext);
        }

        public ParameterDescriptor ToParameterDescriptor() => new ParameterDescriptor();

        private class InjectValueProvider : IValueProvider
        {
            private readonly object value;

            public InjectValueProvider(object value) => this.value = value;

            public Type Type => value.GetType();

            public Task<object> GetValueAsync() => Task.FromResult(value);

            public string ToInvokeString() => value.ToString();
        }
    }
}
