/*
 * RexusOps360 EMS API - Main Program Entry Point
 * 
 * This file configures the ASP.NET Core application for the Emergency Management System.
 * It sets up dependency injection, authentication, CORS, Swagger documentation,
 * and configures the application pipeline.
 * 
 * Key Features:
 * - JWT Authentication with Bearer tokens
 * - Entity Framework Core with SQL Server/In-Memory database
 * - SignalR for real-time communication
 * - Swagger/OpenAPI documentation
 * - CORS policy for frontend integration
 * - Comprehensive service registration
 * 
 * Author: RexusOps360 Development Team
 * Version: 1.0.0
 * Last Updated: 2025-01-17
 */

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RexusOps360.API.Services;
using RexusOps360.API.Hubs;
using RexusOps360.API.Data;
using System.Text;

// Initialize the ASP.NET Core application builder
var builder = WebApplication.CreateBuilder(args);

// =============================================================================
// SERVICE REGISTRATION - Dependency Injection Container Setup
// =============================================================================

// Register MVC controllers for API endpoints
builder.Services.AddControllers();

// Register SignalR for real-time communication (WebSocket support)
// Used for live incident updates, GPS tracking, and notifications
builder.Services.AddSignalR();

// Register HttpClient for external API integrations
// Used by SystemIntegrationService for weather, geocoding, etc.
builder.Services.AddHttpClient();

// =============================================================================
// DATABASE CONFIGURATION - Entity Framework Core Setup
// =============================================================================

// Register the main database context for EMS data
// Supports both SQL Server (production) and In-Memory (testing) databases
builder.Services.AddDbContext<EmsDbContext>(options =>
{
    var environment = builder.Environment.EnvironmentName;
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    
    // Use in-memory database for testing and CI environments
    // This allows for fast testing without external database dependencies
    if (environment == "Test" || environment == "CI" || string.IsNullOrEmpty(connectionString))
    {
        options.UseInMemoryDatabase("EmsTampaDb");
    }
    else
    {
        // Use SQL Server for development and production environments
        // Provides full database features and data persistence
        options.UseSqlServer(connectionString);
    }
});

// =============================================================================
// APPLICATION SERVICES - Business Logic Layer Registration
// =============================================================================

// Core EMS Services - Incident and Emergency Management
builder.Services.AddScoped<IAuditService, AuditService>();                    // Audit logging for compliance
builder.Services.AddScoped<INotificationService, NotificationService>();      // Real-time notifications
builder.Services.AddScoped<IGpsTrackingService, GpsTrackingService>();       // GPS location tracking
builder.Services.AddScoped<IShiftSchedulingService, ShiftSchedulingService>(); // Shift management
builder.Services.AddScoped<IIncidentClusteringService, IncidentClusteringService>(); // Incident analysis
builder.Services.AddScoped<IHotspotDetectionService, HotspotDetectionService>(); // Hotspot detection
builder.Services.AddScoped<ISystemIntegrationService, SystemIntegrationService>(); // External API integration
builder.Services.AddScoped<IRealTimeTrackingService, RealTimeTrackingService>(); // Real-time incident tracking
builder.Services.AddScoped<IRecaptchaService, RecaptchaService>(); // reCAPTCHA verification

// Authentication & Authorization Services
builder.Services.AddScoped<IJwtService, JwtService>();                        // JWT token generation/validation
builder.Services.AddScoped<IAuthService, AuthService>();                      // User authentication/authorization

// SaaS & Multi-tenant Services - REMOVED

// Event Management Services
builder.Services.AddScoped<IEventManagementService, EventManagementService>(); // Event planning and management

// =============================================================================
// CORS CONFIGURATION - Cross-Origin Resource Sharing
// =============================================================================

// Configure CORS policy for frontend integration
// Allows the React/Angular frontend to communicate with the API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()      // Allow requests from any origin
              .AllowAnyMethod()       // Allow all HTTP methods (GET, POST, PUT, DELETE)
              .AllowAnyHeader();      // Allow all headers (including Authorization)
    });
});

// =============================================================================
// SWAGGER/OPENAPI CONFIGURATION - API Documentation
// =============================================================================

// Enable API documentation and testing interface
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with JWT authentication support
// Provides interactive API documentation at /swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "RexusOps360 EMS API", Version = "1.0" });
    
    // Add JWT Bearer token authentication to Swagger UI
    // Allows testing authenticated endpoints directly from the documentation
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Require Bearer token for all endpoints in Swagger
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// =============================================================================
// JWT AUTHENTICATION CONFIGURATION - Bearer Token Authentication
// =============================================================================

// Configure JWT Bearer token authentication for secure API access
// Provides stateless authentication for mobile and web clients
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,           // Validate the token issuer
            ValidateAudience = true,          // Validate the token audience
            ValidateLifetime = true,          // Validate token expiration
            ValidateIssuerSigningKey = true,  // Validate the signing key
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "RexusOps360",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "RexusOps360Users",
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "YourSuperSecretKeyHere12345678901234567890")),
            ClockSkew = TimeSpan.Zero        // No clock skew tolerance for security
        };
    });

// =============================================================================
// AUTHORIZATION POLICIES - Role-Based Access Control (RBAC)
// =============================================================================

// Configure authorization policies for role-based access control
// Defines which roles can access specific endpoints and features
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));                    // Admin-only features
    options.AddPolicy("DispatcherOrAdmin", policy => policy.RequireRole("Admin", "Dispatcher")); // Dispatcher and admin features
    options.AddPolicy("ResponderOrAdmin", policy => policy.RequireRole("Admin", "Responder"));   // Responder and admin features
});

// =============================================================================
// APPLICATION BUILD AND PIPELINE CONFIGURATION
// =============================================================================

// Build the ASP.NET Core application
var app = builder.Build();

// Configure the HTTP request pipeline (middleware stack)
// Order is important - middleware executes in the order it's added

// Development-specific middleware
if (app.Environment.IsDevelopment())
{
    // Enable Swagger UI for API documentation and testing
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "RexusOps360 EMS API V1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at root URL
        c.DocumentTitle = "RexusOps360 EMS API Documentation";
    });
}

// Security and Infrastructure Middleware
// app.UseHttpsRedirection(); // Disabled for development - enable in production

// Static file serving (for frontend files, images, etc.)
app.UseStaticFiles();

// CORS middleware - must be called before routing
app.UseCors("AllowAll");

// Custom middleware (commented out for future implementation)
// app.UseRequestLogging(); // Request/response logging middleware
// app.UseRateLimiting();   // Rate limiting middleware

// Authentication & Authorization middleware
// Must be called before endpoint routing
app.UseAuthentication();  // JWT token validation
app.UseAuthorization();   // Role-based access control

// =============================================================================
// ENDPOINT MAPPING AND APPLICATION STARTUP
// =============================================================================

// Map API controllers (REST endpoints)
app.MapControllers();

// Map SignalR hub for real-time communication
// Enables WebSocket connections for live updates
app.MapHub<EmsHub>("/emsHub");

// Health check endpoint for monitoring and load balancers
// Returns application status and timestamp
app.MapGet("/health", () => new { status = "healthy", timestamp = DateTime.UtcNow });

// =============================================================================
// DATABASE INITIALIZATION - Ensure Database is Ready
// =============================================================================

// Initialize database on application startup
// Creates database and applies migrations if needed
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<EmsDbContext>();
        context.Database.EnsureCreated(); // Creates database if it doesn't exist
    }
    catch (Exception ex)
    {
        // Log the error but don't fail the application startup
        // This allows the app to start even if database is unavailable
        Console.WriteLine($"Database initialization error: {ex.Message}");
    }
}

// =============================================================================
// APPLICATION STARTUP - Run the Application
// =============================================================================

// Start the ASP.NET Core application
// This begins listening for HTTP requests
app.Run();
