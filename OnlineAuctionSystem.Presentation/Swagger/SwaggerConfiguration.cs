using Microsoft.OpenApi.Models;

namespace OnlineAuctionSystem.Presentation.Swagger
{
    public static class SwaggerConfiguration
    {
        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();

            // Was previously just AddSwaggerGen() with no JWT scheme — the
            // "Authorize" button never appeared, so protected endpoints
            // (bidding, creating auctions, closing auctions, seller
            // dashboard, etc.) couldn't be exercised from Swagger UI at all.
            services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Paste just the access token (no \"Bearer \" prefix needed) — Swagger adds it automatically."
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            return services;
        }
    }
}
