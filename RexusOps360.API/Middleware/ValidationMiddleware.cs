using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RexusOps360.API.Models;
using System.ComponentModel.DataAnnotations;

namespace RexusOps360.API.Middleware
{
    public class ValidationMiddleware : IAsyncActionFilter
    {
        private readonly ILogger<ValidationMiddleware> _logger;

        public ValidationMiddleware(ILogger<ValidationMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Check if the model state is valid
            if (!context.ModelState.IsValid)
            {
                var errors = new List<string>();
                
                foreach (var modelState in context.ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }

                var response = new ApiResponse<object>
                {
                    Success = false,
                    Message = "Validation failed",
                    Errors = errors
                };

                context.Result = new BadRequestObjectResult(response);
                return;
            }

            await next();
        }
    }

    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter> _logger;
        private readonly IWebHostEnvironment _environment;

        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger, IWebHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }

        public void OnException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, "An unhandled exception occurred");

            var response = new ApiResponse<object>
            {
                Success = false,
                Message = _environment.IsDevelopment() 
                    ? context.Exception.Message 
                    : "An internal server error occurred",
                Errors = _environment.IsDevelopment() 
                    ? new List<string> { context.Exception.StackTrace ?? "No stack trace available" }
                    : new List<string>()
            };

            context.Result = new ObjectResult(response)
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }

    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var startTime = DateTime.UtcNow;
            var requestPath = context.Request.Path;
            var requestMethod = context.Request.Method;
            var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            _logger.LogInformation("Request started: {Method} {Path} from {ClientIp}", 
                requestMethod, requestPath, clientIp);

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Request failed: {Method} {Path} from {ClientIp}", 
                    requestMethod, requestPath, clientIp);
                throw;
            }
            finally
            {
                var duration = DateTime.UtcNow - startTime;
                _logger.LogInformation("Request completed: {Method} {Path} from {ClientIp} in {Duration}ms", 
                    requestMethod, requestPath, clientIp, duration.TotalMilliseconds);
            }
        }
    }

    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private readonly Dictionary<string, RateLimitInfo> _rateLimitStore = new();
        private readonly object _lockObject = new();

        public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var endpoint = context.Request.Path.Value ?? "";

            // Skip rate limiting for health checks and static files
            if (endpoint.StartsWith("/health") || endpoint.StartsWith("/swagger") || 
                context.Request.Path.StartsWithSegments("/static"))
            {
                await _next(context);
                return;
            }

            if (IsRateLimited(clientIp, endpoint))
            {
                _logger.LogWarning("Rate limit exceeded for {ClientIp} on {Endpoint}", clientIp, endpoint);
                
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.ContentType = "application/json";
                
                var response = new ApiResponse<object>
                {
                    Success = false,
                    Message = "Too many requests. Please try again later.",
                    Errors = new List<string> { "Rate limit exceeded" }
                };

                await context.Response.WriteAsJsonAsync(response);
                return;
            }

            await _next(context);
        }

        private bool IsRateLimited(string clientIp, string endpoint)
        {
            var key = $"{clientIp}:{endpoint}";
            var now = DateTime.UtcNow;

            lock (_lockObject)
            {
                if (_rateLimitStore.TryGetValue(key, out var info))
                {
                    // Clean up old entries
                    if (now > info.ResetTime)
                    {
                        _rateLimitStore.Remove(key);
                        info = new RateLimitInfo();
                    }
                    else
                    {
                        info.RequestCount++;
                        
                        // Rate limit: 100 requests per minute
                        if (info.RequestCount > 100)
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    info = new RateLimitInfo
                    {
                        RequestCount = 1,
                        ResetTime = now.AddMinutes(1)
                    };
                }

                _rateLimitStore[key] = info;
                return false;
            }
        }
    }

    public class RateLimitInfo
    {
        public int RequestCount { get; set; }
        public DateTime ResetTime { get; set; }
    }

    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestLoggingMiddleware>();
        }

        public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RateLimitingMiddleware>();
        }
    }
} 