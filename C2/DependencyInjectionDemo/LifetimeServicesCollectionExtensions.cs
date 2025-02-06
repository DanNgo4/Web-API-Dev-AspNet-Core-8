using DependencyInjectionDemo.Interfaces;
using DependencyInjectionDemo.Services;

namespace DependencyInjectionDemo;

public static class LifetimeServicesCollectionExtensions
{
    public static IServiceCollection AddLifetimeServices(this IServiceCollection services)
    {
        // AddScoped() method indicating that the service is created once per client request and disposed of upon completion of the request
        services.AddScoped<IScopedService, ScopedService>();
        // AddTransient() method indicating that the service is created each time IT is requested and disposed of at the end of the request
        services.AddTransient<ITransientService, TransientService>();
        // AddSingleton() method indicating the service instance will be the same through application lifetime
        services.AddSingleton<ISingletonService, SingletonService>();

        return services;
    }
}
