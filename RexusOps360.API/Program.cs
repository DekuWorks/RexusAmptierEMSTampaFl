using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RexusOps360.API.Services;
using RexusOps360.API.Hubs;
using RexusOps360.API.Data;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add SignalR
builder.Services.AddSignalR();

// Add HttpClient for SystemIntegrationService
builder.Services.AddHttpClient();

// Add Database Context
builder.Services.AddDbContext<EmsDbContext>(options =>
{
    var environment = builder.Environment.EnvironmentName;
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    
    // Use in-memory database for testing and CI environments
    if (environment == "Test" || environment == "CI" || string.IsNullOrEmpty(connectionString))
    {
        options.UseInMemoryDatabase("EmsTampaDb");
    }
    else
    {
        // Use SQL Server for development and production
        options.UseSqlServer(connectionString);
    }
});

// Add Services
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IGpsTrackingService, GpsTrackingService>();
builder.Services.AddScoped<IShiftSchedulingService, ShiftSchedulingService>();
builder.Services.AddScoped<IIncidentClusteringService, IncidentClusteringService>();
builder.Services.AddScoped<IHotspotDetectionService, HotspotDetectionService>();
builder.Services.AddScoped<ISystemIntegrationService, SystemIntegrationService>();

// Register Authentication Services
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Register SaaS Services
builder.Services.AddScoped<ISaasService, SaasService>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with JWT authentication
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "RexusOps360 EMS API", Version = "v1" });
    
    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

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

// Add JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "RexusOps360",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "RexusOps360Users",
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "YourSuperSecretKeyHere12345678901234567890")),
            ClockSkew = TimeSpan.Zero
        };
    });

// Add Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("DispatcherOrAdmin", policy => policy.RequireRole("Admin", "Dispatcher"));
    options.AddPolicy("ResponderOrAdmin", policy => policy.RequireRole("Admin", "Responder"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "RexusOps360 EMS API V1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at root
        c.DocumentTitle = "RexusOps360 EMS API Documentation";
    });
}

app.UseHttpsRedirection();

// Serve static files
app.UseStaticFiles();

// Use CORS
app.UseCors("AllowAll");

// Add custom middleware
app.UseRequestLogging();
app.UseRateLimiting();

// Use Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Map SignalR hub
app.MapHub<EmsHub>("/emsHub");

// Health check endpoint
app.MapGet("/health", () => new { status = "healthy", timestamp = DateTime.UtcNow });

// Ensure database is created and migrations are applied
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<EmsDbContext>();
        context.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        // Log the error but don't fail the application startup
        Console.WriteLine($"Database initialization error: {ex.Message}");
    }
}

app.Run();
