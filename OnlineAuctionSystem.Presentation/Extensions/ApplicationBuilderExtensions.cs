using OnlineAuctionSystem.Infrastructure.Notifications;
using OnlineAuctionSystem.Presentation.Middleware;

namespace OnlineAuctionSystem.Presentation.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static WebApplication ConfigurePipeline(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();

                // Redirect root URL to Swagger
                app.MapGet("/", context =>
                {
                    context.Response.Redirect(
                        "/swagger/index.html",
                        permanent: false);

                    return Task.CompletedTask;
                });
            }

            // Global exception handling
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            // HTTPS
            app.UseHttpsRedirection();

            // Static files: /wwwroot/uploads/*
            app.UseStaticFiles();

            // CORS
            app.UseCors("Default");

            // Authentication MUST come before Authorization
            app.UseAuthentication();

            // Authorization
            app.UseAuthorization();

            // API controllers
            app.MapControllers();

            // SignalR
            app.MapHub<NotificationHub>("/hubs/notifications");

            return app;
        }
    }
}