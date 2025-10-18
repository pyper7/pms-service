# Use the official .NET 9 runtime as base image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY ["PMS.API/PMS.API.csproj", "PMS.API/"]
COPY ["PMS.Application/PMS.Application.csproj", "PMS.Application/"]
COPY ["PMS.Domain/PMS.Domain.csproj", "PMS.Domain/"]
COPY ["PMS.Infrastructure/PMS.Infrastructure.csproj", "PMS.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "PMS.API/PMS.API.csproj"

# Copy everything else
COPY . .

# Build the application
WORKDIR "/src/PMS.API"
RUN dotnet build "PMS.API.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "PMS.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage/image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "PMS.API.dll"]
