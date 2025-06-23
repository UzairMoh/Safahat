namespace Safahat.Extensions;

public static class CorsExtensions
{
    public static IServiceCollection AddCorsConfiguration(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowReactApp", builder =>
            {
                builder
                    .WithOrigins(
                        "https://safahat-client-production.up.railway.app"
                    )
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });
        
        return services;
    }
}