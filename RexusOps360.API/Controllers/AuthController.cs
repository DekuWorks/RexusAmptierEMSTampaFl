/*
 * RexusOps360 EMS API - Authentication Controller
 * 
 * This controller handles all authentication and authorization operations
 * including user login, registration, token management, and profile operations.
 * 
 * Features:
 * - JWT-based authentication
 * - User registration and profile management
 * - Password reset functionality
 * - Token refresh and validation
 * - Role-based access control
 * 
 * Security:
 * - Rate limiting on login attempts
 * - Password hashing with SHA256
 * - JWT token expiration
 * - IP address tracking for security
 * 
 * Author: RexusOps360 Development Team
 * Version: 1.0.0
 * Last Updated: 2025-01-17
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RexusOps360.API.Models;
using RexusOps360.API.Services;
using System.Security.Claims;

namespace RexusOps360.API.Controllers
{
    /// <summary>
    /// Authentication and Authorization Controller
    /// Handles user authentication, registration, and profile management
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Create guest access token for emergency use
        /// </summary>
        /// <returns>JWT token for guest access with limited permissions</returns>
        /// <response code="200">Guest access token generated successfully</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("guest-access")]
        public async Task<IActionResult> CreateGuestAccess()
        {
            try
            {
                // Get client IP address for security tracking
                var ipAddress = GetClientIpAddress();
                
                // Generate guest access token
                var response = await _authService.CreateGuestAccessAsync(ipAddress);

                if (response.Success)
                {
                    // Return guest token with limited permissions
                    return Ok(response);
                }
                else
                {
                    // Return error response
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                // Log the error for debugging and monitoring
                _logger.LogError(ex, "Error in guest access endpoint");
                
                // Return generic error to avoid information leakage
                return StatusCode(500, new ApiResponse<LoginResponse>
                {
                    Success = false,
                    Message = "An internal server error occurred"
                });
            }
        }

        /// <summary>
        /// Authenticate user and generate JWT token
        /// </summary>
        /// <param name="request">Login credentials (username/password)</param>
        /// <returns>JWT token and user information on successful authentication</returns>
        /// <response code="200">Login successful - returns token and user data</response>
        /// <response code="400">Invalid credentials or validation error</response>
        /// <response code="429">Too many login attempts - rate limited</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                // Get client IP address for rate limiting and security tracking
                var ipAddress = GetClientIpAddress();
                
                // Attempt user authentication with rate limiting
                var response = await _authService.LoginAsync(request, ipAddress);

                if (response.Success)
                {
                    // Return JWT token and user information
                    return Ok(response);
                }
                else
                {
                    // Return authentication error (invalid credentials, rate limited, etc.)
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                // Log the error for debugging and monitoring
                _logger.LogError(ex, "Error in login endpoint");
                
                // Return generic error to avoid information leakage
                return StatusCode(500, new ApiResponse<LoginResponse>
                {
                    Success = false,
                    Message = "An internal server error occurred"
                });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var ipAddress = GetClientIpAddress();
                var response = await _authService.RegisterAsync(request, ipAddress);

                if (response.Success)
                {
                    return Ok(response);
                }
                else
                {
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in register endpoint");
                return StatusCode(500, new ApiResponse<LoginResponse>
                {
                    Success = false,
                    Message = "An internal server error occurred"
                });
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var token = GetTokenFromHeader();
                var ipAddress = GetClientIpAddress();
                var response = await _authService.LogoutAsync(token, ipAddress);

                if (response.Success)
                {
                    return Ok(response);
                }
                else
                {
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in logout endpoint");
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An internal server error occurred"
                });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var ipAddress = GetClientIpAddress();
                var response = await _authService.RefreshTokenAsync(request.RefreshToken, ipAddress);

                if (response.Success)
                {
                    return Ok(response);
                }
                else
                {
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in refresh token endpoint");
                return StatusCode(500, new ApiResponse<LoginResponse>
                {
                    Success = false,
                    Message = "An internal server error occurred"
                });
            }
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }

                var response = await _authService.ChangePasswordAsync(userId.Value, request);

                if (response.Success)
                {
                    return Ok(response);
                }
                else
                {
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in change password endpoint");
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An internal server error occurred"
                });
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                var response = await _authService.ResetPasswordAsync(request);

                if (response.Success)
                {
                    return Ok(response);
                }
                else
                {
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in reset password endpoint");
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An internal server error occurred"
                });
            }
        }

        [HttpPost("reset-password-confirm")]
        public async Task<IActionResult> ResetPasswordConfirm([FromBody] ResetPasswordConfirmRequest request)
        {
            try
            {
                var response = await _authService.ResetPasswordConfirmAsync(request);

                if (response.Success)
                {
                    return Ok(response);
                }
                else
                {
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in reset password confirm endpoint");
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An internal server error occurred"
                });
            }
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new ApiResponse<User>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }

                var response = await _authService.GetUserProfileAsync(userId.Value);

                if (response.Success)
                {
                    return Ok(response);
                }
                else
                {
                    return NotFound(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in get current user endpoint");
                return StatusCode(500, new ApiResponse<User>
                {
                    Success = false,
                    Message = "An internal server error occurred"
                });
            }
        }

        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new ApiResponse<User>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }

                var response = await _authService.UpdateUserProfileAsync(userId.Value, request);

                if (response.Success)
                {
                    return Ok(response);
                }
                else
                {
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in update profile endpoint");
                return StatusCode(500, new ApiResponse<User>
                {
                    Success = false,
                    Message = "An internal server error occurred"
                });
            }
        }

        [HttpPost("validate-token")]
        public async Task<IActionResult> ValidateToken([FromBody] ValidateTokenRequest request)
        {
            try
            {
                var response = await _authService.ValidateTokenAsync(request.Token);

                if (response.Success)
                {
                    return Ok(response);
                }
                else
                {
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in validate token endpoint");
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An internal server error occurred"
                });
            }
        }

        [HttpPost("revoke-token")]
        [Authorize]
        public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequest request)
        {
            try
            {
                var ipAddress = GetClientIpAddress();
                var response = await _authService.RevokeTokenAsync(request.Token, ipAddress);

                if (response.Success)
                {
                    return Ok(response);
                }
                else
                {
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in revoke token endpoint");
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An internal server error occurred"
                });
            }
        }

        #region Helper Methods

        private string? GetClientIpAddress()
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString() ??
                   HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault() ??
                   HttpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
        }

        private string GetTokenFromHeader()
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                throw new InvalidOperationException("Invalid authorization header");
            }

            return authHeader.Substring("Bearer ".Length);
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }

            return null;
        }

        #endregion
    }

    public class ValidateTokenRequest
    {
        public string Token { get; set; } = string.Empty;
    }

    public class RevokeTokenRequest
    {
        public string Token { get; set; } = string.Empty;
    }
} 