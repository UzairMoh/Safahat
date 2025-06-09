# Use .NET 8 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution file
COPY *.sln .

# Copy project files (adjust these paths based on your actual project structure)
COPY Safahat.API/*.csproj ./Safahat.API/
COPY Safahat.Application/*.csproj ./Safahat.Application/
COPY Safahat.Infrastructure/*.csproj ./Safahat.Infrastructure/
COPY Safahat.Models/*.csproj ./Safahat.Models/
COPY Safahat.Common/*.csproj ./Safahat.Common/
COPY Safahat.Tests/*.csproj ./Safahat.Tests/

# Restore dependencies
RUN dotnet restore

# Copy all source code
COPY . .

# Build the application
WORKDIR /src/Safahat.API
RUN dotnet build -c Release -o /app/build

# Publish the application
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Use .NET runtime for the final image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published files
COPY --from=build /app/publish .

# Expose port and configure environment
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Start the application
ENTRYPOINT ["dotnet", "Safahat.API.dll"]