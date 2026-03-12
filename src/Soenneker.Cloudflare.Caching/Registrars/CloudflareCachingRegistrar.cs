using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.Cloudflare.Caching.Abstract;
using Soenneker.Cloudflare.Utils.Client.Registrars;

namespace Soenneker.Cloudflare.Caching.Registrars;

/// <summary>
/// Registrar for Cloudflare caching services
/// </summary>
public static class CloudflareCachingRegistrar
{
    /// <summary>
    /// Registers Cloudflare caching services with the service collection
    /// </summary>
    /// <param name="services">The service collection to register with</param>
    public static IServiceCollection AddCloudflareCachingUtilAsSingleton(this IServiceCollection services)
    {
        services.AddCloudflareClientUtilAsSingleton().TryAddSingleton<ICloudflareCachingUtil, CloudflareCachingUtil>();

        return services;
    }

    public static IServiceCollection AddCloudflareCachingUtilAsScoped(this IServiceCollection services)
    {
        services.AddCloudflareClientUtilAsSingleton().TryAddScoped<ICloudflareCachingUtil, CloudflareCachingUtil>();

        return services;
    }
}