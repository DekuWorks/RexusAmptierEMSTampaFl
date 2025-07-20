using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RexusOps360.API.Models;
using RexusOps360.API.Services;
using System.Security.Claims;

namespace RexusOps360.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SaasController : ControllerBase
    {
        private readonly ISaasService _saasService;
        private readonly ILogger<SaasController> _logger;

        public SaasController(ISaasService saasService, ILogger<SaasController> logger)
        {
            _saasService = saasService;
            _logger = logger;
        }

        [HttpPost("tenants")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateTenant([FromBody] CreateTenantRequest request)
        {
            try
            {
                var response = await _saasService.CreateTenantAsync(request);

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
                _logger.LogError(ex, "Error creating tenant");
                return StatusCode(500, new ApiResponse<Tenant>
                {
                    Success = false,
                    Message = "An internal server error occurred"
                });
            }
        }

        [HttpGet("tenants/{tenantId}")]
        public async Task<IActionResult> GetTenant(int tenantId)
        {
            try
            {
                var response = await _saasService.GetTenantAsync(tenantId);

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
                _logger.LogError(ex, "Error retrieving tenant {TenantId}", tenantId);
                return StatusCode(500, new ApiResponse<Tenant>
                {
                    Success = false,
                    Message = "An internal server error occurred"
                });
            }
        }

        [HttpPut("tenants/{tenantId}")]
        public async Task<IActionResult> UpdateTenant(int tenantId, [FromBody] UpdateTenantRequest request)
        {
            try
            {
                var response = await _saasService.UpdateTenantAsync(tenantId, request);

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
                _logger.LogError(ex, "Error updating tenant {TenantId}", tenantId);
                return StatusCode(500, new ApiResponse<Tenant>
                {
                    Success = false,
                    Message = "An internal server error occurred"
                });
            }
        }

        [HttpDelete("tenants/{tenantId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTenant(int tenantId)
        {
            try
            {
                var response = await _saasService.DeleteTenantAsync(tenantId);

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
                _logger.LogError(ex, "Error deleting tenant {TenantId}", tenantId);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An internal server error occurred"
                });
            }
        }

        [HttpGet("tenants")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllTenants()
        {
            try
            {
                var response = await _saasService.GetAllTenantsAsync();

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
                _logger.LogError(ex, "Error retrieving all tenants");
                return StatusCode(500, new ApiResponse<List<Tenant>>
                {
                    Success = false,
                    Message = "An internal server error occurred"
                });
            }
        }

        [HttpPost("tenants/{tenantId}/subscriptions")]
        public async Task<IActionResult> CreateSubscription(int tenantId, [FromBody] SubscriptionRequest request)
        {
            try
            {
                var response = await _saasService.CreateSubscriptionAsync(tenantId, request);

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
                _logger.LogError(ex, "Error creating subscription for tenant {TenantId}", tenantId);
                return StatusCode(500, new ApiResponse<Subscription>
                {
                    Success = false,
                    Message = "An internal server error occurred"
                });
            }
        }

        [HttpGet("subscriptions/{subscriptionId}")]
        public async Task<IActionResult> GetSubscription(int subscriptionId)
        {
            try
            {
                var response = await _saasService.GetSubscriptionAsync(subscriptionId);

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
                _logger.LogError(ex, "Error retrieving subscription {SubscriptionId}", subscriptionId);
                return StatusCode(500, new ApiResponse<Subscription>
                {
                    Success = false,
                    Message = "An internal server error occurred"
                });
            }
        }

        [HttpPost("subscriptions/{subscriptionId}/cancel")]
        public async Task<IActionResult> CancelSubscription(int subscriptionId, [FromBody] CancelSubscriptionRequest request)
        {
            try
            {
                var response = await _saasService.CancelSubscriptionAsync(subscriptionId, request.Reason);

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
                _logger.LogError(ex, "Error cancelling subscription {SubscriptionId}", subscriptionId);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An internal server error occurred"
                });
            }
        }

        [HttpPut("tenants/{tenantId}/billing")]
        public async Task<IActionResult> UpdateBillingInfo(int tenantId, [FromBody] BillingInfoRequest request)
        {
            try
            {
                var response = await _saasService.UpdateBillingInfoAsync(tenantId, request);

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
                _logger.LogError(ex, "Error updating billing info for tenant {TenantId}", tenantId);
                return StatusCode(500, new ApiResponse<BillingInfo>
                {
                    Success = false,
                    Message = "An internal server error occurred"
                });
            }
        }

        [HttpPost("tenants/{tenantId}/invoices")]
        public async Task<IActionResult> GenerateInvoice(int tenantId, [FromBody] GenerateInvoiceRequest request)
        {
            try
            {
                var response = await _saasService.GenerateInvoiceAsync(tenantId, request.BillingDate);

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
                _logger.LogError(ex, "Error generating invoice for tenant {TenantId}", tenantId);
                return StatusCode(500, new ApiResponse<Invoice>
                {
                    Success = false,
                    Message = "An internal server error occurred"
                });
            }
        }

        [HttpGet("tenants/{tenantId}/invoices")]
        public async Task<IActionResult> GetInvoices(int tenantId)
        {
            try
            {
                var response = await _saasService.GetInvoicesAsync(tenantId);

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
                _logger.LogError(ex, "Error retrieving invoices for tenant {TenantId}", tenantId);
                return StatusCode(500, new ApiResponse<List<Invoice>>
                {
                    Success = false,
                    Message = "An internal server error occurred"
                });
            }
        }

        [HttpGet("tenants/{tenantId}/dashboard")]
        public async Task<IActionResult> GetTenantDashboard(int tenantId)
        {
            try
            {
                var response = await _saasService.GetTenantDashboardAsync(tenantId);

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
                _logger.LogError(ex, "Error retrieving dashboard for tenant {TenantId}", tenantId);
                return StatusCode(500, new ApiResponse<TenantDashboard>
                {
                    Success = false,
                    Message = "An internal server error occurred"
                });
            }
        }

        [HttpGet("tenants/{tenantId}/usage")]
        public async Task<IActionResult> GetUsageReport(int tenantId, [FromQuery] DateTime date)
        {
            try
            {
                var response = await _saasService.GetUsageReportAsync(tenantId, date);

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
                _logger.LogError(ex, "Error retrieving usage report for tenant {TenantId}", tenantId);
                return StatusCode(500, new ApiResponse<UsageReport>
                {
                    Success = false,
                    Message = "An internal server error occurred"
                });
            }
        }

        [HttpPost("tenants/{tenantId}/usage")]
        public async Task<IActionResult> TrackUsage(int tenantId, [FromBody] UsageMetrics metrics)
        {
            try
            {
                var response = await _saasService.TrackUsageAsync(tenantId, metrics);

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
                _logger.LogError(ex, "Error tracking usage for tenant {TenantId}", tenantId);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An internal server error occurred"
                });
            }
        }

        [HttpGet("plans")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailablePlans()
        {
            try
            {
                var response = await _saasService.GetAvailablePlansAsync();

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
                _logger.LogError(ex, "Error retrieving available plans");
                return StatusCode(500, new ApiResponse<Dictionary<string, PlanDetails>>
                {
                    Success = false,
                    Message = "An internal server error occurred"
                });
            }
        }

        [HttpGet("tenants/{tenantId}/limits")]
        public async Task<IActionResult> ValidateTenantLimits(int tenantId)
        {
            try
            {
                var response = await _saasService.ValidateTenantLimitsAsync(tenantId);

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
                _logger.LogError(ex, "Error validating tenant limits for {TenantId}", tenantId);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An internal server error occurred"
                });
            }
        }

        [HttpGet("analytics/overview")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAnalyticsOverview()
        {
            try
            {
                var allTenants = await _saasService.GetAllTenantsAsync();
                
                if (!allTenants.Success || allTenants.Data == null)
                {
                    return BadRequest(allTenants);
                }

                var analytics = new
                {
                    TotalTenants = allTenants.Data.Count,
                    ActiveTenants = allTenants.Data.Count(t => t.Status == TenantStatus.Active),
                    TrialTenants = allTenants.Data.Count(t => t.Status == TenantStatus.Trial),
                    SuspendedTenants = allTenants.Data.Count(t => t.Status == TenantStatus.Suspended),
                    TotalRevenue = allTenants.Data.Sum(t => 299.00m), // Mock revenue calculation
                    AverageUsage = allTenants.Data.Average(t => t.MaxUsers),
                    TopPlans = new[]
                    {
                        new { Plan = "Professional", Count = allTenants.Data.Count(t => t.MaxUsers == 25) },
                        new { Plan = "Enterprise", Count = allTenants.Data.Count(t => t.MaxUsers == 100) },
                        new { Plan = "Starter", Count = allTenants.Data.Count(t => t.MaxUsers == 5) }
                    }
                };

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Analytics overview retrieved successfully",
                    Data = analytics
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving analytics overview");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An internal server error occurred"
                });
            }
        }
    }

    public class CancelSubscriptionRequest
    {
        public string Reason { get; set; } = string.Empty;
    }

    public class GenerateInvoiceRequest
    {
        public DateTime BillingDate { get; set; } = DateTime.UtcNow;
    }
} 