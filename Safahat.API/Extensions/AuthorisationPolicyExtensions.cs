namespace Safahat.Extensions;

public static class AuthorisationPolicyExtensions
{
    public static IServiceCollection AddAuthorisationPolicies(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy("AdminOnly", policy => 
                policy.RequireRole("Admin"))
            .AddPolicy("AuthenticatedUser", policy => 
                policy.RequireAuthenticatedUser())
            .AddPolicy("ResourceOwnerOrAdmin", policy =>
                policy.RequireAssertion(context =>
                {
                    if (context.User.IsInRole("Admin"))
                        return true;
                    return false;
                }));
                
        return services;
    }
}