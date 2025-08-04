# Multi-stage build for .NET 6 application
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy project files
COPY ["RexusOps360.API.csproj", "./"]
RUN dotnet restore "RexusOps360.API.csproj"

# Copy source code
COPY . .
RUN dotnet build "RexusOps360.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "RexusOps360.API.csproj" -c Release -o /app/publish

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create non-root user for security
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

ENTRYPOINT ["dotnet", "RexusOps360.API.dll"] 