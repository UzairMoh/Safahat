using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Safahat.Application.Interfaces;
using Safahat.Application.Services;

namespace Safahat.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register AutoMapper
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
            
        // Register services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPostService, PostService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ITagService, TagService>();
            
        // Register validators
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            
        return services;
    }
}