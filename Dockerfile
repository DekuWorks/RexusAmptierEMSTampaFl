# Use the official .NET 9.0 runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the official .NET 9.0 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY ["RexusOps360.API/RexusOps360.API.csproj", "RexusOps360.API/"]
RUN dotnet restore "RexusOps360.API/RexusOps360.API.csproj"

# Copy the rest of the source code
COPY . .
WORKDIR "/src/RexusOps360.API"

# Build the application
RUN dotnet build "RexusOps360.API.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "RexusOps360.API.csproj" -c Release -o /app/publish

# Build the final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80;https://+:443

# Create a non-root user
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

ENTRYPOINT ["dotnet", "RexusOps360.API.dll"] 