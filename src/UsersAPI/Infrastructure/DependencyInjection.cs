using UsersAPI.Application.Abstractions.Messaging;
using UsersAPI.Application.Abstractions.Persistence;
using UsersAPI.Application.Abstractions.Security;
using UsersAPI.Infrastructure.Messaging;
using UsersAPI.Infrastructure.Persistence;
using UsersAPI.Infrastructure.Persistence.Repositories;
using UsersAPI.Infrastructure.Security;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace UsersAPI.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            AddPersistence(services, configuration);
            AddSecurity(services, configuration);
            AddMessaging(services, configuration);

            return services;
        }

        private static void AddPersistence(IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection connection string was not configured.");

            services.AddDbContext<UsersDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddScoped<IUserRepository, UserRepository>();
        }

        private static void AddSecurity(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(JwtOptions.Create(configuration));
            services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
            services.AddScoped<IAccessTokenGenerator, JwtAccessTokenGenerator>();
        }

        private static void AddMessaging(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IUserCreatedEventPublisher, MassTransitUserCreatedEventPublisher>();

            services.AddMassTransit(x =>
            {
                x.SetKebabCaseEndpointNameFormatter();
                x.UsingRabbitMq((context, cfg) =>
                {
                    var host = configuration["RabbitMq:Host"] ?? "localhost";
                    var virtualHost = configuration["RabbitMq:VirtualHost"] ?? "/";
                    var username = configuration["RabbitMq:Username"] ?? "guest";
                    var password = configuration["RabbitMq:Password"] ?? "guest";

                    cfg.Host(host, virtualHost, h =>
                    {
                        h.Username(username);
                        h.Password(password);
                    });

                    cfg.ConfigureEndpoints(context);
                });
            });
        }
    }
}
