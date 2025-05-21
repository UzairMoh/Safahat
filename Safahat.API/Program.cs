using Safahat.Extensions;
using Safahat.Application;
using Safahat.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Basic services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Extended configurations from extension methods
builder.Services.AddSwaggerConfiguration();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorisationPolicies();
builder.Services.AddCorsConfiguration();

// Project-specific services
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication(); 

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowReactApp");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();