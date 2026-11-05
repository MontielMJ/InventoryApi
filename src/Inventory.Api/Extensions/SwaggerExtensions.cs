using Microsoft.OpenApi;
namespace Inventory.Api.Extensions;
public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Inventory API",
                Version = "v1",
                Description = "Inventory management API built with .NET 10, Entity Framework Core, Dapper, CQRS and OAuth2."
            });
            options.AddSecurityDefinition("oauth2", BuildOAuth2Scheme(configuration));
            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("oauth2", document)] = []
            });
        });
        return services;
    }
    private static OpenApiSecurityScheme BuildOAuth2Scheme(IConfiguration configuration)
    {
        return new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Description = "Keycloak authorization code flow with PKCE",
            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = new Uri(Require(configuration, "Swagger:AuthorizationUrl")),
                    TokenUrl = new Uri(Require(configuration, "Swagger:TokenUrl")),
                    Scopes = new Dictionary<string, string>
                    {
                        ["openid"] = "OpenID Connect scope",
                        ["profile"] = "User profile",
                        ["email"] = "User email"
                    }
                }
            }
        };
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
