using Microsoft.EntityFrameworkCore;
using RexusOps360.API.Models;
using RexusOps360.API.Services;

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.Address).HasMaxLength(200);
                
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Incident configuration
            modelBuilder.Entity<Incident>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Location).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Priority).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
                entity.Property(e => e.PhotoPath).HasMaxLength(500);
                entity.Property(e => e.AssignedResponders).HasMaxLength(500);
                entity.Property(e => e.EquipmentNeeded).HasMaxLength(500);
                entity.Property(e => e.ReportedBy).HasMaxLength(100);
                
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.Priority);
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
                entity.Property(e => e.Barcode).HasMaxLength(50);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
                
                entity.HasIndex(e => e.Type);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.Barcode).IsUnique();
            });

            // AuditLog configuration
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Details).HasMaxLength(1000);
                entity.Property(e => e.IpAddress).HasMaxLength(50);
                entity.Property(e => e.EventType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Severity).IsRequired().HasMaxLength(20);
                
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.EventType);
                entity.HasIndex(e => e.Severity);
            });

            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Users
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    Email = "admin@emstampa.com",
                    FullName = "System Administrator",
                    Role = "Admin",
                    Phone = "813-555-0001",
                    Address = "Tampa, FL",
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 6, 1, 8, 0, 0, DateTimeKind.Utc)
                },
                new User
                {
                    Id = 2,
                    Username = "dispatcher",
                    Email = "dispatcher@emstampa.com",
                    FullName = "Emergency Dispatcher",
                    Role = "Dispatcher",
                    Phone = "813-555-0002",
                    Address = "Tampa, FL",
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 6, 1, 8, 5, 0, DateTimeKind.Utc)
                },
                new User
                {
                    Id = 3,
                    Username = "responder",
                    Email = "responder@emstampa.com",
                    FullName = "Emergency Responder",
                    Role = "Responder",
                    Phone = "813-555-0003",
                    Address = "Tampa, FL",
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 6, 1, 8, 10, 0, DateTimeKind.Utc)
                }
            );

            // Seed Responders
            modelBuilder.Entity<Responder>().HasData(
                new Responder
                {
                    Id = 1,
                    Name = "John Smith",
                    Role = "Paramedic",
                    ContactNumber = "813-555-0101",
                    CurrentLocation = "Downtown Tampa",
                    Status = "Available",
                    Specializations = "Cardiac,Trauma",
                    CreatedAt = new DateTime(2024, 6, 1, 8, 15, 0, DateTimeKind.Utc)
                },
                new Responder
                {
                    Id = 2,
                    Name = "Sarah Johnson",
                    Role = "EMT",
                    ContactNumber = "813-555-0102",
                    CurrentLocation = "Airport Area",
                    Status = "Available",
                    Specializations = "Basic Life Support",
                    CreatedAt = new DateTime(2024, 6, 1, 8, 20, 0, DateTimeKind.Utc)
                },
                new Responder
                {
                    Id = 3,
                    Name = "Mike Davis",
                    Role = "Firefighter",
                    ContactNumber = "813-555-0103",
                    CurrentLocation = "West Tampa",
                    Status = "Available",
                    Specializations = "Fire Suppression,Rescue",
                    CreatedAt = new DateTime(2024, 6, 1, 8, 25, 0, DateTimeKind.Utc)
                }
            );

            // Seed Equipment
            modelBuilder.Entity<Equipment>().HasData(
                new Equipment
                {
                    Id = 1,
                    Name = "Defibrillator",
                    Type = "Medical",
                    Quantity = 5,
                    AvailableQuantity = 4,
                    Location = "Main Station",
                    Barcode = "DEF001",
                    Status = "Available",
                    CreatedAt = new DateTime(2024, 6, 1, 8, 30, 0, DateTimeKind.Utc)
                },
                new Equipment
                {
                    Id = 2,
                    Name = "Ambulance",
                    Type = "Transport",
                    Quantity = 3,
                    AvailableQuantity = 2,
                    Location = "Downtown Station",
                    Barcode = "AMB001",
                    Status = "Available",
                    CreatedAt = new DateTime(2024, 6, 1, 8, 35, 0, DateTimeKind.Utc)
                },
                new Equipment
                {
                    Id = 3,
                    Name = "Radio Communication",
                    Type = "Communication",
                    Quantity = 10,
                    AvailableQuantity = 8,
                    Location = "Communication Center",
                    Barcode = "RAD001",
                    Status = "Available",
                    CreatedAt = new DateTime(2024, 6, 1, 8, 40, 0, DateTimeKind.Utc)
                }
            );

            // Seed Incidents
            modelBuilder.Entity<Incident>().HasData(
                new Incident
                {
                    Id = 1,
                    Type = "Medical Emergency",
                    Location = "Downtown Tampa, FL",
                    Description = "Cardiac arrest reported at office building",
                    Priority = "High",
                    Status = "Active",
                    AssignedResponders = "1",
                    EquipmentNeeded = "Defibrillator,Ambulance",
                    ReportedBy = "John Doe",
                    CreatedAt = new DateTime(2024, 6, 1, 9, 0, 0, DateTimeKind.Utc)
                },
                new Incident
                {
                    Id = 2,
                    Type = "Traffic Accident",
                    Location = "I-275, Tampa, FL",
                    Description = "Multi-vehicle collision on highway",
                    Priority = "Medium",
                    Status = "Active",
                    AssignedResponders = "2,3",
                    EquipmentNeeded = "Ambulance,Fire Truck",
                    ReportedBy = "Highway Patrol",
                    CreatedAt = new DateTime(2024, 6, 1, 9, 15, 0, DateTimeKind.Utc)
                },
                new Incident
                {
                    Id = 3,
                    Type = "Fire Emergency",
                    Location = "West Tampa, FL",
                    Description = "Kitchen fire in residential building",
                    Priority = "High",
                    Status = "Resolved",
                    AssignedResponders = "3",
                    EquipmentNeeded = "Fire Truck,Water Tanker",
                    ReportedBy = "Building Manager",
                    CreatedAt = new DateTime(2024, 6, 1, 7, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2024, 6, 1, 8, 0, 0, DateTimeKind.Utc)
                }
            );
        }
    }
} 