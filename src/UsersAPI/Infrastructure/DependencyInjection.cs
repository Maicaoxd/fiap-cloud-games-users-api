using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UsersAPI.Application.Abstractions.Messaging;
using UsersAPI.Application.Abstractions.Persistence;
using UsersAPI.Application.Abstractions.Security;
using UsersAPI.Health;
using UsersAPI.Infrastructure.Messaging;
using UsersAPI.Infrastructure.Persistence;
using UsersAPI.Infrastructure.Persistence.Repositories;
using UsersAPI.Infrastructure.Security;

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

            services.AddScoped<IDatabaseHealthChecker, DatabaseHealthChecker>();
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
            services
                .AddOptions<RabbitMqOptions>()
                .Bind(configuration.GetSection(RabbitMqOptions.SectionName))
                .Validate(options => !string.IsNullOrWhiteSpace(options.Host), "RabbitMq:Host is required.")
                .Validate(options => options.Port is > 0 and <= 65535, "RabbitMq:Port must be between 1 and 65535.")
                .Validate(options => !string.IsNullOrWhiteSpace(options.VirtualHost), "RabbitMq:VirtualHost is required.")
                .Validate(options => !string.IsNullOrWhiteSpace(options.Username), "RabbitMq:Username is required.")
                .Validate(options => !string.IsNullOrWhiteSpace(options.Password), "RabbitMq:Password is required.")
                .ValidateOnStart();

            services.AddSingleton<IRabbitMqConnectionChecker, RabbitMqConnectionChecker>();
            services.AddScoped<IUserCreatedEventPublisher, MassTransitUserCreatedEventPublisher>();

            services.AddMassTransit(x =>
            {
                x.SetKebabCaseEndpointNameFormatter();
                x.UsingRabbitMq((context, cfg) =>
                {
                    var rabbitMqOptions = context.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
                    var virtualHostPath = rabbitMqOptions.VirtualHost == "/"
                        ? string.Empty
                        : Uri.EscapeDataString(rabbitMqOptions.VirtualHost.TrimStart('/'));

                    var hostAddress = new UriBuilder("rabbitmq", rabbitMqOptions.Host, rabbitMqOptions.Port, virtualHostPath).Uri;

                    cfg.Host(hostAddress, h =>
                    {
                        h.Username(rabbitMqOptions.Username);
                        h.Password(rabbitMqOptions.Password);
                    });

                    cfg.ConfigureEndpoints(context);
                });
            });
        }
    }
}

