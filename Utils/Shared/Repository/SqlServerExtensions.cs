
using Microsoft.Extensions.DependencyInjection;

namespace Utils.Shared.Repository
{
    public static class SqlServerExtensions
    {
        public static IServiceCollection AddSqlServerDeLimaIt(this IServiceCollection services, string connectionString, string sqlTimeoutKey)
        {
            services
                .AddScoped(_ => new DataContext(connectionString));
            return services;
        }
    }
}
