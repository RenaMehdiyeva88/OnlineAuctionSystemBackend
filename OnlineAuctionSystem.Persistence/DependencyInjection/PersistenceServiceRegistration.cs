using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OnlineAuctionSystem.Application.Common.Interfaces.Persistence;
using OnlineAuctionSystem.Persistence.Context;
using OnlineAuctionSystem.Persistence.Repositories;

namespace OnlineAuctionSystem.Persistence.DependencyInjection
{

    public static class PersistenceServiceRegistration
    {
        // Called from WebApi's Program.cs: services.AddPersistence(configuration);
        public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AuctionDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IAuctionRepository, AuctionRepository>();
            services.AddScoped<IBidRepository, BidRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
