using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OnlineAuctionSystem.Application.Common.Behaviours;
using OnlineAuctionSystem.Infrastructure.DependencyInjection;
using OnlineAuctionSystem.Persistence.DependencyInjection;
using System.Reflection;
using System.Security.Claims;
using System.Text;

namespace OnlineAuctionSystem.Presentation.Extensions
{
    public static class ServiceCollectionExtensions
    {
        // ============================================================
        // APPLICATION
        // ============================================================

        public static IServiceCollection AddApplicationLayer(
            this IServiceCollection services)
        {
            var applicationAssembly =
                Assembly.Load("OnlineAuctionSystem.Application");

            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(applicationAssembly));

            services.AddAutoMapper(applicationAssembly);

            services.AddValidatorsFromAssembly(applicationAssembly);

            services.AddTransient(
                typeof(IPipelineBehavior<,>),
                typeof(ValidationBehaviour<,>));

            return services;
        }

        // ============================================================
        // PERSISTENCE + INFRASTRUCTURE
        // ============================================================

        public static IServiceCollection AddPersistenceAndInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddPersistence(configuration);

            services.AddInfrastructure(configuration);

            return services;
        }

        // ============================================================
        // JWT AUTHENTICATION
        // ============================================================

        public static IServiceCollection AddJwtAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var jwtKey = configuration["Jwt:Key"];

            if (string.IsNullOrWhiteSpace(jwtKey))
            {
                throw new InvalidOperationException(
                    "Jwt:Key is missing from configuration.");
            }

            var jwtIssuer = configuration["Jwt:Issuer"];

            if (string.IsNullOrWhiteSpace(jwtIssuer))
            {
                throw new InvalidOperationException(
                    "Jwt:Issuer is missing from configuration.");
            }

            var jwtAudience = configuration["Jwt:Audience"];

            if (string.IsNullOrWhiteSpace(jwtAudience))
            {
                throw new InvalidOperationException(
                    "Jwt:Audience is missing from configuration.");
            }

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme =
                        JwtBearerDefaults.AuthenticationScheme;

                    options.DefaultChallengeScheme =
                        JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;

                    options.SaveToken = true;

                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            // ------------------------------------------------
                            // Signature
                            // ------------------------------------------------
                            ValidateIssuerSigningKey = true,

                            IssuerSigningKey =
                                new SymmetricSecurityKey(
                                    Encoding.UTF8.GetBytes(jwtKey)),

                            // ------------------------------------------------
                            // Issuer
                            // ------------------------------------------------
                            ValidateIssuer = true,

                            ValidIssuer = jwtIssuer,

                            // ------------------------------------------------
                            // Audience
                            // ------------------------------------------------
                            ValidateAudience = true,

                            ValidAudience = jwtAudience,

                            // ------------------------------------------------
                            // Lifetime
                            // ------------------------------------------------
                            ValidateLifetime = true,

                            ClockSkew = TimeSpan.Zero,

                            // ------------------------------------------------
                            // IMPORTANT:
                            // ASP.NET Core will use this claim
                            // for [Authorize(Roles = "Seller")]
                            // ------------------------------------------------
                            RoleClaimType = ClaimTypes.Role,

                            // User name claim
                            NameClaimType = ClaimTypes.NameIdentifier
                        };

                    // --------------------------------------------------------
                    // SignalR JWT support
                    // Browser WebSocket connections cannot always send
                    // Authorization header, so we allow access_token
                    // in the query string for /hubs/*
                    // --------------------------------------------------------
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken =
                                context.Request.Query["access_token"];

                            var path =
                                context.HttpContext.Request.Path;

                            if (!string.IsNullOrEmpty(accessToken) &&
                                path.StartsWithSegments("/hubs"))
                            {
                                context.Token = accessToken;
                            }

                            return Task.CompletedTask;
                        },

                        // ----------------------------------------------------
                        // Useful for debugging authentication problems
                        // ----------------------------------------------------
                        OnAuthenticationFailed = context =>
                        {
                            Console.WriteLine(
                                $"JWT Authentication failed: " +
                                $"{context.Exception.Message}");

                            return Task.CompletedTask;
                        },

                        OnTokenValidated = context =>
                        {
                            Console.WriteLine(
                                "JWT token successfully validated.");

                            var user = context.Principal;

                            if (user != null)
                            {
                                Console.WriteLine(
                                    $"User: {user.Identity?.Name}");

                                Console.WriteLine(
                                    $"Roles: {string.Join(
                                        ", ",
                                        user.Claims
                                            .Where(c =>
                                                c.Type == ClaimTypes.Role)
                                            .Select(c => c.Value))}");
                            }

                            return Task.CompletedTask;
                        }
                    };
                });

            // Authorization
            services.AddAuthorization();

            return services;
        }

        // ============================================================
        // CORS
        // ============================================================

        public static IServiceCollection AddCorsPolicy(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("Default", policy =>
                {
                    var allowedOrigins =
                        configuration
                            .GetSection("Cors:AllowedOrigins")
                            .Get<string[]>();

                    policy
                        .WithOrigins(
                            allowedOrigins ??
                            Array.Empty<string>())
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            return services;
        }
    }
}