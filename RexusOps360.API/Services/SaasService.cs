using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RexusOps360.API.Models;
using System.Text.RegularExpressions;

namespace RexusOps360.API.Services
{
    public interface ISaasService
    {
        Task<ApiResponse<Tenant>> CreateTenantAsync(CreateTenantRequest request);
        Task<ApiResponse<Tenant>> GetTenantAsync(int tenantId);
        Task<ApiResponse<Tenant>> UpdateTenantAsync(int tenantId, UpdateTenantRequest request);
        Task<ApiResponse<bool>> DeleteTenantAsync(int tenantId);
        Task<ApiResponse<List<Tenant>>> GetAllTenantsAsync();
        Task<ApiResponse<Subscription>> CreateSubscriptionAsync(int tenantId, SubscriptionRequest request);
        Task<ApiResponse<Subscription>> GetSubscriptionAsync(int subscriptionId);
        Task<ApiResponse<bool>> CancelSubscriptionAsync(int subscriptionId, string reason);
        Task<ApiResponse<BillingInfo>> UpdateBillingInfoAsync(int tenantId, BillingInfoRequest request);
        Task<ApiResponse<Invoice>> GenerateInvoiceAsync(int tenantId, DateTime billingDate);
        Task<ApiResponse<List<Invoice>>> GetInvoicesAsync(int tenantId);
        Task<ApiResponse<TenantDashboard>> GetTenantDashboardAsync(int tenantId);
        Task<ApiResponse<UsageReport>> GetUsageReportAsync(int tenantId, DateTime date);
        Task<ApiResponse<bool>> TrackUsageAsync(int tenantId, UsageMetrics metrics);
        Task<ApiResponse<Dictionary<string, PlanDetails>>> GetAvailablePlansAsync();
        Task<ApiResponse<bool>> ValidateTenantLimitsAsync(int tenantId);
    }

    public class SaasService : ISaasService
    {
        private readonly ILogger<SaasService> _logger;
        private readonly IConfiguration _configuration;
        private readonly Dictionary<int, Tenant> _tenants = new();
        private readonly Dictionary<int, Subscription> _subscriptions = new();
        private readonly Dictionary<int, BillingInfo> _billingInfos = new();
        private readonly Dictionary<int, List<Invoice>> _invoices = new();
        private readonly Dictionary<int, List<UsageMetrics>> _usageMetrics = new();

        public SaasService(ILogger<SaasService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            InitializeMockData();
        }

        public async Task<ApiResponse<Tenant>> CreateTenantAsync(CreateTenantRequest request)
        {
            var response = new ApiResponse<Tenant>();

            try
            {
                // Validate slug format
                if (!Regex.IsMatch(request.Slug, @"^[a-z0-9-]+$"))
                {
                    response.Success = false;
                    response.Message = "Slug can only contain lowercase letters, numbers, and hyphens";
                    return response;
                }

                // Check if slug already exists
                if (_tenants.Values.Any(t => t.Slug == request.Slug))
                {
                    response.Success = false;
                    response.Message = "Tenant slug already exists";
                    return response;
                }

                // Get plan details
                if (!SaasPlans.Plans.TryGetValue(request.PlanName.ToLower(), out var plan))
                {
                    response.Success = false;
                    response.Message = "Invalid plan selected";
                    return response;
                }

                // Create tenant
                var tenant = new Tenant
                {
                    Id = _tenants.Count + 1,
                    Name = request.Name,
                    Slug = request.Slug,
                    AdminEmail = request.AdminEmail,
                    Description = request.Description,
                    Industry = request.Industry,
                    Region = request.Region,
                    Phone = request.Phone,
                    Address = request.Address,
                    Website = request.Website,
                    Status = request.StartTrial ? TenantStatus.Trial : TenantStatus.Active,
                    TrialEndsAt = request.StartTrial ? DateTime.UtcNow.AddDays(plan.TrialDays) : null,
                    MaxUsers = plan.MaxUsers,
                    MaxIncidents = plan.MaxIncidents,
                    MaxStorageGB = plan.MaxStorageGB,
                    CustomBranding = plan.CustomBranding,
                    AdvancedAnalytics = plan.AdvancedAnalytics,
                    ApiAccess = plan.ApiAccess,
                    PrioritySupport = plan.PrioritySupport,
                    CreatedAt = DateTime.UtcNow
                };

                _tenants[tenant.Id] = tenant;

                // Create subscription
                var subscription = new Subscription
                {
                    Id = _subscriptions.Count + 1,
                    TenantId = tenant.Id,
                    PlanName = plan.Name,
                    Status = request.StartTrial ? SubscriptionStatus.Trial : SubscriptionStatus.Active,
                    StartDate = DateTime.UtcNow,
                    TrialEndsAt = request.StartTrial ? DateTime.UtcNow.AddDays(plan.TrialDays) : null,
                    MonthlyPrice = plan.MonthlyPrice,
                    MaxUsers = plan.MaxUsers,
                    MaxIncidents = plan.MaxIncidents,
                    MaxStorageGB = plan.MaxStorageGB,
                    CustomBranding = plan.CustomBranding,
                    AdvancedAnalytics = plan.AdvancedAnalytics,
                    ApiAccess = plan.ApiAccess,
                    PrioritySupport = plan.PrioritySupport,
                    BillingCycle = BillingCycle.Monthly,
                    NextBillingDate = DateTime.UtcNow.AddMonths(1),
                    CreatedAt = DateTime.UtcNow
                };

                _subscriptions[subscription.Id] = subscription;

                response.Success = true;
                response.Message = "Tenant created successfully";
                response.Data = tenant;

                _logger.LogInformation("Tenant {TenantName} created with plan {PlanName}", tenant.Name, plan.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tenant");
                response.Success = false;
                response.Message = "An error occurred while creating tenant";
            }

            return response;
        }

        public async Task<ApiResponse<Tenant>> GetTenantAsync(int tenantId)
        {
            var response = new ApiResponse<Tenant>();

            try
            {
                if (_tenants.TryGetValue(tenantId, out var tenant))
                {
                    response.Success = true;
                    response.Message = "Tenant retrieved successfully";
                    response.Data = tenant;
                }
                else
                {
                    response.Success = false;
                    response.Message = "Tenant not found";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tenant {TenantId}", tenantId);
                response.Success = false;
                response.Message = "An error occurred while retrieving tenant";
            }

            return response;
        }

        public async Task<ApiResponse<Tenant>> UpdateTenantAsync(int tenantId, UpdateTenantRequest request)
        {
            var response = new ApiResponse<Tenant>();

            try
            {
                if (!_tenants.TryGetValue(tenantId, out var tenant))
                {
                    response.Success = false;
                    response.Message = "Tenant not found";
                    return response;
                }

                // Update tenant properties
                if (!string.IsNullOrEmpty(request.Name))
                    tenant.Name = request.Name;
                if (!string.IsNullOrEmpty(request.Description))
                    tenant.Description = request.Description;
                if (!string.IsNullOrEmpty(request.Industry))
                    tenant.Industry = request.Industry;
                if (!string.IsNullOrEmpty(request.Region))
                    tenant.Region = request.Region;
                if (!string.IsNullOrEmpty(request.Phone))
                    tenant.Phone = request.Phone;
                if (!string.IsNullOrEmpty(request.Address))
                    tenant.Address = request.Address;
                if (!string.IsNullOrEmpty(request.Website))
                    tenant.Website = request.Website;

                tenant.UpdatedAt = DateTime.UtcNow;

                response.Success = true;
                response.Message = "Tenant updated successfully";
                response.Data = tenant;

                _logger.LogInformation("Tenant {TenantId} updated", tenantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tenant {TenantId}", tenantId);
                response.Success = false;
                response.Message = "An error occurred while updating tenant";
            }

            return response;
        }

        public async Task<ApiResponse<bool>> DeleteTenantAsync(int tenantId)
        {
            var response = new ApiResponse<bool>();

            try
            {
                if (_tenants.Remove(tenantId))
                {
                    // Remove related data
                    var subscriptionsToRemove = _subscriptions.Values.Where(s => s.TenantId == tenantId).ToList();
                    foreach (var subscription in subscriptionsToRemove)
                    {
                        _subscriptions.Remove(subscription.Id);
                    }

                    response.Success = true;
                    response.Message = "Tenant deleted successfully";
                    response.Data = true;

                    _logger.LogInformation("Tenant {TenantId} deleted", tenantId);
                }
                else
                {
                    response.Success = false;
                    response.Message = "Tenant not found";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting tenant {TenantId}", tenantId);
                response.Success = false;
                response.Message = "An error occurred while deleting tenant";
            }

            return response;
        }

        public async Task<ApiResponse<List<Tenant>>> GetAllTenantsAsync()
        {
            var response = new ApiResponse<List<Tenant>>();

            try
            {
                var tenants = _tenants.Values.ToList();
                response.Success = true;
                response.Message = "Tenants retrieved successfully";
                response.Data = tenants;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tenants");
                response.Success = false;
                response.Message = "An error occurred while retrieving tenants";
            }

            return response;
        }

        public async Task<ApiResponse<Subscription>> CreateSubscriptionAsync(int tenantId, SubscriptionRequest request)
        {
            var response = new ApiResponse<Subscription>();

            try
            {
                if (!_tenants.TryGetValue(tenantId, out var tenant))
                {
                    response.Success = false;
                    response.Message = "Tenant not found";
                    return response;
                }

                if (!SaasPlans.Plans.TryGetValue(request.PlanName.ToLower(), out var plan))
                {
                    response.Success = false;
                    response.Message = "Invalid plan selected";
                    return response;
                }

                // Cancel existing subscription
                var existingSubscription = _subscriptions.Values.FirstOrDefault(s => s.TenantId == tenantId && s.Status == SubscriptionStatus.Active);
                if (existingSubscription != null)
                {
                    existingSubscription.Status = SubscriptionStatus.Cancelled;
                    existingSubscription.CancelledAt = DateTime.UtcNow;
                    existingSubscription.CancellationReason = "Upgraded to new plan";
                }

                // Create new subscription
                var subscription = new Subscription
                {
                    Id = _subscriptions.Count + 1,
                    TenantId = tenantId,
                    PlanName = plan.Name,
                    Status = request.StartTrial ? SubscriptionStatus.Trial : SubscriptionStatus.Active,
                    StartDate = DateTime.UtcNow,
                    TrialEndsAt = request.StartTrial ? DateTime.UtcNow.AddDays(plan.TrialDays) : null,
                    MonthlyPrice = plan.MonthlyPrice,
                    MaxUsers = plan.MaxUsers,
                    MaxIncidents = plan.MaxIncidents,
                    MaxStorageGB = plan.MaxStorageGB,
                    CustomBranding = plan.CustomBranding,
                    AdvancedAnalytics = plan.AdvancedAnalytics,
                    ApiAccess = plan.ApiAccess,
                    PrioritySupport = plan.PrioritySupport,
                    BillingCycle = request.BillingCycle,
                    NextBillingDate = DateTime.UtcNow.AddMonths(1),
                    CreatedAt = DateTime.UtcNow
                };

                _subscriptions[subscription.Id] = subscription;

                // Update tenant limits
                tenant.MaxUsers = plan.MaxUsers;
                tenant.MaxIncidents = plan.MaxIncidents;
                tenant.MaxStorageGB = plan.MaxStorageGB;
                tenant.CustomBranding = plan.CustomBranding;
                tenant.AdvancedAnalytics = plan.AdvancedAnalytics;
                tenant.ApiAccess = plan.ApiAccess;
                tenant.PrioritySupport = plan.PrioritySupport;

                response.Success = true;
                response.Message = "Subscription created successfully";
                response.Data = subscription;

                _logger.LogInformation("Subscription created for tenant {TenantId} with plan {PlanName}", tenantId, plan.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating subscription for tenant {TenantId}", tenantId);
                response.Success = false;
                response.Message = "An error occurred while creating subscription";
            }

            return response;
        }

        public async Task<ApiResponse<Subscription>> GetSubscriptionAsync(int subscriptionId)
        {
            var response = new ApiResponse<Subscription>();

            try
            {
                if (_subscriptions.TryGetValue(subscriptionId, out var subscription))
                {
                    response.Success = true;
                    response.Message = "Subscription retrieved successfully";
                    response.Data = subscription;
                }
                else
                {
                    response.Success = false;
                    response.Message = "Subscription not found";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving subscription {SubscriptionId}", subscriptionId);
                response.Success = false;
                response.Message = "An error occurred while retrieving subscription";
            }

            return response;
        }

        public async Task<ApiResponse<bool>> CancelSubscriptionAsync(int subscriptionId, string reason)
        {
            var response = new ApiResponse<bool>();

            try
            {
                if (_subscriptions.TryGetValue(subscriptionId, out var subscription))
                {
                    subscription.Status = SubscriptionStatus.Cancelled;
                    subscription.CancelledAt = DateTime.UtcNow;
                    subscription.CancellationReason = reason;

                    response.Success = true;
                    response.Message = "Subscription cancelled successfully";
                    response.Data = true;

                    _logger.LogInformation("Subscription {SubscriptionId} cancelled: {Reason}", subscriptionId, reason);
                }
                else
                {
                    response.Success = false;
                    response.Message = "Subscription not found";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling subscription {SubscriptionId}", subscriptionId);
                response.Success = false;
                response.Message = "An error occurred while cancelling subscription";
            }

            return response;
        }

        public async Task<ApiResponse<BillingInfo>> UpdateBillingInfoAsync(int tenantId, BillingInfoRequest request)
        {
            var response = new ApiResponse<BillingInfo>();

            try
            {
                if (!_tenants.TryGetValue(tenantId, out var tenant))
                {
                    response.Success = false;
                    response.Message = "Tenant not found";
                    return response;
                }

                var billingInfo = new BillingInfo
                {
                    Id = _billingInfos.Count + 1,
                    TenantId = tenantId,
                    CompanyName = request.CompanyName,
                    ContactName = request.ContactName,
                    Email = request.Email,
                    Phone = request.Phone,
                    Address = request.Address,
                    City = request.City,
                    State = request.State,
                    PostalCode = request.PostalCode,
                    Country = request.Country,
                    TaxId = request.TaxId,
                    CreatedAt = DateTime.UtcNow
                };

                _billingInfos[billingInfo.Id] = billingInfo;

                response.Success = true;
                response.Message = "Billing information updated successfully";
                response.Data = billingInfo;

                _logger.LogInformation("Billing info updated for tenant {TenantId}", tenantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating billing info for tenant {TenantId}", tenantId);
                response.Success = false;
                response.Message = "An error occurred while updating billing information";
            }

            return response;
        }

        public async Task<ApiResponse<Invoice>> GenerateInvoiceAsync(int tenantId, DateTime billingDate)
        {
            var response = new ApiResponse<Invoice>();

            try
            {
                if (!_tenants.TryGetValue(tenantId, out var tenant))
                {
                    response.Success = false;
                    response.Message = "Tenant not found";
                    return response;
                }

                var subscription = _subscriptions.Values.FirstOrDefault(s => s.TenantId == tenantId && s.Status == SubscriptionStatus.Active);
                if (subscription == null)
                {
                    response.Success = false;
                    response.Message = "No active subscription found";
                    return response;
                }

                var invoice = new Invoice
                {
                    Id = (_invoices.ContainsKey(tenantId) ? _invoices[tenantId].Count : 0) + 1,
                    TenantId = tenantId,
                    InvoiceNumber = $"INV-{tenantId:D4}-{DateTime.UtcNow:yyyyMMdd}-{DateTime.UtcNow.Ticks % 10000:D4}",
                    Status = InvoiceStatus.Draft,
                    IssueDate = billingDate,
                    DueDate = billingDate.AddDays(30),
                    Subtotal = subscription.MonthlyPrice,
                    TaxAmount = subscription.MonthlyPrice * 0.08m, // 8% tax
                    TotalAmount = subscription.MonthlyPrice * 1.08m,
                    Currency = "USD",
                    CreatedAt = DateTime.UtcNow
                };

                // Add invoice items
                invoice.Items.Add(new InvoiceItem
                {
                    Id = 1,
                    InvoiceId = invoice.Id,
                    Description = $"{subscription.PlanName} Plan - {billingDate:MMMM yyyy}",
                    Quantity = 1,
                    UnitPrice = subscription.MonthlyPrice,
                    TotalPrice = subscription.MonthlyPrice,
                    ItemType = "Subscription"
                });

                if (!_invoices.ContainsKey(tenantId))
                    _invoices[tenantId] = new List<Invoice>();

                _invoices[tenantId].Add(invoice);

                response.Success = true;
                response.Message = "Invoice generated successfully";
                response.Data = invoice;

                _logger.LogInformation("Invoice generated for tenant {TenantId}: {InvoiceNumber}", tenantId, invoice.InvoiceNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating invoice for tenant {TenantId}", tenantId);
                response.Success = false;
                response.Message = "An error occurred while generating invoice";
            }

            return response;
        }

        public async Task<ApiResponse<List<Invoice>>> GetInvoicesAsync(int tenantId)
        {
            var response = new ApiResponse<List<Invoice>>();

            try
            {
                var invoices = _invoices.ContainsKey(tenantId) ? _invoices[tenantId] : new List<Invoice>();
                response.Success = true;
                response.Message = "Invoices retrieved successfully";
                response.Data = invoices;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving invoices for tenant {TenantId}", tenantId);
                response.Success = false;
                response.Message = "An error occurred while retrieving invoices";
            }

            return response;
        }

        public async Task<ApiResponse<TenantDashboard>> GetTenantDashboardAsync(int tenantId)
        {
            var response = new ApiResponse<TenantDashboard>();

            try
            {
                if (!_tenants.TryGetValue(tenantId, out var tenant))
                {
                    response.Success = false;
                    response.Message = "Tenant not found";
                    return response;
                }

                var dashboard = new TenantDashboard
                {
                    Tenant = tenant,
                    CurrentSubscription = _subscriptions.Values.FirstOrDefault(s => s.TenantId == tenantId && s.Status == SubscriptionStatus.Active),
                    BillingInfo = _billingInfos.Values.FirstOrDefault(b => b.TenantId == tenantId),
                    RecentInvoices = _invoices.ContainsKey(tenantId) ? _invoices[tenantId].Take(5).ToList() : new List<Invoice>(),
                    CurrentUsage = await GetUsageReportAsync(tenantId, DateTime.UtcNow.Date).ContinueWith(t => t.Result.Data),
                    UsageHistory = new List<UsageReport>(),
                    Metrics = new Dictionary<string, object>
                    {
                        ["totalIncidents"] = 0,
                        ["activeUsers"] = 0,
                        ["storageUsed"] = 0,
                        ["apiCalls"] = 0
                    }
                };

                response.Success = true;
                response.Message = "Dashboard retrieved successfully";
                response.Data = dashboard;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard for tenant {TenantId}", tenantId);
                response.Success = false;
                response.Message = "An error occurred while retrieving dashboard";
            }

            return response;
        }

        public async Task<ApiResponse<UsageReport>> GetUsageReportAsync(int tenantId, DateTime date)
        {
            var response = new ApiResponse<UsageReport>();

            try
            {
                if (!_tenants.TryGetValue(tenantId, out var tenant))
                {
                    response.Success = false;
                    response.Message = "Tenant not found";
                    return response;
                }

                var usage = _usageMetrics.ContainsKey(tenantId) 
                    ? _usageMetrics[tenantId].FirstOrDefault(u => u.Date.Date == date.Date)
                    : null;

                var report = new UsageReport
                {
                    TenantId = tenantId,
                    TenantName = tenant.Name,
                    Date = date,
                    ActiveUsers = usage?.ActiveUsers ?? 0,
                    TotalIncidents = usage?.TotalIncidents ?? 0,
                    StorageUsedMB = usage?.StorageUsedMB ?? 0,
                    ApiCalls = usage?.ApiCalls ?? 0,
                    SupportTickets = usage?.SupportTickets ?? 0,
                    UsagePercentage = CalculateUsagePercentage(tenant, usage)
                };

                response.Success = true;
                response.Message = "Usage report retrieved successfully";
                response.Data = report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving usage report for tenant {TenantId}", tenantId);
                response.Success = false;
                response.Message = "An error occurred while retrieving usage report";
            }

            return response;
        }

        public async Task<ApiResponse<bool>> TrackUsageAsync(int tenantId, UsageMetrics metrics)
        {
            var response = new ApiResponse<bool>();

            try
            {
                if (!_tenants.TryGetValue(tenantId, out var tenant))
                {
                    response.Success = false;
                    response.Message = "Tenant not found";
                    return response;
                }

                if (!_usageMetrics.ContainsKey(tenantId))
                    _usageMetrics[tenantId] = new List<UsageMetrics>();

                metrics.TenantId = tenantId;
                metrics.Date = DateTime.UtcNow.Date;
                metrics.CreatedAt = DateTime.UtcNow;

                _usageMetrics[tenantId].Add(metrics);

                response.Success = true;
                response.Message = "Usage tracked successfully";
                response.Data = true;

                _logger.LogInformation("Usage tracked for tenant {TenantId}: {ActiveUsers} users, {TotalIncidents} incidents", 
                    tenantId, metrics.ActiveUsers, metrics.TotalIncidents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking usage for tenant {TenantId}", tenantId);
                response.Success = false;
                response.Message = "An error occurred while tracking usage";
            }

            return response;
        }

        public async Task<ApiResponse<Dictionary<string, PlanDetails>>> GetAvailablePlansAsync()
        {
            var response = new ApiResponse<Dictionary<string, PlanDetails>>();

            try
            {
                response.Success = true;
                response.Message = "Available plans retrieved successfully";
                response.Data = SaasPlans.Plans;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available plans");
                response.Success = false;
                response.Message = "An error occurred while retrieving plans";
            }

            return response;
        }

        public async Task<ApiResponse<bool>> ValidateTenantLimitsAsync(int tenantId)
        {
            var response = new ApiResponse<bool>();

            try
            {
                if (!_tenants.TryGetValue(tenantId, out var tenant))
                {
                    response.Success = false;
                    response.Message = "Tenant not found";
                    return response;
                }

                var currentUsage = _usageMetrics.ContainsKey(tenantId) 
                    ? _usageMetrics[tenantId].FirstOrDefault(u => u.Date.Date == DateTime.UtcNow.Date)
                    : null;

                if (currentUsage != null)
                {
                    var isValid = currentUsage.ActiveUsers <= tenant.MaxUsers &&
                                 currentUsage.TotalIncidents <= tenant.MaxIncidents &&
                                 currentUsage.StorageUsedMB <= tenant.MaxStorageGB * 1024;

                    response.Success = true;
                    response.Message = isValid ? "Usage within limits" : "Usage exceeds limits";
                    response.Data = isValid;
                }
                else
                {
                    response.Success = true;
                    response.Message = "No usage data available";
                    response.Data = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating tenant limits for {TenantId}", tenantId);
                response.Success = false;
                response.Message = "An error occurred while validating limits";
            }

            return response;
        }

        #region Private Methods

        private void InitializeMockData()
        {
            // Create sample tenants
            var tenant1 = new Tenant
            {
                Id = 1,
                Name = "Tampa Fire Department",
                Slug = "tampa-fire",
                AdminEmail = "admin@tampafire.gov",
                Description = "Emergency management for Tampa Fire Department",
                Industry = "Emergency Services",
                Region = "Florida",
                Status = TenantStatus.Active,
                MaxUsers = 25,
                MaxIncidents = 2000,
                MaxStorageGB = 20,
                CustomBranding = true,
                AdvancedAnalytics = true,
                ApiAccess = true,
                PrioritySupport = false,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            };

            var tenant2 = new Tenant
            {
                Id = 2,
                Name = "Hillsborough County EMS",
                Slug = "hillsborough-ems",
                AdminEmail = "admin@hillsboroughems.gov",
                Description = "Emergency medical services for Hillsborough County",
                Industry = "Emergency Services",
                Region = "Florida",
                Status = TenantStatus.Active,
                MaxUsers = 100,
                MaxIncidents = 10000,
                MaxStorageGB = 100,
                CustomBranding = true,
                AdvancedAnalytics = true,
                ApiAccess = true,
                PrioritySupport = true,
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            };

            _tenants[1] = tenant1;
            _tenants[2] = tenant2;

            // Create sample subscriptions
            var sub1 = new Subscription
            {
                Id = 1,
                TenantId = 1,
                PlanName = "Professional",
                Status = SubscriptionStatus.Active,
                StartDate = DateTime.UtcNow.AddDays(-30),
                MonthlyPrice = 299.00m,
                MaxUsers = 25,
                MaxIncidents = 2000,
                MaxStorageGB = 20,
                CustomBranding = true,
                AdvancedAnalytics = true,
                ApiAccess = true,
                PrioritySupport = false,
                BillingCycle = BillingCycle.Monthly,
                NextBillingDate = DateTime.UtcNow.AddDays(5),
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            };

            var sub2 = new Subscription
            {
                Id = 2,
                TenantId = 2,
                PlanName = "Enterprise",
                Status = SubscriptionStatus.Active,
                StartDate = DateTime.UtcNow.AddDays(-15),
                MonthlyPrice = 799.00m,
                MaxUsers = 100,
                MaxIncidents = 10000,
                MaxStorageGB = 100,
                CustomBranding = true,
                AdvancedAnalytics = true,
                ApiAccess = true,
                PrioritySupport = true,
                BillingCycle = BillingCycle.Monthly,
                NextBillingDate = DateTime.UtcNow.AddDays(20),
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            };

            _subscriptions[1] = sub1;
            _subscriptions[2] = sub2;

            // Create sample billing info
            var billing1 = new BillingInfo
            {
                Id = 1,
                TenantId = 1,
                CompanyName = "Tampa Fire Department",
                ContactName = "John Smith",
                Email = "billing@tampafire.gov",
                Phone = "+1-813-555-0100",
                Address = "123 Fire Station Way",
                City = "Tampa",
                State = "FL",
                PostalCode = "33602",
                Country = "USA",
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            };

            var billing2 = new BillingInfo
            {
                Id = 2,
                TenantId = 2,
                CompanyName = "Hillsborough County EMS",
                ContactName = "Sarah Johnson",
                Email = "billing@hillsboroughems.gov",
                Phone = "+1-813-555-0200",
                Address = "456 EMS Boulevard",
                City = "Tampa",
                State = "FL",
                PostalCode = "33603",
                Country = "USA",
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            };

            _billingInfos[1] = billing1;
            _billingInfos[2] = billing2;

            // Initialize usage metrics
            _usageMetrics[1] = new List<UsageMetrics>();
            _usageMetrics[2] = new List<UsageMetrics>();

            // Initialize invoices
            _invoices[1] = new List<Invoice>();
            _invoices[2] = new List<Invoice>();
        }

        private decimal CalculateUsagePercentage(Tenant tenant, UsageMetrics? usage)
        {
            if (usage == null) return 0;

            var userPercentage = (decimal)usage.ActiveUsers / tenant.MaxUsers * 100;
            var incidentPercentage = (decimal)usage.TotalIncidents / tenant.MaxIncidents * 100;
            var storagePercentage = (decimal)usage.StorageUsedMB / (tenant.MaxStorageGB * 1024) * 100;

            return Math.Max(Math.Max(userPercentage, incidentPercentage), storagePercentage);
        }

        #endregion
    }
} 