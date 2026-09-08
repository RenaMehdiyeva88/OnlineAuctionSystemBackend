using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OnlineAuctionSystem.Application.Common.Interfaces;
using OnlineAuctionSystem.Application.Common.Interfaces.Services;
using OnlineAuctionSystem.Infrastructure.BackgroundJobs;
using OnlineAuctionSystem.Infrastructure.Identity;
using OnlineAuctionSystem.Infrastructure.Notifications;
using OnlineAuctionSystem.Infrastructure.Services;

namespace OnlineAuctionSystem.Infrastructure.DependencyInjection
{
    public static class InfrastructureServiceRegistration
    {
        // Called from WebApi's Program.cs: services.AddInfrastructure(configuration);
        // Persistence (DbContext, repositories, UnitOfWork) is registered separately
        // via OnlineAuctionSystem.Persistence's AddPersistence(configuration).
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Identity
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();

            // Services
            services.AddSingleton<IDateTime, DateTimeService>();

            // Notifications (F3, F5)
            services.AddSignalR();
            services.AddScoped<INotificationService, SignalRNotificationService>();

            // Background jobs (F4)
            services.AddHostedService<AuctionAutoCloseService>();

            return services;
        }
    }
}
