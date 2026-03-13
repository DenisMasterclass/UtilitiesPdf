using Microsoft.Extensions.DependencyInjection;
using Utils.Repositories;

namespace Utils.DependencyInjection
{
    public static class ConfigurationExtension
    {
        public static IServiceCollection AddPdfReaders(this IServiceCollection services)
        {
            services.AddScoped<IRepository, Repository>();

            return services;
        }
    }
}
