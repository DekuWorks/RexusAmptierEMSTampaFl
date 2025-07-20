using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using RexusOps360.API.Models;
using RexusOps360.API.Services;
using Xunit;

namespace RexusOps360.API.Tests
{
    public class AuthServiceTests
    {
        private readonly Mock<IJwtService> _mockJwtService;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ILogger<AuthService>> _mockLogger;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _mockJwtService = new Mock<IJwtService>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<AuthService>>();

            _authService = new AuthService(_mockJwtService.Object, _mockConfiguration.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ReturnsSuccess()
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = "admin",
                Password = "admin123"
            };

            _mockJwtService.Setup(x => x.GenerateToken(It.IsAny<User>()))
                .Returns("mock-jwt-token");

            // Act
            var result = await _authService.LoginAsync(request, "127.0.0.1");

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Login successful", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal("admin", result.Data.Username);
            Assert.Equal("Admin", result.Data.Role);
        }

        [Fact]
        public async Task LoginAsync_WithInvalidCredentials_ReturnsFailure()
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = "invalid",
                Password = "invalid"
            };

            // Act
            var result = await _authService.LoginAsync(request, "127.0.0.1");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Invalid credentials", result.Message);
        }

        [Fact]
        public async Task LoginAsync_WithRateLimitedIp_ReturnsFailure()
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = "admin",
                Password = "admin123"
            };

            // Act - Make multiple failed attempts
            for (int i = 0; i < 6; i++)
            {
                var failedRequest = new LoginRequest
                {
                    Username = "invalid",
                    Password = "invalid"
                };
                await _authService.LoginAsync(failedRequest, "127.0.0.1");
            }

            // Try to login with valid credentials
            var result = await _authService.LoginAsync(request, "127.0.0.1");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Too many login attempts. Please try again later.", result.Message);
        }

        [Fact]
        public async Task RegisterAsync_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var request = new RegisterRequest
            {
                FirstName = "John",
                LastName = "Doe",
                Username = "johndoe",
                Email = "john.doe@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                Role = UserRole.Responder,
                AcceptTerms = true
            };

            _mockJwtService.Setup(x => x.GenerateToken(It.IsAny<User>()))
                .Returns("mock-jwt-token");

            // Act
            var result = await _authService.RegisterAsync(request, "127.0.0.1");

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Registration successful", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal("johndoe", result.Data.Username);
            Assert.Equal("Responder", result.Data.Role);
        }

        [Fact]
        public async Task RegisterAsync_WithExistingUsername_ReturnsFailure()
        {
            // Arrange
            var request = new RegisterRequest
            {
                FirstName = "John",
                LastName = "Doe",
                Username = "admin", // Existing username
                Email = "john.doe@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                Role = UserRole.Responder,
                AcceptTerms = true
            };

            // Act
            var result = await _authService.RegisterAsync(request, "127.0.0.1");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Username already exists", result.Message);
        }

        [Fact]
        public async Task RegisterAsync_WithWeakPassword_ReturnsFailure()
        {
            // Arrange
            var request = new RegisterRequest
            {
                FirstName = "John",
                LastName = "Doe",
                Username = "johndoe",
                Email = "john.doe@example.com",
                Password = "weak", // Weak password
                ConfirmPassword = "weak",
                Role = UserRole.Responder,
                AcceptTerms = true
            };

            // Act
            var result = await _authService.RegisterAsync(request, "127.0.0.1");

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Password does not meet requirements", result.Message);
        }

        [Fact]
        public async Task RegisterAsync_WithMismatchedPasswords_ReturnsFailure()
        {
            // Arrange
            var request = new RegisterRequest
            {
                FirstName = "John",
                LastName = "Doe",
                Username = "johndoe",
                Email = "john.doe@example.com",
                Password = "Password123!",
                ConfirmPassword = "DifferentPassword123!", // Mismatched
                Role = UserRole.Responder,
                AcceptTerms = true
            };

            // Act
            var result = await _authService.RegisterAsync(request, "127.0.0.1");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Passwords do not match.", result.Message);
        }

        [Fact]
        public async Task RegisterAsync_WithoutAcceptingTerms_ReturnsFailure()
        {
            // Arrange
            var request = new RegisterRequest
            {
                FirstName = "John",
                LastName = "Doe",
                Username = "johndoe",
                Email = "john.doe@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                Role = UserRole.Responder,
                AcceptTerms = false // Not accepting terms
            };

            // Act
            var result = await _authService.RegisterAsync(request, "127.0.0.1");

            // Assert
            Assert.False(result.Success);
            Assert.Contains("You must accept the terms and conditions", result.Message);
        }

        [Fact]
        public async Task ChangePasswordAsync_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var request = new ChangePasswordRequest
            {
                CurrentPassword = "admin123",
                NewPassword = "NewPassword123!",
                ConfirmNewPassword = "NewPassword123!"
            };

            // Act
            var result = await _authService.ChangePasswordAsync(1, request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Password changed successfully", result.Message);
        }

        [Fact]
        public async Task ChangePasswordAsync_WithInvalidCurrentPassword_ReturnsFailure()
        {
            // Arrange
            var request = new ChangePasswordRequest
            {
                CurrentPassword = "wrongpassword",
                NewPassword = "NewPassword123!",
                ConfirmNewPassword = "NewPassword123!"
            };

            // Act
            var result = await _authService.ChangePasswordAsync(1, request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Current password is incorrect", result.Message);
        }

        [Fact]
        public async Task GetUserProfileAsync_WithValidUserId_ReturnsSuccess()
        {
            // Act
            var result = await _authService.GetUserProfileAsync(1);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Profile retrieved successfully", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal("admin", result.Data.Username);
            Assert.Equal("Admin", result.Data.Role);
        }

        [Fact]
        public async Task GetUserProfileAsync_WithInvalidUserId_ReturnsFailure()
        {
            // Act
            var result = await _authService.GetUserProfileAsync(999);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("User not found", result.Message);
        }

        [Fact]
        public async Task UpdateUserProfileAsync_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var request = new UpdateProfileRequest
            {
                FirstName = "Updated",
                LastName = "Name",
                Email = "updated@example.com",
                Phone = "+1234567890"
            };

            // Act
            var result = await _authService.UpdateUserProfileAsync(1, request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Profile updated successfully", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal("Updated", result.Data.FirstName);
            Assert.Equal("Name", result.Data.LastName);
        }

        [Fact]
        public async Task ValidateTokenAsync_WithValidToken_ReturnsSuccess()
        {
            // Arrange
            var validToken = "valid-jwt-token";
            _mockJwtService.Setup(x => x.ValidateToken(validToken))
                .Returns(new System.Security.Claims.ClaimsPrincipal());

            // Act
            var result = await _authService.ValidateTokenAsync(validToken);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Token is valid", result.Message);
        }

        [Fact]
        public async Task ValidateTokenAsync_WithInvalidToken_ReturnsFailure()
        {
            // Arrange
            var invalidToken = "invalid-jwt-token";
            _mockJwtService.Setup(x => x.ValidateToken(invalidToken))
                .Returns((System.Security.Claims.ClaimsPrincipal?)null);

            // Act
            var result = await _authService.ValidateTokenAsync(invalidToken);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Token is invalid", result.Message);
        }

        [Fact]
        public async Task LogoutAsync_WithValidToken_ReturnsSuccess()
        {
            // Arrange
            var token = "valid-jwt-token";

            // Act
            var result = await _authService.LogoutAsync(token, "127.0.0.1");

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Token revoked successfully", result.Message);
        }

        [Fact]
        public async Task RefreshTokenAsync_WithValidToken_ReturnsSuccess()
        {
            // Arrange
            var refreshToken = "valid-refresh-token";

            _mockJwtService.Setup(x => x.GenerateToken(It.IsAny<User>()))
                .Returns("new-jwt-token");

            // Act
            var result = await _authService.RefreshTokenAsync(refreshToken, "127.0.0.1");

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Token refreshed successfully", result.Message);
            Assert.NotNull(result.Data);
        }

        [Fact]
        public async Task RefreshTokenAsync_WithInvalidToken_ReturnsFailure()
        {
            // Arrange
            var invalidRefreshToken = "invalid-refresh-token";

            // Act
            var result = await _authService.RefreshTokenAsync(invalidRefreshToken, "127.0.0.1");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Invalid refresh token", result.Message);
        }

        [Fact]
        public async Task ResetPasswordAsync_WithValidEmail_ReturnsSuccess()
        {
            // Arrange
            var request = new ResetPasswordRequest
            {
                Email = "admin@rexusops360.com"
            };

            // Act
            var result = await _authService.ResetPasswordAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Password reset email sent", result.Message);
        }

        [Fact]
        public async Task ResetPasswordAsync_WithInvalidEmail_ReturnsSuccess()
        {
            // Arrange
            var request = new ResetPasswordRequest
            {
                Email = "nonexistent@example.com"
            };

            // Act
            var result = await _authService.ResetPasswordAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("If the email exists, a reset link has been sent", result.Message);
        }

        [Theory]
        [InlineData("Password123!", true)]
        [InlineData("password", false)] // No uppercase
        [InlineData("PASSWORD", false)] // No lowercase
        [InlineData("Password", false)] // No digit
        [InlineData("Password123", false)] // No special character
        [InlineData("Pass1!", false)] // Too short
        public void ValidationHelper_IsValidPassword_ReturnsExpectedResult(string password, bool expected)
        {
            // Act
            var result = ValidationHelper.IsValidPassword(password);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("validuser", true)]
        [InlineData("user123", true)]
        [InlineData("user_name", true)]
        [InlineData("user-name", true)]
        [InlineData("ab", false)] // Too short
        [InlineData("user@name", false)] // Invalid character
        [InlineData("user name", false)] // Space not allowed
        public void ValidationHelper_IsValidUsername_ReturnsExpectedResult(string username, bool expected)
        {
            // Act
            var result = ValidationHelper.IsValidUsername(username);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("test@example.com", true)]
        [InlineData("invalid-email", false)]
        [InlineData("test@", false)]
        [InlineData("@example.com", false)]
        [InlineData("", false)]
        public void ValidationHelper_IsValidEmail_ReturnsExpectedResult(string email, bool expected)
        {
            // Act
            var result = ValidationHelper.IsValidEmail(email);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("+1234567890", true)]
        [InlineData("1234567890", true)]
        [InlineData("123-456-7890", false)] // Invalid format
        [InlineData("", true)] // Optional field
        [InlineData(null, true)] // Optional field
        public void ValidationHelper_IsValidPhoneNumber_ReturnsExpectedResult(string phone, bool expected)
        {
            // Act
            var result = ValidationHelper.IsValidPhoneNumber(phone);

            // Assert
            Assert.Equal(expected, result);
        }
    }
} 