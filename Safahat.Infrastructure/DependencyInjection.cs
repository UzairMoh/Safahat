using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Safahat.Infrastructure.Data.Context;
using Safahat.Infrastructure.Repositories.Implementations;
using Safahat.Infrastructure.Repositories.Interfaces;

namespace Safahat.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment = null)
    {
        // Only register DbContext if NOT in Testing environment
        if (environment?.IsEnvironment("Testing") != true)
        {
            services.AddDbContext<SafahatDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(SafahatDbContext).Assembly.FullName)));
        }
            
        // Always register repositories (they're needed for both production and testing)
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();
            
        return services;
    }
}