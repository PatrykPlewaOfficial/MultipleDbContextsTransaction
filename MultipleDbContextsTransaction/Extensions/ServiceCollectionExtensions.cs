using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MultipleDbContextsTransaction.Contexts;

namespace MultipleDbContextsTransaction.Extensions
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDbConnectionProvider(this IServiceCollection services, string connectionString)
        {
            services.AddScoped<IConnectionWrapper>(_ => new ConnectionWrapper(connectionString));

            return services;
        }

        public static IServiceCollection AddUnitOfWork(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork>(x =>
            {
                var dbContexts = x.GetServices<ISharedTransactionDbContext>().Cast<DbContext>(); // ToDo: improve that
                return new UnitOfWork(dbContexts);
            });

            services.AddScoped(typeof(IUnitOfWork<>), typeof(UnitOfWork<>));

            return services;
        }

        public static IServiceCollection AddDbContextWithSharedTransaction<TDbContext>(this IServiceCollection services, Func<DbContextOptions<TDbContext>, TDbContext> createFn)
            where TDbContext : DbContext, ISharedTransactionDbContext
        {
            services.AddScoped<TDbContext>(x =>
            {
                var options = new DbContextOptionsBuilder<TDbContext>()
                    .UseSqlServer(x.GetRequiredService<IConnectionWrapper>().Connection)
                    .Options;

                return createFn(options);
            });

            services.AddScoped<ISharedTransactionDbContext>(x => x.GetRequiredService<TDbContext>());

            return services;
        }
    }
}
