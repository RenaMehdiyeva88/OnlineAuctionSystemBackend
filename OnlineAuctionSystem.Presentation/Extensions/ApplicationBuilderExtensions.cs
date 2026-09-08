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
                    context.Response.Redirect("/swagger/index.html", permanent: false);
                    return Task.CompletedTask;
                });
            }

            app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.UseHttpsRedirection();
            app.UseCors("Default");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.MapHub<NotificationHub>("/hubs/notifications");

            return app;
        }
    }
}
