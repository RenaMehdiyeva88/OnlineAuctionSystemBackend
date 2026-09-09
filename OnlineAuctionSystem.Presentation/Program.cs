using OnlineAuctionSystem.Application.Common.Interfaces.Services;
using OnlineAuctionSystem.Persistence.Context;
using OnlineAuctionSystem.Persistence.Seed;
using OnlineAuctionSystem.Presentation.Extensions;
using OnlineAuctionSystem.Presentation.Filters;
using OnlineAuctionSystem.Presentation.Swagger;

var builder = WebApplication.CreateBuilder(args);

// --- Layer registration ---
builder.Services.AddApplicationLayer();
builder.Services.AddPersistenceAndInfrastructure(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddCorsPolicy(builder.Configuration);

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
});

builder.Services.AddSwaggerDocumentation();

var app = builder.Build();

// --- Seed database (categories + demo users) on startup ---
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    var seederLogger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseSeeder");
    await DatabaseSeeder.SeedAsync(context, passwordHasher, seederLogger);
}

app.ConfigurePipeline();
app.Run();