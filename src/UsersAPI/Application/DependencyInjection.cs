using UsersAPI.Application.Users.Authenticate;
using UsersAPI.Application.Users.ChangePassword;
using UsersAPI.Application.Users.Deactivate;
using UsersAPI.Application.Users.ForgotPassword;
using UsersAPI.Application.Users.List;
using UsersAPI.Application.Users.Register;
using UsersAPI.Application.Users.Update;
using UsersAPI.Application.Users.UpdateCurrent;

namespace UsersAPI.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            AddUserUseCases(services);

            return services;
        }

        private static void AddUserUseCases(IServiceCollection services)
        {
            services.AddScoped<AuthenticateUserUseCase>();
            services.AddScoped<ChangePasswordUseCase>();
            services.AddScoped<DeactivateUserUseCase>();
            services.AddScoped<ForgotPasswordUseCase>();
            services.AddScoped<ListUsersUseCase>();
            services.AddScoped<RegisterUserUseCase>();
            services.AddScoped<UpdateUserUseCase>();
            services.AddScoped<UpdateCurrentUserUseCase>();
        }
    }
}
