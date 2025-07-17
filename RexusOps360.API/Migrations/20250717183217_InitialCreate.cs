using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RexusOps360.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Equipment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    AvailableQuantity = table.Column<int>(type: "int", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Barcode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LastMaintenance = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipment", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Incidents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AssignedResponders = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    EquipmentNeeded = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PhotoPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReportedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Incidents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Responders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ContactNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CurrentLocation = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Specializations = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Responders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Equipment",
                columns: new[] { "Id", "AvailableQuantity", "Barcode", "CreatedAt", "LastMaintenance", "Location", "Name", "Quantity", "Status", "Type" },
                values: new object[,]
                {
                    { 1, 4, "DEF001", new DateTime(2025, 7, 17, 18, 32, 17, 274, DateTimeKind.Utc).AddTicks(5336), null, "Main Station", "Defibrillator", 5, "Available", "Medical" },
                    { 2, 2, "AMB001", new DateTime(2025, 7, 17, 18, 32, 17, 274, DateTimeKind.Utc).AddTicks(5410), null, "Downtown Station", "Ambulance", 3, "Available", "Transport" },
                    { 3, 8, "RAD001", new DateTime(2025, 7, 17, 18, 32, 17, 274, DateTimeKind.Utc).AddTicks(5411), null, "Communication Center", "Radio Communication", 10, "Available", "Communication" }
                });

            migrationBuilder.InsertData(
                table: "Incidents",
                columns: new[] { "Id", "AssignedResponders", "CreatedAt", "Description", "EquipmentNeeded", "Location", "PhotoPath", "Priority", "ReportedBy", "Status", "Type", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "1", new DateTime(2025, 7, 17, 18, 32, 17, 274, DateTimeKind.Utc).AddTicks(6309), "Cardiac arrest reported at office building", "Defibrillator,Ambulance", "Downtown Tampa, FL", null, "High", "John Doe", "Active", "Medical Emergency", null },
                    { 2, "2,3", new DateTime(2025, 7, 17, 18, 32, 17, 274, DateTimeKind.Utc).AddTicks(6381), "Multi-vehicle collision on highway", "Ambulance,Fire Truck", "I-275, Tampa, FL", null, "Medium", "Highway Patrol", "Active", "Traffic Accident", null },
                    { 3, "3", new DateTime(2025, 7, 17, 16, 32, 17, 274, DateTimeKind.Utc).AddTicks(6382), "Kitchen fire in residential building", "Fire Truck,Water Tanker", "West Tampa, FL", null, "High", "Building Manager", "Resolved", "Fire Emergency", new DateTime(2025, 7, 17, 17, 32, 17, 274, DateTimeKind.Utc).AddTicks(6432) }
                });

            migrationBuilder.InsertData(
                table: "Responders",
                columns: new[] { "Id", "ContactNumber", "CreatedAt", "CurrentLocation", "Name", "Role", "Specializations", "Status" },
                values: new object[,]
                {
                    { 1, "813-555-0101", new DateTime(2025, 7, 17, 18, 32, 17, 274, DateTimeKind.Utc).AddTicks(4320), "Downtown Tampa", "John Smith", "Paramedic", "Cardiac,Trauma", "Available" },
                    { 2, "813-555-0102", new DateTime(2025, 7, 17, 18, 32, 17, 274, DateTimeKind.Utc).AddTicks(4405), "Airport Area", "Sarah Johnson", "EMT", "Basic Life Support", "Available" },
                    { 3, "813-555-0103", new DateTime(2025, 7, 17, 18, 32, 17, 274, DateTimeKind.Utc).AddTicks(4406), "West Tampa", "Mike Davis", "Firefighter", "Fire Suppression,Rescue", "Available" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Address", "CreatedAt", "Email", "FullName", "IsActive", "LastLoginAt", "PasswordHash", "Phone", "Role", "TenantId", "Username" },
                values: new object[,]
                {
                    { 1, "Tampa, FL", new DateTime(2025, 7, 17, 18, 32, 17, 274, DateTimeKind.Utc).AddTicks(522), "admin@emstampa.com", "System Administrator", true, null, "", "813-555-0001", "Admin", null, "admin" },
                    { 2, "Tampa, FL", new DateTime(2025, 7, 17, 18, 32, 17, 274, DateTimeKind.Utc).AddTicks(607), "dispatcher@emstampa.com", "Emergency Dispatcher", true, null, "", "813-555-0002", "Dispatcher", null, "dispatcher" },
                    { 3, "Tampa, FL", new DateTime(2025, 7, 17, 18, 32, 17, 274, DateTimeKind.Utc).AddTicks(608), "responder@emstampa.com", "Emergency Responder", true, null, "", "813-555-0003", "Responder", null, "responder" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EventType",
                table: "AuditLogs",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Severity",
                table: "AuditLogs",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Timestamp",
                table: "AuditLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_Barcode",
                table: "Equipment",
                column: "Barcode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_Status",
                table: "Equipment",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_Type",
                table: "Equipment",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_CreatedAt",
                table: "Incidents",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_Priority",
                table: "Incidents",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_Status",
                table: "Incidents",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Responders_Role",
                table: "Responders",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_Responders_Status",
                table: "Responders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "Equipment");

            migrationBuilder.DropTable(
                name: "Incidents");

            migrationBuilder.DropTable(
                name: "Responders");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
