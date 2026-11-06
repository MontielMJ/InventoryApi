using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
namespace Inventory.Api.Extensions;
public static class AuthenticationExtensions
{
    public static IServiceCollection AddApiAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var authority = Require(configuration, "Auth:Authority");
        var issuer = Require(configuration, "Auth:Issuer");
        var audience = Require(configuration, "Auth:Audience");
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = authority;
                options.Audience = audience;
                options.RequireHttpsMetadata = false;
                options.MetadataAddress = $"{authority.TrimEnd('/')}/.well-known/openid-configuration";
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });
        services.AddAuthorization();
        services.AddCors(options => options.AddPolicy("Frontend", policy => policy
            .WithOrigins(configuration["Frontend:Origin"] ?? "http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()));
        return services;
    }
    private static string Require(IConfiguration configuration, string key)
    {
        var value = configuration[key];
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Configuration value '{key}' is required and must be provided through environment variables.");
        }
        return value;
    }
}
