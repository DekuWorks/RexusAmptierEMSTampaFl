# Event Management System - RexusOps360

## 🎯 Overview

The **Event Management System** is a comprehensive SaaS solution designed for transportation agencies to manage internal and external events. Built on the robust RexusOps360 platform, it provides complete event lifecycle management from planning through execution and reporting.

## ✨ Key Features

### 🏢 **Multi-Tenant Architecture**
- **Secure tenant isolation** - Each organization's data is completely separated
- **Role-based access control** - Admin, EventManager, CheckInStaff roles
- **Custom branding** - Organization-specific logos, colors, and styling
- **Subscription management** - Plan-based feature access and usage limits

### 📅 **Event Planning & Management**
- **Multiple event types** - In-person, virtual, and hybrid events
- **Session scheduling** - Detailed agenda management with time slots
- **Speaker coordination** - Speaker profiles, assignments, and management
- **Event branding** - Custom logos, colors, and CSS styling
- **Status tracking** - Draft, Published, RegistrationOpen, InProgress, Completed

### 👥 **Registration & Payment**
- **Online registration forms** - Comprehensive attendee data capture
- **Group registrations** - Bulk registration with automated confirmations
- **Payment processing** - Optional payment integration with invoicing
- **Check-in functionality** - On-site and virtual check-in capabilities
- **Special requirements** - Dietary restrictions, accessibility needs

### 📧 **Communication & Engagement**
- **Email notifications** - Invites, reminders, confirmations, updates
- **Calendar integration** - Outlook and Google Calendar support
- **SMS notifications** - Optional mobile messaging for updates
- **Real-time updates** - Live event status and session updates

### 🏨 **Travel & Lodging Support**
- **Travel preferences** - Collect attendee travel requirements
- **Hotel coordination** - Room block management and reservations
- **Accommodation details** - Lodging preferences and special requests
- **Partner integration** - Travel and accommodation service providers

### 📊 **Analytics & Reporting**
- **Real-time dashboards** - Live attendee data and event metrics
- **Session analytics** - Attendance tracking and engagement metrics
- **Export capabilities** - Excel and PDF report generation
- **Performance insights** - Registration trends and event success metrics

### 🔐 **User Access Management**
- **Role-based permissions** - Admin, EventManager, CheckInStaff, Attendee
- **Department separation** - Multi-department event management
- **Event-level access** - Granular permissions per event
- **Audit logging** - Complete activity tracking and compliance

### 📱 **Mobile Accessibility**
- **Responsive design** - Mobile-friendly web interface
- **Mobile check-in** - QR code scanning and mobile check-in
- **Event schedules** - Mobile-optimized agenda viewing
- **Real-time updates** - Push notifications for event changes

## 🏗️ Architecture

### Technology Stack

| Component | Technology | Purpose |
|-----------|------------|---------|
| **Backend API** | ASP.NET Core 6 | RESTful API with SignalR |
| **Database** | Entity Framework Core | Data persistence and migrations |
| **Authentication** | JWT Tokens | Secure API access |
| **Real-time** | SignalR | Live updates and notifications |
| **Frontend** | HTML5, CSS3, JavaScript | Responsive web interface |
| **Charts** | Chart.js | Analytics and reporting |
| **UI Framework** | Bootstrap 5 | Modern responsive design |

### System Components

```
Event Management System/
├── Models/
│   ├── EventModels.cs           # Core event entities
│   ├── SaasModels.cs           # Multi-tenant support
│   └── ValidationModels.cs     # Input validation
├── Services/
│   ├── EventManagementService.cs # Business logic
│   ├── AuthService.cs          # Authentication
│   └── SaasService.cs         # SaaS features
├── Controllers/
│   ├── EventManagementController.cs # REST API endpoints
│   ├── AuthController.cs       # Authentication
│   └── SaasController.cs      # SaaS management
└── Frontend/
    ├── event-management.html   # Main dashboard
    ├── mobile-checkin.html    # Mobile interface
    └── event-registration.html # Public registration
```

## 🔧 API Reference

### Authentication Endpoints

```http
POST /api/auth/login              # User authentication
POST /api/auth/register           # User registration
POST /api/auth/logout             # Session termination
GET  /api/auth/me                 # Current user info
```

### Event Management Endpoints

#### Events
```http
GET    /api/eventmanagement/events              # List all events
POST   /api/eventmanagement/events              # Create new event
GET    /api/eventmanagement/events/{id}         # Get specific event
PUT    /api/eventmanagement/events/{id}         # Update event
DELETE /api/eventmanagement/events/{id}         # Delete event
```

#### Sessions
```http
GET    /api/eventmanagement/sessions            # List all sessions
POST   /api/eventmanagement/sessions            # Create new session
GET    /api/eventmanagement/sessions/{id}       # Get specific session
PUT    /api/eventmanagement/sessions/{id}       # Update session
DELETE /api/eventmanagement/sessions/{id}       # Delete session
GET    /api/eventmanagement/events/{id}/sessions # Get event sessions
```

#### Registrations
```http
POST   /api/eventmanagement/registrations       # Create registration
GET    /api/eventmanagement/registrations/{id}  # Get registration
GET    /api/eventmanagement/events/{id}/registrations # Get event registrations
POST   /api/eventmanagement/registrations/{id}/checkin # Check-in attendee
POST   /api/eventmanagement/registrations/{id}/cancel # Cancel registration
```

#### Speakers
```http
GET    /api/eventmanagement/speakers            # List all speakers
POST   /api/eventmanagement/speakers            # Create new speaker
GET    /api/eventmanagement/speakers/{id}       # Get specific speaker
PUT    /api/eventmanagement/speakers/{id}       # Update speaker
DELETE /api/eventmanagement/speakers/{id}       # Delete speaker
```

#### Analytics
```http
GET    /api/eventmanagement/events/{id}/analytics # Event analytics
```

### Request/Response Examples

#### Create Event
```json
POST /api/eventmanagement/events
{
  "title": "Transportation Innovation Summit 2024",
  "description": "Annual conference for transportation professionals",
  "startDate": "2024-03-15T09:00:00Z",
  "endDate": "2024-03-17T17:00:00Z",
  "type": "Hybrid",
  "location": "Tampa Convention Center",
  "virtualMeetingUrl": "https://meet.example.com/summit2024",
  "maxAttendees": 500,
  "registrationFee": 299.00,
  "currency": "USD",
  "isPublic": true,
  "requiresApproval": false,
  "brandingLogoUrl": "https://example.com/logo.png",
  "brandingColor": "#2c3e50"
}
```

#### Create Registration
```json
POST /api/eventmanagement/registrations
{
  "eventId": 1,
  "firstName": "John",
  "lastName": "Smith",
  "email": "john.smith@company.com",
  "phone": "+1-555-0123",
  "organization": "ABC Transportation",
  "jobTitle": "Operations Manager",
  "address": "123 Main St",
  "city": "Tampa",
  "state": "FL",
  "zipCode": "33601",
  "country": "USA",
  "type": "Individual",
  "specialRequirements": "Wheelchair accessible seating",
  "dietaryRestrictions": "Vegetarian",
  "emailNotifications": true,
  "smsNotifications": false,
  "sessionIds": [1, 3, 5]
}
```

#### Create Session
```json
POST /api/eventmanagement/sessions
{
  "eventId": 1,
  "title": "Future of Autonomous Vehicles",
  "description": "Discussion on autonomous vehicle technology and implementation",
  "startTime": "2024-03-15T10:00:00Z",
  "endTime": "2024-03-15T11:30:00Z",
  "location": "Main Ballroom",
  "virtualMeetingUrl": "https://meet.example.com/autonomous",
  "maxCapacity": 200,
  "track": "Technology",
  "type": "Panel",
  "materials": "https://example.com/materials/autonomous.pdf",
  "speakerIds": [1, 2, 3]
}
```

## 📊 Data Models

### Core Entities

#### Event
```csharp
public class Event
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public EventType Type { get; set; }
    public EventStatus Status { get; set; }
    public string? Location { get; set; }
    public string? VirtualMeetingUrl { get; set; }
    public int MaxAttendees { get; set; }
    public decimal? RegistrationFee { get; set; }
    public string? Currency { get; set; }
    public bool IsPublic { get; set; }
    public bool RequiresApproval { get; set; }
    public string? BrandingLogoUrl { get; set; }
    public string? BrandingColor { get; set; }
    public string? CustomCss { get; set; }
    public int TenantId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

#### Registration
```csharp
public class Registration
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string? Phone { get; set; }
    public string? Organization { get; set; }
    public string? JobTitle { get; set; }
    public RegistrationStatus Status { get; set; }
    public RegistrationType Type { get; set; }
    public DateTime RegistrationDate { get; set; }
    public DateTime? CheckInDate { get; set; }
    public string? SpecialRequirements { get; set; }
    public string? DietaryRestrictions { get; set; }
    public decimal? AmountPaid { get; set; }
    public string? PaymentMethod { get; set; }
    public DateTime? PaymentDate { get; set; }
}
```

#### Session
```csharp
public class Session
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Location { get; set; }
    public string? VirtualMeetingUrl { get; set; }
    public int MaxCapacity { get; set; }
    public string? Track { get; set; }
    public SessionType Type { get; set; }
    public SessionStatus Status { get; set; }
    public string? Materials { get; set; }
}
```

#### Speaker
```csharp
public class Speaker
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string? Title { get; set; }
    public string? Organization { get; set; }
    public string? Bio { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? PhotoUrl { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? TwitterUrl { get; set; }
    public int TenantId { get; set; }
}
```

### Enums

```csharp
public enum EventType
{
    InPerson,
    Virtual,
    Hybrid
}

public enum EventStatus
{
    Draft,
    Published,
    RegistrationOpen,
    RegistrationClosed,
    InProgress,
    Completed,
    Cancelled
}

public enum SessionType
{
    Keynote,
    Breakout,
    Panel,
    Workshop,
    Networking,
    Lunch,
    Break
}

public enum RegistrationStatus
{
    Pending,
    Confirmed,
    CheckedIn,
    Cancelled,
    Waitlisted
}

public enum RegistrationType
{
    Individual,
    Group,
    VIP,
    Speaker,
    Sponsor
}
```

## 🚀 Quick Start

### Prerequisites
- .NET 6.0 SDK
- SQL Server or SQLite
- Visual Studio 2022 or VS Code

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/DekuWorks/RexusAmptierEMSTampaFl.git
   cd EMS_Tampa-FL_Amptier
   ```

2. **Navigate to API directory**
   ```bash
   cd RexusOps360.API
   ```

3. **Restore dependencies**
   ```bash
   dotnet restore
   ```

4. **Run the application**
   ```bash
   dotnet run
   ```

5. **Access the application**
   - API: `http://localhost:5000`
   - Swagger UI: `http://localhost:5000`
   - Event Management: `http://localhost:5000/frontend/event-management.html`

### Configuration

#### Environment Variables
```bash
# Database
ConnectionStrings__DefaultConnection="Server=localhost;Database=EventManagementDb;..."

# JWT Authentication
Jwt__Key="YourSuperSecretKeyHere12345678901234567890"
Jwt__Issuer="RexusOps360"
Jwt__Audience="RexusOps360Users"

# Event Management
EventManagement__MaxEventsPerTenant=100
EventManagement__MaxRegistrationsPerEvent=1000
EventManagement__DefaultTimeZone="UTC"
```

#### App Settings
```json
{
  "EventManagement": {
    "MaxEventsPerTenant": 100,
    "MaxRegistrationsPerEvent": 1000,
    "DefaultTimeZone": "UTC",
    "EmailNotifications": {
      "Enabled": true,
      "SmtpServer": "smtp.example.com",
      "SmtpPort": 587,
      "Username": "noreply@example.com",
      "Password": "your_password"
    },
    "SmsNotifications": {
      "Enabled": false,
      "Provider": "Twilio",
      "AccountSid": "your_account_sid",
      "AuthToken": "your_auth_token"
    }
  }
}
```

## 📱 User Interfaces

### Main Dashboard
- **Event overview** - List of all events with status and metrics
- **Quick actions** - Create event, manage registrations, view analytics
- **Real-time updates** - Live event status and registration counts
- **Statistics cards** - Total events, registrations, revenue, active events

### Event Management
- **Event creation** - Comprehensive event setup with branding
- **Session scheduling** - Drag-and-drop agenda management
- **Speaker management** - Speaker profiles and assignments
- **Registration management** - Attendee list and check-in

### Registration System
- **Public registration** - Mobile-friendly registration forms
- **Group registration** - Bulk registration with CSV import
- **Payment processing** - Secure payment integration
- **Check-in interface** - QR code scanning and mobile check-in

### Analytics Dashboard
- **Event metrics** - Registration trends, attendance rates
- **Revenue tracking** - Payment analytics and financial reports
- **Session analytics** - Popular sessions and engagement metrics
- **Export capabilities** - Excel and PDF report generation

## 🔐 Security Features

### Authentication & Authorization
- **JWT token authentication** - Secure API access
- **Role-based access control** - Admin, EventManager, CheckInStaff roles
- **Multi-tenant isolation** - Complete data separation between organizations
- **Audit logging** - Comprehensive activity tracking

### Data Protection
- **Input validation** - Server-side validation and sanitization
- **SQL injection prevention** - Parameterized queries
- **XSS protection** - Content Security Policy headers
- **CSRF protection** - Anti-forgery token validation

### Privacy & Compliance
- **GDPR compliance** - Data protection and privacy controls
- **Data encryption** - At-rest and in-transit encryption
- **Access logging** - Complete audit trail
- **Data retention** - Configurable data retention policies

## 📊 Analytics & Reporting

### Real-time Metrics
- **Registration trends** - Daily, weekly, monthly registration patterns
- **Event performance** - Attendance rates and engagement metrics
- **Revenue tracking** - Payment analytics and financial insights
- **Session analytics** - Popular sessions and speaker performance

### Export Capabilities
- **Excel reports** - Detailed event and registration data
- **PDF reports** - Professional event summaries and analytics
- **CSV exports** - Raw data for external analysis
- **Custom reports** - Configurable report templates

### Dashboard Widgets
- **Registration funnel** - Registration to check-in conversion
- **Revenue charts** - Payment trends and financial metrics
- **Session attendance** - Real-time session attendance tracking
- **Geographic data** - Attendee location analytics

## 🔄 System Integration

### Calendar Integration
- **Outlook integration** - Event sync with Microsoft Outlook
- **Google Calendar** - Event sync with Google Calendar
- **iCal support** - Standard calendar format export
- **Calendar API** - RESTful calendar integration

### Payment Processing
- **Stripe integration** - Secure payment processing
- **PayPal support** - Alternative payment method
- **Invoice generation** - Automated invoice creation
- **Payment tracking** - Complete payment history

### Communication Services
- **Email service** - SMTP integration for notifications
- **SMS service** - Twilio integration for mobile messaging
- **Push notifications** - Real-time mobile notifications
- **Webhook support** - External system integration

## 🚀 Deployment

### Local Development
```bash
# Start the API
cd RexusOps360.API
dotnet run

# Access the application
open http://localhost:5000
```

### Docker Deployment
```bash
# Build the image
docker build -t event-management-system .

# Run the container
docker run -p 5000:5000 event-management-system
```

### Azure Deployment
```bash
# Deploy to Azure App Service
az webapp up --name event-management-system --resource-group EventManagement
```

### AWS Deployment
```bash
# Deploy to AWS ECS
aws ecs create-service --cluster event-management --service-name event-management-api
```

## 🧪 Testing

### Unit Tests
```bash
# Run all tests
cd RexusOps360.API.Tests
dotnet test

# Run specific test category
dotnet test --filter Category=EventManagement
```

### Integration Tests
```bash
# Run integration tests
dotnet test --filter Category=Integration
```

### API Tests
```bash
# Test API endpoints
dotnet test --filter Category=API
```

## 📞 Support

### Documentation
- [API Documentation](http://localhost:5000) - Swagger UI
- [Event Management Guide](event-management.html) - User interface guide
- [Integration Guide](INTEGRATION_GUIDE.md) - System integration

### Contact
- **Email**: support@rexusops360.com
- **Phone**: (813) 555-0123
- **Documentation**: [https://docs.rexusops360.com](https://docs.rexusops360.com)

### Emergency Support
- **24/7 Support**: support@rexusops360.com
- **Critical Issues**: (813) 555-0124
- **System Status**: [https://status.rexusops360.com](https://status.rexusops360.com)

## 🏆 Features Summary

### ✅ Implemented Features

#### Event Management
- [x] Multi-tenant event creation and management
- [x] Session scheduling and agenda management
- [x] Speaker coordination and assignment
- [x] Event branding and customization
- [x] Status tracking and workflow management

#### Registration System
- [x] Online registration forms
- [x] Group registration support
- [x] Payment processing integration
- [x] Check-in functionality
- [x] Special requirements handling

#### Communication
- [x] Email notification system
- [x] Calendar integration
- [x] SMS notifications
- [x] Real-time updates

#### Analytics & Reporting
- [x] Real-time dashboards
- [x] Session analytics
- [x] Export capabilities
- [x] Performance insights

#### Security & Access
- [x] Role-based access control
- [x] Multi-tenant isolation
- [x] Audit logging
- [x] Data encryption

#### Mobile Support
- [x] Responsive web design
- [x] Mobile check-in
- [x] Event schedules
- [x] Real-time notifications

### 🚀 Production Ready

The Event Management System is **production-ready** with:

- ✅ **Enterprise-grade security**
- ✅ **Multi-tenant SaaS architecture**
- ✅ **Comprehensive event management**
- ✅ **Real-time analytics**
- ✅ **Mobile-responsive design**
- ✅ **Complete API documentation**
- ✅ **Extensive testing coverage**

---

**Built with ❤️ for Transportation Agencies**

*RexusOps360 Event Management - Empowering Event Success* 