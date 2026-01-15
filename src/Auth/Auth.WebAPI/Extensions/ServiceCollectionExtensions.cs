using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Auth.Core.Interfaces.Domain;
using Auth.Core.Interfaces.Application;
using Auth.Infrastructure.Services;
using Auth.Infrastructure.Data;
using MediatR;
using Serilog;
using Serilog.Events;

namespace Auth.WebAPI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSerilogConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            var tamañoMaximoMB = configuration.GetValue<int?>("TamanhoMaximoLogEnMegas") ?? 10;
            
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Microservicio", "Auth")
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
                .WriteTo.File(
                    path: $"Logs/Auth-{DateTime.Now:yyyyMMdd}.txt",
                    rollingInterval: RollingInterval.Infinite,
                    fileSizeLimitBytes: tamañoMaximoMB * 1024 * 1024,
                    rollOnFileSizeLimit: true,
                    retainedFileCountLimit: int.MaxValue,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
                )
                .CreateLogger();

            services.AddSerilog();

            return services;
        }

        public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Auth.API",
                    Version = "v1",
                    Description = "Microservicio de Autenticación",
                    Contact = new OpenApiContact
                    {
                        Name = "Alexis Lael Lara Rodriguez",
                        Email = "efestos.736@gmail.com"
                    }
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme. \\r\\n\\r\\n Enter 'Bearer' [space] and then your token in the text input below.\\r\\n\\r\\nExample: \\\"Bearer 1safsfsdfdfd\\\""
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                        new string[] {}
                    }
                });
            });

            return services;
        }

        public static IServiceCollection AddAuthServices(this IServiceCollection services, IConfiguration configuration)
        {
            // MediatR
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Auth.Application.Handlers.LoginCommandHandler).Assembly));

            // JWT Configuration
            var jwtSettings = configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey)),
                        ValidateIssuer = true,
                        ValidIssuer = issuer,
                        ValidateAudience = true,
                        ValidAudience = audience,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };

                    // Eventos para diagnóstico
                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            Log.Error("Token authentication failed: {Exception}, Token: {Token}",
                                context.Exception.Message,
                                context.Request.Headers["Authorization"].ToString());
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = context =>
                        {
                            var name = context.Principal?.FindFirst("unique_name")?.Value;
                            var roles = string.Join(",", context.Principal?.FindAll("roles").Select(c => c.Value) ?? Enumerable.Empty<string>());
                            Log.Information("JWT validado. usuario={User} roles={Roles}", name, roles);
                            return Task.CompletedTask;
                        },
                        OnChallenge = context =>
                        {
                            var authHeader = context.Request.Headers["Authorization"].ToString();
                            Log.Warning("Token challenge - Error: {Error}, ErrorDescription: {ErrorDescription}, AuthHeader: {AuthHeader}, HasToken: {HasToken}",
                                context.Error ?? "null",
                                context.ErrorDescription ?? "null",
                                string.IsNullOrEmpty(authHeader) ? "Missing" : "Present",
                                !string.IsNullOrEmpty(authHeader));
                            return Task.CompletedTask;
                        },
                        OnMessageReceived = context =>
                        {
                            var token = context.Request.Headers["Authorization"].ToString();
                            Log.Information("Token received: {HasToken}, Value: {Token}",
                                !string.IsNullOrEmpty(token),
                                string.IsNullOrEmpty(token) ? "None" : token.Substring(0, Math.Min(20, token.Length)) + "...");
                            return Task.CompletedTask;
                        }
                    };
                });

            // Dependency Injection
            var connectionString = configuration.GetConnectionString("AuthDB");
            services.AddScoped<IUnitOfWork>(provider => new DapperUnitOfWork(connectionString));
            services.AddScoped<ITokenService>(provider => new TokenService(secretKey, issuer, audience));

            return services;
        }

        public static IServiceCollection AddCorsConfiguration(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });

                options.AddPolicy("Production", policy =>
                {
                    policy.WithOrigins("https://com", "https://app.com")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });

            return services;
        }

        public static IServiceCollection AddHealthChecksConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHealthChecks();
            return services;
        }

        public static void UseSerilogRequestLogging(this IApplicationBuilder app)
        {
            app.UseSerilogRequestLogging(options =>
            {
                options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
                options.GetLevel = (httpContext, elapsed, ex) => ex != null
                    ? LogEventLevel.Error
                    : httpContext.Response.StatusCode > 499
                        ? LogEventLevel.Error
                        : LogEventLevel.Information;
            });
        }
    }
}
