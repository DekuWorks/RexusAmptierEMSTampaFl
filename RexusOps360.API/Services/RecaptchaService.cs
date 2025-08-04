/*
 * RexusOps360 EMS API - reCAPTCHA Service
 * 
 * This service handles reCAPTCHA verification for public incident reports.
 * It verifies user responses with Google's reCAPTCHA API to prevent spam.
 * 
 * Features:
 * - reCAPTCHA v2 verification
 * - Spam prevention
 * - Rate limiting integration
 * - Configurable verification settings
 * 
 * Author: RexusOps360 Development Team
 * Version: 1.0.0
 * Last Updated: 2025-01-17
 */

using System.Text.Json;

namespace RexusOps360.API.Services
{
    /// <summary>
    /// reCAPTCHA verification service interface
    /// </summary>
    public interface IRecaptchaService
    {
        Task<bool> VerifyRecaptchaAsync(string recaptchaResponse, string remoteIp);
    }

    /// <summary>
    /// reCAPTCHA verification service implementation
    /// </summary>
    public class RecaptchaService : IRecaptchaService
    {
        private readonly ILogger<RecaptchaService> _logger;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        // reCAPTCHA configuration
        private readonly string _secretKey;
        private readonly string _verifyUrl = "https://www.google.com/recaptcha/api/siteverify";

        public RecaptchaService(ILogger<RecaptchaService> logger, IConfiguration configuration, HttpClient httpClient)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClient = httpClient;
            
            // Get reCAPTCHA secret key from configuration
            // In production, use proper secret management
            _secretKey = _configuration["Recaptcha:SecretKey"] ?? "6LeIxAcTAAAAAGG-vFI1TnRWxMZNFuojJ4WifJWe";
        }

        /// <summary>
        /// Verify reCAPTCHA response with Google API
        /// </summary>
        /// <param name="recaptchaResponse">The reCAPTCHA response from the client</param>
        /// <param name="remoteIp">The remote IP address of the client</param>
        /// <returns>True if verification successful, false otherwise</returns>
        public async Task<bool> VerifyRecaptchaAsync(string recaptchaResponse, string remoteIp)
        {
            try
            {
                // For demo mode, accept any non-empty response
                if (string.IsNullOrWhiteSpace(recaptchaResponse))
                {
                    _logger.LogWarning("reCAPTCHA response is empty");
                    return false;
                }

                // In demo mode, accept the response without verification
                if (_configuration["Environment"] == "Development" || _secretKey.Contains("demo"))
                {
                    _logger.LogInformation("Demo mode: Accepting reCAPTCHA response without verification");
                    return true;
                }

                // Prepare verification request
                var formData = new Dictionary<string, string>
                {
                    { "secret", _secretKey },
                    { "response", recaptchaResponse },
                    { "remoteip", remoteIp }
                };

                var content = new FormUrlEncodedContent(formData);

                // Send verification request to Google
                var response = await _httpClient.PostAsync(_verifyUrl, content);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("reCAPTCHA verification request failed: {StatusCode}", response.StatusCode);
                    return false;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var verificationResult = JsonSerializer.Deserialize<RecaptchaVerificationResponse>(responseContent);

                if (verificationResult?.Success == true)
                {
                    _logger.LogInformation("reCAPTCHA verification successful for IP: {RemoteIp}", remoteIp);
                    return true;
                }
                else
                {
                    _logger.LogWarning("reCAPTCHA verification failed for IP: {RemoteIp}. Errors: {Errors}", 
                        remoteIp, string.Join(", ", verificationResult?.ErrorCodes ?? new List<string>()));
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying reCAPTCHA for IP: {RemoteIp}", remoteIp);
                return false;
            }
        }
    }

    /// <summary>
    /// reCAPTCHA verification response model
    /// </summary>
    public class RecaptchaVerificationResponse
    {
        public bool Success { get; set; }
        public List<string> ErrorCodes { get; set; } = new List<string>();
    }
} 