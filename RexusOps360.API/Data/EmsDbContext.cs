using Microsoft.EntityFrameworkCore;
using RexusOps360.API.Models;

namespace RexusOps360.API.Data
{
    public class EmsDbContext : DbContext
    {
        public EmsDbContext(DbContextOptions<EmsDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Incident> Incidents { get; set; }
        public DbSet<Responder> Responders { get; set; }
        public DbSet<Equipment> Equipment { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<SystemIntegration> SystemIntegrations { get; set; }
        public DbSet<IntegrationData> IntegrationData { get; set; }
        public DbSet<Hotspot> Hotspots { get; set; }
        public DbSet<HotspotAlert> HotspotAlerts { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        // Event Management Entities
        public DbSet<Event> Events { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Registration> Registrations { get; set; }
        public DbSet<Speaker> Speakers { get; set; }
        public DbSet<EventSpeaker> EventSpeakers { get; set; }
        public DbSet<SessionSpeaker> SessionSpeakers { get; set; }
        public DbSet<SessionAttendee> SessionAttendees { get; set; }
        public DbSet<EventSponsor> EventSponsors { get; set; }
        public DbSet<Sponsor> Sponsors { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(20);
                entity.Property(e => e.FirstName).HasMaxLength(50);
                entity.Property(e => e.LastName).HasMaxLength(50);
                entity.Property(e => e.Department).HasMaxLength(50);
                
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.Role);
            });

            // Incident configuration
            modelBuilder.Entity<Incident>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Type).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Location).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Priority).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
                entity.Property(e => e.UtilityType).HasMaxLength(50);
                entity.Property(e => e.Category).HasMaxLength(100);
                entity.Property(e => e.Zone).HasMaxLength(50);
                entity.Property(e => e.ClusterId).HasMaxLength(50);
                entity.Property(e => e.ContactInfo).HasMaxLength(500);
                entity.Property(e => e.Remarks).HasMaxLength(500);
                entity.Property(e => e.PhotoPath).HasMaxLength(500);
                entity.Property(e => e.AssignedResponders).HasMaxLength(500);
                entity.Property(e => e.EquipmentNeeded).HasMaxLength(500);
                entity.Property(e => e.ReportedBy).HasMaxLength(100);
                
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.Priority);
                entity.HasIndex(e => e.UtilityType);
                entity.HasIndex(e => e.ClusterId);
                entity.HasIndex(e => e.Zone);
            });

            // Responder configuration
            modelBuilder.Entity<Responder>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ContactNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.CurrentLocation).HasMaxLength(200);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Specializations).HasMaxLength(500);
                
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.Role);
            });

            // Equipment configuration
            modelBuilder.Entity<Equipment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Location).HasMaxLength(200);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
                entity.Property(e => e.MaintenanceNotes).HasMaxLength(500);
                
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.Type);
            });

            // Audit Log configuration
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
                entity.Property(e => e.EntityType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.EntityId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Details).HasMaxLength(1000);
                entity.Property(e => e.IpAddress).HasMaxLength(45);
                entity.Property(e => e.UserAgent).HasMaxLength(500);
                
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Action);
                entity.HasIndex(e => e.EntityType);
            });

            // System Integration configuration
            modelBuilder.Entity<SystemIntegration>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Endpoint).IsRequired().HasMaxLength(200);
                entity.Property(e => e.ApiKey).HasMaxLength(100);
                entity.Property(e => e.Username).HasMaxLength(100);
                entity.Property(e => e.Password).HasMaxLength(100);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Protocol).HasMaxLength(50);
                
                entity.HasIndex(e => e.Type);
                entity.HasIndex(e => e.Status);
            });

            // Integration Data configuration
            modelBuilder.Entity<IntegrationData>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DataType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.DataValue).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Unit).HasMaxLength(100);
                entity.Property(e => e.Location).HasMaxLength(200);
                entity.Property(e => e.AlertLevel).HasMaxLength(50);
                
                entity.HasIndex(e => e.IntegrationId);
                entity.HasIndex(e => e.DataType);
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.IsAlert);
            });

            // Hotspot configuration
            modelBuilder.Entity<Hotspot>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
                entity.Property(e => e.UtilityType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Location).HasMaxLength(200);
                entity.Property(e => e.Severity).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                
                entity.HasIndex(e => e.UtilityType);
                entity.HasIndex(e => e.Severity);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.FirstDetected);
            });

            // Hotspot Alert configuration
            modelBuilder.Entity<HotspotAlert>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Message).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.AlertLevel).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.AcknowledgedBy).HasMaxLength(100);
                
                entity.HasIndex(e => e.HotspotId);
                entity.HasIndex(e => e.AlertLevel);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.CreatedAt);
            });

            // Seed Users
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    Email = "admin@rexusops360.com",
                    PasswordHash = "hashed_password_here",
                    Role = "Admin",
                    FirstName = "System",
                    LastName = "Administrator",
                    Department = "IT",
                    CreatedAt = new DateTime(2024, 6, 1, 8, 0, 0, DateTimeKind.Utc)
                },
                new User
                {
                    Id = 2,
                    Username = "dispatcher1",
                    Email = "dispatcher1@rexusops360.com",
                    PasswordHash = "hashed_password_here",
                    Role = "Dispatcher",
                    FirstName = "John",
                    LastName = "Dispatcher",
                    Department = "Operations",
                    CreatedAt = new DateTime(2024, 6, 1, 8, 0, 0, DateTimeKind.Utc)
                },
                new User
                {
                    Id = 3,
                    Username = "responder1",
                    Email = "responder1@rexusops360.com",
                    PasswordHash = "hashed_password_here",
                    Role = "Responder",
                    FirstName = "Sarah",
                    LastName = "Responder",
                    Department = "Emergency Response",
                    CreatedAt = new DateTime(2024, 6, 1, 8, 0, 0, DateTimeKind.Utc)
                }
            );

            // Seed Responders
            modelBuilder.Entity<Responder>().HasData(
                new Responder
                {
                    Id = 1,
                    Name = "John Smith",
                    Role = "EMT",
                    ContactNumber = "555-0101",
                    CurrentLocation = "Downtown Tampa",
                    Status = "Available",
                    Specializations = "Medical Emergency,Trauma",
                    CreatedAt = new DateTime(2024, 6, 1, 8, 0, 0, DateTimeKind.Utc)
                },
                new Responder
                {
                    Id = 2,
                    Name = "Maria Garcia",
                    Role = "Firefighter",
                    ContactNumber = "555-0102",
                    CurrentLocation = "West Tampa",
                    Status = "Available",
                    Specializations = "Fire Emergency,Rescue",
                    CreatedAt = new DateTime(2024, 6, 1, 8, 0, 0, DateTimeKind.Utc)
                },
                new Responder
                {
                    Id = 3,
                    Name = "David Johnson",
                    Role = "Police Officer",
                    ContactNumber = "555-0103",
                    CurrentLocation = "North Tampa",
                    Status = "On Call",
                    Specializations = "Traffic Control,Security",
                    CreatedAt = new DateTime(2024, 6, 1, 8, 0, 0, DateTimeKind.Utc)
                }
            );

            // Seed Equipment
            modelBuilder.Entity<Equipment>().HasData(
                new Equipment
                {
                    Id = 1,
                    Name = "Ambulance Unit 1",
                    Type = "Vehicle",
                    Quantity = 1,
                    AvailableQuantity = 1,
                    Location = "Downtown Station",
                    Status = "Available",
                    LastMaintenance = new DateTime(2024, 5, 15, 8, 0, 0, DateTimeKind.Utc),
                    CreatedAt = new DateTime(2024, 6, 1, 8, 0, 0, DateTimeKind.Utc)
                },
                new Equipment
                {
                    Id = 2,
                    Name = "Fire Truck 1",
                    Type = "Vehicle",
                    Quantity = 1,
                    AvailableQuantity = 1,
                    Location = "West Tampa Station",
                    Status = "Available",
                    LastMaintenance = new DateTime(2024, 5, 20, 8, 0, 0, DateTimeKind.Utc),
                    CreatedAt = new DateTime(2024, 6, 1, 8, 0, 0, DateTimeKind.Utc)
                },
                new Equipment
                {
                    Id = 3,
                    Name = "Defibrillator",
                    Type = "Medical",
                    Quantity = 5,
                    AvailableQuantity = 5,
                    Location = "Central Warehouse",
                    Status = "Available",
                    LastMaintenance = new DateTime(2024, 5, 25, 8, 0, 0, DateTimeKind.Utc),
                    CreatedAt = new DateTime(2024, 6, 1, 8, 0, 0, DateTimeKind.Utc)
                }
            );

            // Seed Incidents with enhanced data
            modelBuilder.Entity<Incident>().HasData(
                new Incident
                {
                    Id = 1,
                    Type = "Medical Emergency",
                    Location = "Downtown Tampa, FL",
                    Description = "Cardiac arrest reported at office building",
                    Priority = "High",
                    Status = "Active",
                    UtilityType = "Combined",
                    Category = "Medical Emergency",
                    Zone = "Downtown",
                    SeverityLevel = 4,
                    ContactInfo = "John Doe, 555-1234",
                    Remarks = "Patient conscious, responding to treatment",
                    AssignedResponders = "1",
                    EquipmentNeeded = "Defibrillator,Ambulance",
                    ReportedBy = "John Doe",
                    CreatedAt = new DateTime(2024, 6, 1, 9, 0, 0, DateTimeKind.Utc)
                },
                new Incident
                {
                    Id = 2,
                    Type = "Sewer Overflow",
                    Location = "West Tampa, FL",
                    Description = "Multiple sewer overflow reports in residential area",
                    Priority = "High",
                    Status = "Active",
                    UtilityType = "Sewer",
                    Category = "Sewer Overflow",
                    Zone = "West Tampa",
                    SeverityLevel = 5,
                    ContactInfo = "Multiple residents affected",
                    Remarks = "Heavy rainfall causing system overload",
                    AssignedResponders = "2,3",
                    EquipmentNeeded = "Pump Truck,Vacuum Truck",
                    ReportedBy = "City Inspector",
                    CreatedAt = new DateTime(2024, 6, 1, 9, 15, 0, DateTimeKind.Utc)
                },
                new Incident
                {
                    Id = 3,
                    Type = "Water Main Break",
                    Location = "North Tampa, FL",
                    Description = "Water main break causing low pressure",
                    Priority = "Medium",
                    Status = "Active",
                    UtilityType = "Water",
                    Category = "Water Main Break",
                    Zone = "North Tampa",
                    SeverityLevel = 3,
                    ContactInfo = "Utility Department",
                    Remarks = "Crew dispatched, estimated repair time 4 hours",
                    AssignedResponders = "2",
                    EquipmentNeeded = "Excavator,Water Truck",
                    ReportedBy = "Utility Worker",
                    CreatedAt = new DateTime(2024, 6, 1, 7, 0, 0, DateTimeKind.Utc)
                }
            );

            // Seed System Integrations
            modelBuilder.Entity<SystemIntegration>().HasData(
                new SystemIntegration
                {
                    Id = 1,
                    Name = "SCADA System",
                    Type = "SCADA",
                    Endpoint = "https://scada.rexusops360.com/api",
                    Status = "Active",
                    Description = "Main SCADA system for water and sewer operations",
                    Protocol = "REST",
                    PollingIntervalSeconds = 30,
                    CreatedAt = new DateTime(2024, 6, 1, 8, 0, 0, DateTimeKind.Utc)
                },
                new SystemIntegration
                {
                    Id = 2,
                    Name = "GPS Tracking System",
                    Type = "GPS",
                    Endpoint = "https://gps.rexusops360.com/api",
                    Status = "Active",
                    Description = "Vehicle and asset GPS tracking system",
                    Protocol = "REST",
                    PollingIntervalSeconds = 15,
                    CreatedAt = new DateTime(2024, 6, 1, 8, 0, 0, DateTimeKind.Utc)
                },
                new SystemIntegration
                {
                    Id = 3,
                    Name = "Weather Service",
                    Type = "Weather",
                    Endpoint = "https://api.openweathermap.org/data/2.5/weather",
                    ApiKey = "demo_api_key",
                    Status = "Active",
                    Description = "Weather data integration for predictive alerts",
                    Protocol = "REST",
                    PollingIntervalSeconds = 300,
                    CreatedAt = new DateTime(2024, 6, 1, 8, 0, 0, DateTimeKind.Utc)
                }
            );

            // Event Management Entity Configurations
            modelBuilder.Entity<Event>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Location).HasMaxLength(200);
                entity.Property(e => e.VirtualMeetingUrl).HasMaxLength(500);
                entity.Property(e => e.TimeZone).HasMaxLength(50);
                entity.Property(e => e.Currency).HasMaxLength(10);
                entity.Property(e => e.BrandingLogoUrl).HasMaxLength(500);
                entity.Property(e => e.BrandingColor).HasMaxLength(20);
                entity.Property(e => e.CustomCss).HasMaxLength(2000);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.UpdatedBy).HasMaxLength(100);
                
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.Type);
                entity.HasIndex(e => e.StartDate);
            });

            modelBuilder.Entity<Session>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Location).HasMaxLength(200);
                entity.Property(e => e.VirtualMeetingUrl).HasMaxLength(500);
                entity.Property(e => e.Track).HasMaxLength(100);
                entity.Property(e => e.Materials).HasMaxLength(500);
                
                entity.HasIndex(e => e.EventId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.Type);
                entity.HasIndex(e => e.StartTime);
            });

            modelBuilder.Entity<Registration>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.Organization).HasMaxLength(200);
                entity.Property(e => e.JobTitle).HasMaxLength(100);
                entity.Property(e => e.Address).HasMaxLength(500);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.State).HasMaxLength(50);
                entity.Property(e => e.ZipCode).HasMaxLength(20);
                entity.Property(e => e.Country).HasMaxLength(100);
                entity.Property(e => e.SpecialRequirements).HasMaxLength(500);
                entity.Property(e => e.DietaryRestrictions).HasMaxLength(200);
                entity.Property(e => e.PaymentMethod).HasMaxLength(50);
                entity.Property(e => e.SmsPhone).HasMaxLength(20);
                
                entity.HasIndex(e => e.EventId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.Type);
                entity.HasIndex(e => e.RegistrationDate);
            });

            modelBuilder.Entity<Speaker>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Title).HasMaxLength(200);
                entity.Property(e => e.Organization).HasMaxLength(200);
                entity.Property(e => e.Bio).HasMaxLength(1000);
                entity.Property(e => e.Email).HasMaxLength(200);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.PhotoUrl).HasMaxLength(500);
                entity.Property(e => e.LinkedInUrl).HasMaxLength(500);
                entity.Property(e => e.TwitterUrl).HasMaxLength(500);
                
            });

            modelBuilder.Entity<EventSpeaker>(entity =>
            {
                entity.HasKey(e => new { e.EventId, e.SpeakerId });
                entity.Property(e => e.Role).HasMaxLength(100);
                
                entity.HasOne<Event>()
                    .WithMany(e => e.Speakers)
                    .HasForeignKey(es => es.EventId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne<Speaker>()
                    .WithMany()
                    .HasForeignKey(es => es.SpeakerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<SessionSpeaker>(entity =>
            {
                entity.HasKey(e => new { e.SessionId, e.SpeakerId });
                entity.Property(e => e.Role).HasMaxLength(100);
                
                entity.HasOne<Session>()
                    .WithMany(s => s.Speakers)
                    .HasForeignKey(ss => ss.SessionId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne<Speaker>()
                    .WithMany()
                    .HasForeignKey(ss => ss.SpeakerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<SessionAttendee>(entity =>
            {
                entity.HasKey(e => new { e.SessionId, e.RegistrationId });
                
                entity.HasOne<Session>()
                    .WithMany(s => s.Attendees)
                    .HasForeignKey(sa => sa.SessionId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne<Registration>()
                    .WithMany()
                    .HasForeignKey(sa => sa.RegistrationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Sponsor>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.LogoUrl).HasMaxLength(500);
                entity.Property(e => e.Website).HasMaxLength(200);
                entity.Property(e => e.ContactEmail).HasMaxLength(200);
                entity.Property(e => e.ContactPhone).HasMaxLength(20);
                
            });

            modelBuilder.Entity<EventSponsor>(entity =>
            {
                entity.HasKey(e => new { e.EventId, e.SponsorId });
                entity.Property(e => e.SponsorshipLevel).HasMaxLength(100);
                
                entity.HasOne<Event>()
                    .WithMany(e => e.Sponsors)
                    .HasForeignKey(es => es.EventId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne<Sponsor>()
                    .WithMany(s => s.Events)
                    .HasForeignKey(es => es.SponsorId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
} 