using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RexusOps360.API.Models;
using System.Security.Cryptography;
using System.Text;

namespace RexusOps360.API.Services
{
    public interface IAuthService
    {
        Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request, string? ipAddress);
        Task<ApiResponse<LoginResponse>> RegisterAsync(RegisterRequest request, string? ipAddress);
        Task<ApiResponse<bool>> LogoutAsync(string token, string? ipAddress);
        Task<ApiResponse<LoginResponse>> RefreshTokenAsync(string refreshToken, string? ipAddress);
        Task<ApiResponse<bool>> ChangePasswordAsync(int userId, ChangePasswordRequest request);
        Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordRequest request);
        Task<ApiResponse<bool>> ResetPasswordConfirmAsync(ResetPasswordConfirmRequest request);
        Task<ApiResponse<User>> GetUserProfileAsync(int userId);
        Task<ApiResponse<User>> UpdateUserProfileAsync(int userId, UpdateProfileRequest request);
        Task<ApiResponse<bool>> ValidateTokenAsync(string token);
        Task<ApiResponse<bool>> RevokeTokenAsync(string token, string? ipAddress);
    }

    public class AuthService : IAuthService
    {
        private readonly IJwtService _jwtService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;
        private readonly Dictionary<string, LoginAttempt> _loginAttempts = new();
        private readonly object _lockObject = new();

        public AuthService(IJwtService jwtService, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _jwtService = jwtService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request, string? ipAddress)
        {
            var response = new ApiResponse<LoginResponse>();

            try
            {
                // Check rate limiting
                if (IsRateLimited(ipAddress))
                {
                    response.Success = false;
                    response.Message = "Too many login attempts. Please try again later.";
                    return response;
                }

                // Validate input
                var validationResult = ValidateLoginRequest(request);
                if (!validationResult.Success)
                {
                    response.Success = false;
                    response.Message = validationResult.Message;
                    response.Errors = validationResult.Errors;
                    return response;
                }

                // Get user from database (mock for now)
                var user = await GetUserByUsernameAsync(request.Username);
                if (user == null)
                {
                    RecordFailedLoginAttempt(ipAddress);
                    response.Success = false;
                    response.Message = "Invalid credentials";
                    return response;
                }

                // Verify password
                if (!await VerifyPasswordAsync(request.Password, user.PasswordHash))
                {
                    RecordFailedLoginAttempt(ipAddress);
                    response.Success = false;
                    response.Message = "Invalid credentials";
                    return response;
                }

                // Check if account is active
                if (!user.IsActive)
                {
                    response.Success = false;
                    response.Message = "Account is deactivated. Please contact administrator.";
                    return response;
                }

                // Generate tokens
                var token = _jwtService.GenerateToken(user);
                var refreshToken = GenerateRefreshToken();

                // Update last login
                user.LastLoginAt = DateTime.UtcNow;
                await UpdateUserAsync(user);

                // Clear failed attempts
                ClearFailedAttempts(ipAddress);

                // Create response
                var loginResponse = new LoginResponse
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    Username = user.Username,
                    FullName = user.FullName,
                    Role = user.Role,
                    TenantId = user.TenantId,
                    ExpiresAt = DateTime.UtcNow.AddHours(8),
                    RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(30),
                    IsFirstLogin = user.LastLoginAt == null
                };

                response.Success = true;
                response.Message = "Login successful";
                response.Data = loginResponse;

                _logger.LogInformation("User {Username} logged in successfully from {IpAddress}", user.Username, ipAddress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {Username}", request.Username);
                response.Success = false;
                response.Message = "An error occurred during login. Please try again.";
            }

            return response;
        }

        public async Task<ApiResponse<LoginResponse>> RegisterAsync(RegisterRequest request, string? ipAddress)
        {
            var response = new ApiResponse<LoginResponse>();

            try
            {
                // Validate input
                var validationResult = ValidateRegisterRequest(request);
                if (!validationResult.Success)
                {
                    response.Success = false;
                    response.Message = validationResult.Message;
                    response.Errors = validationResult.Errors;
                    return response;
                }

                // Check if username already exists
                var existingUser = await GetUserByUsernameAsync(request.Username);
                if (existingUser != null)
                {
                    response.Success = false;
                    response.Message = "Username already exists";
                    return response;
                }

                // Check if email already exists
                var existingEmail = await GetUserByEmailAsync(request.Email);
                if (existingEmail != null)
                {
                    response.Success = false;
                    response.Message = "Email already registered";
                    return response;
                }

                // Hash password
                var passwordHash = await HashPasswordAsync(request.Password);

                // Create new user
                var newUser = new User
                {
                    Username = request.Username,
                    Email = request.Email,
                    PasswordHash = passwordHash,
                    Role = request.Role.ToString(),
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Phone = request.Phone,
                    Address = request.Address,
                    TenantId = "tampa-fl", // Default tenant
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                // Save user to database
                await CreateUserAsync(newUser);

                // Generate tokens
                var token = _jwtService.GenerateToken(newUser);
                var refreshToken = GenerateRefreshToken();

                // Create response
                var loginResponse = new LoginResponse
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    Username = newUser.Username,
                    FullName = newUser.FullName,
                    Role = newUser.Role,
                    TenantId = newUser.TenantId,
                    ExpiresAt = DateTime.UtcNow.AddHours(8),
                    RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(30),
                    IsFirstLogin = true
                };

                response.Success = true;
                response.Message = "Registration successful";
                response.Data = loginResponse;

                _logger.LogInformation("New user {Username} registered successfully from {IpAddress}", newUser.Username, ipAddress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for user {Username}", request.Username);
                response.Success = false;
                response.Message = "An error occurred during registration. Please try again.";
            }

            return response;
        }

        public async Task<ApiResponse<bool>> LogoutAsync(string token, string? ipAddress)
        {
            var response = new ApiResponse<bool>();

            try
            {
                // Validate token
                var principal = _jwtService.ValidateToken(token);
                if (principal == null)
                {
                    response.Success = false;
                    response.Message = "Invalid token";
                    return response;
                }

                // Add token to blacklist (in production, use Redis or database)
                await RevokeTokenAsync(token, ipAddress);

                response.Success = true;
                response.Message = "Logout successful";
                response.Data = true;

                _logger.LogInformation("User logged out successfully from {IpAddress}", ipAddress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                response.Success = false;
                response.Message = "An error occurred during logout.";
            }

            return response;
        }

        public async Task<ApiResponse<LoginResponse>> RefreshTokenAsync(string refreshToken, string? ipAddress)
        {
            var response = new ApiResponse<LoginResponse>();

            try
            {
                // Validate refresh token (in production, check against database)
                if (string.IsNullOrEmpty(refreshToken))
                {
                    response.Success = false;
                    response.Message = "Invalid refresh token";
                    return response;
                }

                // Get user from refresh token (mock implementation)
                var user = await GetUserFromRefreshTokenAsync(refreshToken);
                if (user == null)
                {
                    response.Success = false;
                    response.Message = "Invalid refresh token";
                    return response;
                }

                // Generate new tokens
                var newToken = _jwtService.GenerateToken(user);
                var newRefreshToken = GenerateRefreshToken();

                // Create response
                var loginResponse = new LoginResponse
                {
                    Token = newToken,
                    RefreshToken = newRefreshToken,
                    Username = user.Username,
                    FullName = user.FullName,
                    Role = user.Role,
                    TenantId = user.TenantId,
                    ExpiresAt = DateTime.UtcNow.AddHours(8),
                    RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(30),
                    IsFirstLogin = false
                };

                response.Success = true;
                response.Message = "Token refreshed successfully";
                response.Data = loginResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                response.Success = false;
                response.Message = "An error occurred while refreshing token.";
            }

            return response;
        }

        public async Task<ApiResponse<bool>> ChangePasswordAsync(int userId, ChangePasswordRequest request)
        {
            var response = new ApiResponse<bool>();

            try
            {
                // Validate input
                if (!ValidationHelper.IsValidPassword(request.NewPassword))
                {
                    response.Success = false;
                    response.Message = "New password does not meet requirements";
                    return response;
                }

                // Get user
                var user = await GetUserByIdAsync(userId);
                if (user == null)
                {
                    response.Success = false;
                    response.Message = "User not found";
                    return response;
                }

                // Verify current password
                if (!await VerifyPasswordAsync(request.CurrentPassword, user.PasswordHash))
                {
                    response.Success = false;
                    response.Message = "Current password is incorrect";
                    return response;
                }

                // Hash new password
                var newPasswordHash = await HashPasswordAsync(request.NewPassword);

                // Update password
                user.PasswordHash = newPasswordHash;
                await UpdateUserAsync(user);

                response.Success = true;
                response.Message = "Password changed successfully";
                response.Data = true;

                _logger.LogInformation("Password changed for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", userId);
                response.Success = false;
                response.Message = "An error occurred while changing password.";
            }

            return response;
        }

        public async Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var response = new ApiResponse<bool>();

            try
            {
                // Validate email
                if (!ValidationHelper.IsValidEmail(request.Email))
                {
                    response.Success = false;
                    response.Message = "Invalid email format";
                    return response;
                }

                // Get user by email
                var user = await GetUserByEmailAsync(request.Email);
                if (user == null)
                {
                    // Don't reveal if email exists or not
                    response.Success = true;
                    response.Message = "If the email exists, a reset link has been sent";
                    response.Data = true;
                    return response;
                }

                // Generate reset token
                var resetToken = GenerateResetToken();

                // Store reset token (in production, save to database with expiration)
                await StoreResetTokenAsync(user.Id, resetToken);

                // Send email (in production, use email service)
                await SendPasswordResetEmailAsync(user.Email, resetToken);

                response.Success = true;
                response.Message = "Password reset email sent";
                response.Data = true;

                _logger.LogInformation("Password reset requested for user {Email}", request.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting password reset for {Email}", request.Email);
                response.Success = false;
                response.Message = "An error occurred while processing the request.";
            }

            return response;
        }

        public async Task<ApiResponse<bool>> ResetPasswordConfirmAsync(ResetPasswordConfirmRequest request)
        {
            var response = new ApiResponse<bool>();

            try
            {
                // Validate new password
                if (!ValidationHelper.IsValidPassword(request.NewPassword))
                {
                    response.Success = false;
                    response.Message = "New password does not meet requirements";
                    return response;
                }

                // Validate reset token (in production, check against database)
                var userId = await ValidateResetTokenAsync(request.Token);
                if (userId == null)
                {
                    response.Success = false;
                    response.Message = "Invalid or expired reset token";
                    return response;
                }

                // Get user
                var user = await GetUserByIdAsync(userId.Value);
                if (user == null)
                {
                    response.Success = false;
                    response.Message = "User not found";
                    return response;
                }

                // Hash new password
                var newPasswordHash = await HashPasswordAsync(request.NewPassword);

                // Update password
                user.PasswordHash = newPasswordHash;
                await UpdateUserAsync(user);

                // Invalidate reset token
                await InvalidateResetTokenAsync(request.Token);

                response.Success = true;
                response.Message = "Password reset successfully";
                response.Data = true;

                _logger.LogInformation("Password reset completed for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming password reset");
                response.Success = false;
                response.Message = "An error occurred while resetting password.";
            }

            return response;
        }

        public async Task<ApiResponse<User>> GetUserProfileAsync(int userId)
        {
            var response = new ApiResponse<User>();

            try
            {
                var user = await GetUserByIdAsync(userId);
                if (user == null)
                {
                    response.Success = false;
                    response.Message = "User not found";
                    return response;
                }

                // Don't return sensitive information
                user.PasswordHash = string.Empty;

                response.Success = true;
                response.Message = "Profile retrieved successfully";
                response.Data = user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user profile for {UserId}", userId);
                response.Success = false;
                response.Message = "An error occurred while retrieving profile.";
            }

            return response;
        }

        public async Task<ApiResponse<User>> UpdateUserProfileAsync(int userId, UpdateProfileRequest request)
        {
            var response = new ApiResponse<User>();

            try
            {
                // Validate input
                var validationResult = ValidateUpdateProfileRequest(request);
                if (!validationResult.Success)
                {
                    response.Success = false;
                    response.Message = validationResult.Message;
                    response.Errors = validationResult.Errors;
                    return response;
                }

                // Get user
                var user = await GetUserByIdAsync(userId);
                if (user == null)
                {
                    response.Success = false;
                    response.Message = "User not found";
                    return response;
                }

                // Check if email is already taken by another user
                if (!string.Equals(user.Email, request.Email, StringComparison.OrdinalIgnoreCase))
                {
                    var existingUser = await GetUserByEmailAsync(request.Email);
                    if (existingUser != null)
                    {
                        response.Success = false;
                        response.Message = "Email already registered by another user";
                        return response;
                    }
                }

                // Update user profile
                user.FirstName = request.FirstName;
                user.LastName = request.LastName;
                user.Email = request.Email;
                user.Phone = request.Phone;
                user.Address = request.Address;

                await UpdateUserAsync(user);

                // Don't return sensitive information
                user.PasswordHash = string.Empty;

                response.Success = true;
                response.Message = "Profile updated successfully";
                response.Data = user;

                _logger.LogInformation("Profile updated for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile for {UserId}", userId);
                response.Success = false;
                response.Message = "An error occurred while updating profile.";
            }

            return response;
        }

        public async Task<ApiResponse<bool>> ValidateTokenAsync(string token)
        {
            var response = new ApiResponse<bool>();

            try
            {
                var principal = _jwtService.ValidateToken(token);
                response.Success = principal != null;
                response.Message = principal != null ? "Token is valid" : "Token is invalid";
                response.Data = principal != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                response.Success = false;
                response.Message = "An error occurred while validating token.";
                response.Data = false;
            }

            return response;
        }

        public async Task<ApiResponse<bool>> RevokeTokenAsync(string token, string? ipAddress)
        {
            var response = new ApiResponse<bool>();

            try
            {
                // In production, add token to blacklist in Redis or database
                // For now, just log the revocation
                _logger.LogInformation("Token revoked from {IpAddress}", ipAddress);

                response.Success = true;
                response.Message = "Token revoked successfully";
                response.Data = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking token");
                response.Success = false;
                response.Message = "An error occurred while revoking token.";
            }

            return response;
        }

        #region Private Methods

        private ValidationResult ValidateLoginRequest(LoginRequest request)
        {
            var errors = new List<string>();

            if (string.IsNullOrEmpty(request.Username))
                errors.Add("Username is required");

            if (string.IsNullOrEmpty(request.Password))
                errors.Add("Password is required");

            if (errors.Any())
            {
                return new ValidationResult
                {
                    Success = false,
                    Message = "Validation failed",
                    Errors = errors
                };
            }

            return new ValidationResult { Success = true };
        }

        private ValidationResult ValidateRegisterRequest(RegisterRequest request)
        {
            var errors = new List<string>();

            if (string.IsNullOrEmpty(request.FirstName) || request.FirstName.Length < 2)
                errors.Add("First name must be at least 2 characters");

            if (string.IsNullOrEmpty(request.LastName) || request.LastName.Length < 2)
                errors.Add("Last name must be at least 2 characters");

            if (!ValidationHelper.IsValidUsername(request.Username))
                errors.Add("Username must be at least 3 characters and contain only letters, numbers, underscores, and hyphens");

            if (!ValidationHelper.IsValidEmail(request.Email))
                errors.Add("Invalid email format");

            if (!ValidationHelper.IsValidPassword(request.Password))
                errors.Add("Password must contain at least 8 characters, including uppercase, lowercase, number, and special character");

            if (request.Password != request.ConfirmPassword)
                errors.Add("Passwords do not match");

            if (!request.AcceptTerms)
                errors.Add("You must accept the terms and conditions");

            if (!string.IsNullOrEmpty(request.Phone) && !ValidationHelper.IsValidPhoneNumber(request.Phone))
                errors.Add("Invalid phone number format");

            if (errors.Any())
            {
                return new ValidationResult
                {
                    Success = false,
                    Message = "Validation failed",
                    Errors = errors
                };
            }

            return new ValidationResult { Success = true };
        }

        private ValidationResult ValidateUpdateProfileRequest(UpdateProfileRequest request)
        {
            var errors = new List<string>();

            if (string.IsNullOrEmpty(request.FirstName) || request.FirstName.Length < 2)
                errors.Add("First name must be at least 2 characters");

            if (string.IsNullOrEmpty(request.LastName) || request.LastName.Length < 2)
                errors.Add("Last name must be at least 2 characters");

            if (!ValidationHelper.IsValidEmail(request.Email))
                errors.Add("Invalid email format");

            if (!string.IsNullOrEmpty(request.Phone) && !ValidationHelper.IsValidPhoneNumber(request.Phone))
                errors.Add("Invalid phone number format");

            if (errors.Any())
            {
                return new ValidationResult
                {
                    Success = false,
                    Message = "Validation failed",
                    Errors = errors
                };
            }

            return new ValidationResult { Success = true };
        }

        private bool IsRateLimited(string? ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress))
                return false;

            lock (_lockObject)
            {
                if (_loginAttempts.TryGetValue(ipAddress, out var attempt))
                {
                    if (attempt.Count >= 5 && DateTime.UtcNow < attempt.LockedUntil)
                    {
                        return true;
                    }

                    if (DateTime.UtcNow >= attempt.LockedUntil)
                    {
                        _loginAttempts.Remove(ipAddress);
                    }
                }
            }

            return false;
        }

        private void RecordFailedLoginAttempt(string? ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress))
                return;

            lock (_lockObject)
            {
                if (_loginAttempts.TryGetValue(ipAddress, out var attempt))
                {
                    attempt.Count++;
                    if (attempt.Count >= 5)
                    {
                        attempt.LockedUntil = DateTime.UtcNow.AddMinutes(15);
                    }
                }
                else
                {
                    _loginAttempts[ipAddress] = new LoginAttempt
                    {
                        Count = 1,
                        FirstAttempt = DateTime.UtcNow
                    };
                }
            }
        }

        private void ClearFailedAttempts(string? ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress))
                return;

            lock (_lockObject)
            {
                _loginAttempts.Remove(ipAddress);
            }
        }

        private async Task<string> HashPasswordAsync(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private async Task<bool> VerifyPasswordAsync(string password, string hash)
        {
            var hashedPassword = await HashPasswordAsync(password);
            return hashedPassword == hash;
        }

        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        private string GenerateResetToken()
        {
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        // Mock database methods (replace with actual database calls)
        private async Task<User?> GetUserByUsernameAsync(string username)
        {
            // Mock implementation - replace with actual database query
            var users = GetMockUsers();
            return users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }

        private async Task<User?> GetUserByEmailAsync(string email)
        {
            // Mock implementation - replace with actual database query
            var users = GetMockUsers();
            return users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }

        private async Task<User?> GetUserByIdAsync(int id)
        {
            // Mock implementation - replace with actual database query
            var users = GetMockUsers();
            return users.FirstOrDefault(u => u.Id == id);
        }

        private async Task<User?> GetUserFromRefreshTokenAsync(string refreshToken)
        {
            // Mock implementation - replace with actual database query
            var users = GetMockUsers();
            return users.FirstOrDefault(); // In production, validate refresh token
        }

        private async Task CreateUserAsync(User user)
        {
            // Mock implementation - replace with actual database insert
            await Task.CompletedTask;
        }

        private async Task UpdateUserAsync(User user)
        {
            // Mock implementation - replace with actual database update
            await Task.CompletedTask;
        }

        private async Task StoreResetTokenAsync(int userId, string token)
        {
            // Mock implementation - replace with actual database insert
            await Task.CompletedTask;
        }

        private async Task<int?> ValidateResetTokenAsync(string token)
        {
            // Mock implementation - replace with actual database query
            await Task.CompletedTask;
            return 1; // Return user ID if valid
        }

        private async Task InvalidateResetTokenAsync(string token)
        {
            // Mock implementation - replace with actual database update
            await Task.CompletedTask;
        }

        private async Task SendPasswordResetEmailAsync(string email, string token)
        {
            // Mock implementation - replace with actual email service
            await Task.CompletedTask;
            _logger.LogInformation("Password reset email would be sent to {Email} with token {Token}", email, token);
        }

        private List<User> GetMockUsers()
        {
            return new List<User>
            {
                new User
                {
                    Id = 1,
                    Username = "admin",
                    PasswordHash = "jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=", // pass123
                    Role = "Admin",
                    TenantId = "tampa-fl",
                    FirstName = "System",
                    LastName = "Administrator",
                    Email = "admin@rexusops360.com",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                },
                new User
                {
                    Id = 2,
                    Username = "dispatcher1",
                    PasswordHash = "jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=", // pass123
                    Role = "Dispatcher",
                    TenantId = "tampa-fl",
                    FirstName = "John",
                    LastName = "Smith",
                    Email = "john.smith@emstampa.com",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-15)
                },
                new User
                {
                    Id = 3,
                    Username = "responder1",
                    PasswordHash = "jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=", // pass123
                    Role = "Responder",
                    TenantId = "tampa-fl",
                    FirstName = "Sarah",
                    LastName = "Johnson",
                    Email = "sarah.johnson@emstampa.com",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-10)
                }
            };
        }

        #endregion
    }

    public class ValidationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new List<string>();
    }

    public class LoginAttempt
    {
        public int Count { get; set; }
        public DateTime FirstAttempt { get; set; }
        public DateTime LockedUntil { get; set; }
    }
} 