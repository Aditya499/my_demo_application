# Multi-stage Docker build for ASP.NET Core 8
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and projects for layer caching
COPY my_demo_application.sln ./
COPY my_demo_application.Domain/my_demo_application.Domain.csproj my_demo_application.Domain/
COPY my_demo_application.Application/my_demo_application.Application.csproj my_demo_application.Application/
COPY my_demo_application.Infrastructure/my_demo_application.Infrastructure.csproj my_demo_application.Infrastructure/
COPY my_demo_application/my_demo_application.csproj my_demo_application/

RUN dotnet restore my_demo_application.sln

# Copy all files and build
COPY . .
WORKDIR /src/my_demo_application
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime Image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Expose default HTTP port
ENV ASPNETCORE_ENVIRONMENT=Production
ENV USE_SQLITE=true
ENV PORT=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "my_demo_application.dll"]
