using Microsoft.Extensions.Configuration;

namespace RexusOps360.API.Configuration
{
    public class SecurityConfig
    {
        public JwtConfig Jwt { get; set; } = new();
        public PasswordConfig Password { get; set; } = new();
        public RateLimitConfig RateLimit { get; set; } = new();
        public SessionConfig Session { get; set; } = new();
        public CorsConfig Cors { get; set; } = new();
    }

    public class JwtConfig
    {
        public string Key { get; set; } = "YourSuperSecretKeyHere12345678901234567890";
        public string Issuer { get; set; } = "RexusOps360";
        public string Audience { get; set; } = "RexusOps360Users";
        public int ExpirationHours { get; set; } = 8;
        public int RefreshTokenExpirationDays { get; set; } = 30;
    }

    public class PasswordConfig
    {
        public int MinLength { get; set; } = 8;
        public int MaxLength { get; set; } = 100;
        public bool RequireUppercase { get; set; } = true;
        public bool RequireLowercase { get; set; } = true;
        public bool RequireDigit { get; set; } = true;
        public bool RequireSpecialCharacter { get; set; } = true;
        public int MaxFailedAttempts { get; set; } = 5;
        public int LockoutDurationMinutes { get; set; } = 15;
        public int PasswordHistoryCount { get; set; } = 5;
        public int PasswordExpirationDays { get; set; } = 90;
    }

    public class RateLimitConfig
    {
        public int RequestsPerMinute { get; set; } = 100;
        public int RequestsPerHour { get; set; } = 1000;
        public int BurstLimit { get; set; } = 10;
        public string[] ExemptPaths { get; set; } = { "/health", "/swagger" };
    }

    public class SessionConfig
    {
        public int IdleTimeoutMinutes { get; set; } = 30;
        public int AbsoluteTimeoutHours { get; set; } = 24;
        public bool SlidingExpiration { get; set; } = true;
        public string CookieName { get; set; } = "RexusOps360.Session";
        public bool HttpOnly { get; set; } = true;
        public bool Secure { get; set; } = true;
        public string SameSite { get; set; } = "Strict";
    }

    public class CorsConfig
    {
        public string[] AllowedOrigins { get; set; } = { "*" };
        public string[] AllowedMethods { get; set; } = { "GET", "POST", "PUT", "DELETE", "OPTIONS" };
        public string[] AllowedHeaders { get; set; } = { "*" };
        public bool AllowCredentials { get; set; } = true;
        public int MaxAgeSeconds { get; set; } = 86400;
    }

    public static class SecurityConfigExtensions
    {
        public static SecurityConfig GetSecurityConfig(this IConfiguration configuration)
        {
            var config = new SecurityConfig();
            
            configuration.GetSection("Security:Jwt").Bind(config.Jwt);
            configuration.GetSection("Security:Password").Bind(config.Password);
            configuration.GetSection("Security:RateLimit").Bind(config.RateLimit);
            configuration.GetSection("Security:Session").Bind(config.Session);
            configuration.GetSection("Security:Cors").Bind(config.Cors);
            
            return config;
        }
    }
} 