using Localizr.Application.Common.Abstractions;
using Localizr.Application.Common.Contracts;
using Localizr.Application.Common.Pipeline.Validation;
using Localizr.Application.Identity.Abstractions;
using Localizr.Application.Identity.Handlers;
using Localizr.Application.Identity.Validators;
using Localizr.Application.Localization.Abstractions;
using Localizr.Infrastructure.Common.Repository;
using Localizr.Infrastructure.Common.UnitOfWork;
using Localizr.Infrastructure.Identity.Models;
using Localizr.Infrastructure.Identity.Options;
using Localizr.Infrastructure.Identity.Services;
using Localizr.Infrastructure.Localization.Options;
using Localizr.Infrastructure.Localization.Services;
using Localizr.Infrastructure.Persistence;
using Localizr.Infrastructure.Persistence.Middlewares;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Localizr.Composition.Extensions;

/// <summary>Adds Localizr services to the web host.</summary>
public static class WebApplicationBuilderExtensions
{
    extension(WebApplicationBuilder builder)
    {
        /// <summary>Runs the Localizr web application.</summary>
        /// <typeparam name="TProgram">The program type.</typeparam>
        /// <typeparam name="TApp">The root component.</typeparam>
        /// <returns>A task for application startup.</returns>
        public async Task RunLocalizrAsync<TProgram, TApp>(Action<WebApplicationBuilder> configureMudBlazor)
            where TProgram : class
            where TApp : IComponent
        {
            builder.Configuration.AddEnvironmentVariables();
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();
            builder.Services.AddControllers();
            builder.Services.AddSignalR();
            builder.Services.AddOpenApi();
            configureMudBlazor?.Invoke(builder);
            builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
            builder.Services.Configure<GoogleTranslateOptions>(builder.Configuration.GetSection(GoogleTranslateOptions.SectionName));

            builder.Services.AddDbContext<LocalizrDbContext>(options => options.UseSqlServer(
                builder.Configuration.GetConnectionString("SQLServer"),
                sql => sql.CommandTimeout(90)));

            builder.Services.AddIdentityCore<User>(options =>
                {
                    options.User.RequireUniqueEmail = true;
                    options.SignIn.RequireConfirmedEmail = false;
                    options.Password.RequiredLength = 8;
                    options.Password.RequireDigit = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireNonAlphanumeric = true;
                    options.Lockout.MaxFailedAccessAttempts = 5;
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                }).AddRoles<Role>()
                    .AddEntityFrameworkStores<LocalizrDbContext>()
                    .AddSignInManager()
                    .AddDefaultTokenProviders();

            builder.Services.AddScoped<IIdentityService, IdentityService>();
            builder.Services.AddSingleton<IRevokedTokenStore, RevokedTokenStore>();
            builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
            builder.Services.AddScoped<IIdentityEmailSender, LoggingIdentityEmailSender>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            builder.Services.AddHttpClient<ITranslatorService, TranslatorService>((serviceProvider, client) =>
            {
                GoogleTranslateOptions options = serviceProvider
                    .GetRequiredService<IOptions<GoogleTranslateOptions>>()
                    .Value;

                if (!Uri.TryCreate(options.Endpoint, UriKind.Absolute, out Uri? endpoint))
                {
                    throw new InvalidOperationException(
                        "Google translation endpoint must be a valid absolute URI.");
                }

                client.BaseAddress = endpoint;
            });
            builder.Services.AddValidatorsFromAssemblyContaining<RegisterCommandValidator>();
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    JwtOptions jwt = builder.Configuration
                        .GetSection(JwtOptions.SectionName)
                        .Get<JwtOptions>()
                        ?? throw new InvalidOperationException("JWT configuration is missing.");

                    if (Encoding.UTF8.GetByteCount(jwt.Key) < 32)
                    {
                        throw new InvalidOperationException("Jwt:Key must contain 256 bits.");
                    }

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                        ValidateIssuer = true,
                        ValidIssuer = jwt.Issuer,
                        ValidateAudience = true,
                        ValidAudience = jwt.Audience,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromSeconds(30)
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = context =>
                        {
                            JwtSecurityToken? token = context.SecurityToken as JwtSecurityToken;
                            string? tokenType = token?.Claims.FirstOrDefault(
                                claim => claim.Type == JwtRegisteredClaimNames.Typ)?.Value;

                            if (!string.Equals(tokenType, "access", StringComparison.Ordinal))
                            {
                                context.Fail("The token is not an access token.");
                                return Task.CompletedTask;
                            }

                            if (token is not null && context.HttpContext.RequestServices
                                .GetRequiredService<IRevokedTokenStore>().IsRevoked(token.Id))
                            {
                                context.Fail("The access token has been revoked.");
                            }

                            return Task.CompletedTask;
                        }
                    };
                });

            builder.Services.AddAuthorizationBuilder()
                .AddPolicy(IdentityPolicies.Administrator, policy => policy.RequireClaim(
                    IdentityClaimTypes.Permission, "system.admin"))
                .AddPolicy(IdentityPolicies.User, policy => policy.RequireClaim(
                    IdentityClaimTypes.Permission, "system.user"));

            builder.Services.AddMediator(options =>
            {
                options.ServiceLifetime = ServiceLifetime.Scoped;
                options.Assemblies = [typeof(IdentityHandlers).Assembly];
                options.PipelineBehaviors =
                [
                    typeof(ValidationMiddleware<,>),
                    typeof(TransactionMiddleware<,>)
                ];
            });

            await builder.Build().RunLocalizrAsync<TApp>();
        }
    }
}
