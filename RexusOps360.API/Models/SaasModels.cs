using System.ComponentModel.DataAnnotations;

namespace RexusOps360.API.Models
{
    public class Tenant
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string Slug { get; set; } = string.Empty; // URL-friendly identifier
        
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string AdminEmail { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string? Description { get; set; }
        
        [StringLength(100)]
        public string? Industry { get; set; }
        
        [StringLength(50)]
        public string? Region { get; set; }
        
        [StringLength(20)]
        public string? Phone { get; set; }
        
        [StringLength(200)]
        public string? Address { get; set; }
        
        [StringLength(100)]
        public string? Website { get; set; }
        
        public TenantStatus Status { get; set; } = TenantStatus.Active;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        public DateTime? TrialEndsAt { get; set; }
        
        // SaaS-specific properties
        public int MaxUsers { get; set; } = 10;
        public int MaxIncidents { get; set; } = 1000;
        public int MaxStorageGB { get; set; } = 10;
        public bool CustomBranding { get; set; } = false;
        public bool AdvancedAnalytics { get; set; } = false;
        public bool ApiAccess { get; set; } = false;
        public bool PrioritySupport { get; set; } = false;
        
        // Navigation properties
        public virtual ICollection<User> Users { get; set; } = new List<User>();
        public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
        public virtual ICollection<BillingInfo> BillingInfos { get; set; } = new List<BillingInfo>();
    }

    public class Subscription
    {
        public int Id { get; set; }
        
        public int TenantId { get; set; }
        public virtual Tenant Tenant { get; set; } = null!;
        
        [Required]
        [StringLength(50)]
        public string PlanName { get; set; } = string.Empty;
        
        public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;
        
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        
        public DateTime? EndDate { get; set; }
        
        public DateTime? TrialEndsAt { get; set; }
        
        public decimal MonthlyPrice { get; set; }
        
        public decimal? SetupFee { get; set; }
        
        public int MaxUsers { get; set; }
        
        public int MaxIncidents { get; set; }
        
        public int MaxStorageGB { get; set; }
        
        public bool CustomBranding { get; set; }
        
        public bool AdvancedAnalytics { get; set; }
        
        public bool ApiAccess { get; set; }
        
        public bool PrioritySupport { get; set; }
        
        public BillingCycle BillingCycle { get; set; } = BillingCycle.Monthly;
        
        public DateTime? NextBillingDate { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        public DateTime? CancelledAt { get; set; }
        
        [StringLength(500)]
        public string? CancellationReason { get; set; }
    }

    public class BillingInfo
    {
        public int Id { get; set; }
        
        public int TenantId { get; set; }
        public virtual Tenant Tenant { get; set; } = null!;
        
        [Required]
        [StringLength(100)]
        public string CompanyName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string ContactName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string? Phone { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Address { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string State { get; set; } = string.Empty;
        
        [Required]
        [StringLength(20)]
        public string PostalCode { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string Country { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string? TaxId { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
    }

    public class Invoice
    {
        public int Id { get; set; }
        
        public int TenantId { get; set; }
        public virtual Tenant Tenant { get; set; } = null!;
        
        [Required]
        [StringLength(50)]
        public string InvoiceNumber { get; set; } = string.Empty;
        
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
        
        public DateTime IssueDate { get; set; } = DateTime.UtcNow;
        
        public DateTime DueDate { get; set; }
        
        public DateTime? PaidDate { get; set; }
        
        public decimal Subtotal { get; set; }
        
        public decimal TaxAmount { get; set; }
        
        public decimal TotalAmount { get; set; }
        
        [StringLength(3)]
        public string Currency { get; set; } = "USD";
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public virtual ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
    }

    public class InvoiceItem
    {
        public int Id { get; set; }
        
        public int InvoiceId { get; set; }
        public virtual Invoice Invoice { get; set; } = null!;
        
        [Required]
        [StringLength(100)]
        public string Description { get; set; } = string.Empty;
        
        public int Quantity { get; set; } = 1;
        
        public decimal UnitPrice { get; set; }
        
        public decimal TotalPrice { get; set; }
        
        [StringLength(50)]
        public string? ItemType { get; set; } // Subscription, Setup, Overage, etc.
    }

    public class UsageMetrics
    {
        public int Id { get; set; }
        
        public int TenantId { get; set; }
        public virtual Tenant Tenant { get; set; } = null!;
        
        public DateTime Date { get; set; }
        
        public int ActiveUsers { get; set; }
        
        public int TotalIncidents { get; set; }
        
        public int StorageUsedMB { get; set; }
        
        public int ApiCalls { get; set; }
        
        public int SupportTickets { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // SaaS-specific request/response models
    public class CreateTenantRequest
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Slug can only contain lowercase letters, numbers, and hyphens")]
        public string Slug { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string AdminEmail { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string? Description { get; set; }
        
        [StringLength(100)]
        public string? Industry { get; set; }
        
        [StringLength(50)]
        public string? Region { get; set; }
        
        [StringLength(20)]
        public string? Phone { get; set; }
        
        [StringLength(200)]
        public string? Address { get; set; }
        
        [StringLength(100)]
        public string? Website { get; set; }
        
        [Required]
        public string PlanName { get; set; } = string.Empty;
        
        public bool StartTrial { get; set; } = true;
    }

    public class UpdateTenantRequest
    {
        [StringLength(100)]
        public string? Name { get; set; }
        
        [StringLength(200)]
        public string? Description { get; set; }
        
        [StringLength(100)]
        public string? Industry { get; set; }
        
        [StringLength(50)]
        public string? Region { get; set; }
        
        [StringLength(20)]
        public string? Phone { get; set; }
        
        [StringLength(200)]
        public string? Address { get; set; }
        
        [StringLength(100)]
        public string? Website { get; set; }
    }

    public class SubscriptionRequest
    {
        [Required]
        public string PlanName { get; set; } = string.Empty;
        
        public BillingCycle BillingCycle { get; set; } = BillingCycle.Monthly;
        
        public bool StartTrial { get; set; } = true;
        
        [StringLength(500)]
        public string? Notes { get; set; }
    }

    public class BillingInfoRequest
    {
        [Required]
        [StringLength(100)]
        public string CompanyName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string ContactName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string? Phone { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Address { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string State { get; set; } = string.Empty;
        
        [Required]
        [StringLength(20)]
        public string PostalCode { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string Country { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string? TaxId { get; set; }
    }

    public class UsageReport
    {
        public int TenantId { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalIncidents { get; set; }
        public int StorageUsedMB { get; set; }
        public int ApiCalls { get; set; }
        public int SupportTickets { get; set; }
        public decimal UsagePercentage { get; set; }
    }

    public class TenantDashboard
    {
        public Tenant Tenant { get; set; } = null!;
        public Subscription? CurrentSubscription { get; set; }
        public BillingInfo? BillingInfo { get; set; }
        public List<Invoice> RecentInvoices { get; set; } = new List<Invoice>();
        public UsageReport CurrentUsage { get; set; } = null!;
        public List<UsageReport> UsageHistory { get; set; } = new List<UsageReport>();
        public Dictionary<string, object> Metrics { get; set; } = new Dictionary<string, object>();
    }

    // Enums
    public enum TenantStatus
    {
        Active,
        Suspended,
        Cancelled,
        Trial
    }

    public enum SubscriptionStatus
    {
        Active,
        PastDue,
        Cancelled,
        Trial,
        Expired
    }

    public enum InvoiceStatus
    {
        Draft,
        Sent,
        Paid,
        Overdue,
        Cancelled
    }

    public enum BillingCycle
    {
        Monthly,
        Quarterly,
        Yearly
    }

    // SaaS Plans
    public static class SaasPlans
    {
        public static readonly Dictionary<string, PlanDetails> Plans = new()
        {
            ["starter"] = new PlanDetails
            {
                Name = "Starter",
                MonthlyPrice = 99.00m,
                MaxUsers = 5,
                MaxIncidents = 500,
                MaxStorageGB = 5,
                CustomBranding = false,
                AdvancedAnalytics = false,
                ApiAccess = false,
                PrioritySupport = false,
                TrialDays = 14
            },
            ["professional"] = new PlanDetails
            {
                Name = "Professional",
                MonthlyPrice = 299.00m,
                MaxUsers = 25,
                MaxIncidents = 2000,
                MaxStorageGB = 20,
                CustomBranding = true,
                AdvancedAnalytics = true,
                ApiAccess = true,
                PrioritySupport = false,
                TrialDays = 14
            },
            ["enterprise"] = new PlanDetails
            {
                Name = "Enterprise",
                MonthlyPrice = 799.00m,
                MaxUsers = 100,
                MaxIncidents = 10000,
                MaxStorageGB = 100,
                CustomBranding = true,
                AdvancedAnalytics = true,
                ApiAccess = true,
                PrioritySupport = true,
                TrialDays = 30
            }
        };
    }

    public class PlanDetails
    {
        public string Name { get; set; } = string.Empty;
        public decimal MonthlyPrice { get; set; }
        public int MaxUsers { get; set; }
        public int MaxIncidents { get; set; }
        public int MaxStorageGB { get; set; }
        public bool CustomBranding { get; set; }
        public bool AdvancedAnalytics { get; set; }
        public bool ApiAccess { get; set; }
        public bool PrioritySupport { get; set; }
        public int TrialDays { get; set; }
    }
} 