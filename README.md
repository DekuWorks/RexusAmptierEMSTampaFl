# RexusOps360 - Comprehensive Emergency Management & Event Management System

[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)](https://github.com/DekuWorks/RexusAmptierEMSTampaFl)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-6.0-purple.svg)](https://dotnet.microsoft.com/)
[![SaaS](https://img.shields.io/badge/SaaS-Ready-orange.svg)](https://en.wikipedia.org/wiki/Software_as_a_service)
[![AWS](https://img.shields.io/badge/AWS-Deployed-yellow.svg)](https://aws.amazon.com/)

## 🚨 Overview

RexusOps360 is a comprehensive **Emergency Management System (EMS)** and **Event Management System** designed specifically for Tampa, FL and surrounding areas. Built with modern .NET 6 architecture, it provides real-time emergency response coordination, incident management, resource tracking, multi-tenant SaaS capabilities, and complete event lifecycle management.

### ✨ Key Features

#### 🏥 Emergency Management
- **Real-time Emergency Response** - Live incident tracking and coordination
- **Multi-tenant SaaS Platform** - Support for multiple emergency organizations
- **Mobile Responder Interface** - Field operations support
- **GPS Tracking & Mapping** - Real-time location services
- **Advanced Analytics** - Performance metrics and reporting
- **Enterprise Security** - Role-based access control and authentication

#### 🎯 Event Management
- **Complete Event Lifecycle** - Planning through execution and reporting
- **Multi-tenant Architecture** - Secure data isolation for organizations
- **Registration & Payment** - Online registration with payment processing
- **Communication Tools** - Email, SMS, and calendar integration
- **Analytics & Reporting** - Real-time dashboards and insights
- **Mobile Accessibility** - Responsive design and mobile check-in

## 🏗️ Architecture

### Technology Stack

| Component | Technology | Version |
|-----------|------------|---------|
| **Backend API** | ASP.NET Core 6 | 6.0+ |
| **Database** | Entity Framework Core | 6.0+ |
| **Authentication** | JWT Tokens | - |
| **Real-time** | SignalR | 6.0+ |
| **Frontend** | HTML5, CSS3, JavaScript | - |
| **Charts** | Chart.js | 3.7+ |
| **UI Framework** | Bootstrap 5 | 5.1+ |
| **Deployment** | Docker, AWS | - |

### System Components

```
RexusOps360 System/
├── RexusOps360.API/           # Main API backend
│   ├── Controllers/           # API endpoints
│   │   ├── AuthController.cs
│   │   ├── IncidentsController.cs
│   │   ├── EventManagementController.cs
│   │   └── SaasController.cs
│   ├── Models/               # Data models
│   │   ├── EventModels.cs
│   │   ├── SaasModels.cs
│   │   └── ValidationModels.cs
│   ├── Services/             # Business logic
│   │   ├── AuthService.cs
│   │   ├── EventManagementService.cs
│   │   └── SaasService.cs
│   ├── Middleware/           # Custom middleware
│   └── Configuration/        # App configuration
├── frontend/                 # Web interface
│   ├── index.html           # Main EMS dashboard
│   ├── event-management.html # Event management
│   ├── saas-dashboard.html  # SaaS management
│   └── mobile-responder.html # Mobile interface
└── RexusOps360.API.Tests/   # Unit tests
```

## 🚀 Quick Start

### Prerequisites

- [.NET 6.0 SDK](https://dotnet.microsoft.com/download)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
- [SQL Server](https://www.microsoft.com/sql-server) or [SQLite](https://sqlite.org/)
- [Docker](https://www.docker.com/) (for containerized deployment)

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
   - EMS Dashboard: `http://localhost:5000/frontend/`
   - Event Management: `http://localhost:5000/frontend/event-management.html`

### Demo Credentials

| Role | Username | Password | Access Level |
|------|----------|----------|--------------|
| **Admin** | `admin` | `admin123` | Full system access |
| **Dispatcher** | `dispatcher1` | `pass123` | Incident management |
| **EventManager** | `eventmanager1` | `pass123` | Event management |
| **Responder** | `responder1` | `pass123` | Field operations |

## 🔐 Authentication & Security

### Comprehensive Security Features

- **JWT Token Authentication** - Secure API access
- **Role-Based Access Control (RBAC)** - Admin, Dispatcher, EventManager, Responder roles
- **Password Security** - SHA256 hashing with salt
- **Rate Limiting** - 5 failed attempts = 15-minute lockout
- **Input Validation** - Comprehensive data validation
- **CORS Configuration** - Cross-origin request handling
- **Security Headers** - XSS protection and security policies

### Authentication Flow

```mermaid
graph TD
    A[User Login] --> B[Validate Credentials]
    B --> C[Generate JWT Token]
    C --> D[Return Token + User Info]
    D --> E[Access Protected Resources]
    E --> F[Token Validation]
    F --> G[Role-Based Authorization]
```

### API Security Endpoints

```http
POST /api/auth/login              # User authentication
POST /api/auth/register           # User registration
POST /api/auth/logout             # Session termination
POST /api/auth/refresh-token      # Token refresh
POST /api/auth/change-password    # Password change
POST /api/auth/reset-password     # Password reset
GET  /api/auth/me                 # Current user info
PUT  /api/auth/profile            # Profile update
```

## 🏢 SaaS Multi-Tenancy

### Tenant Management

The system supports multiple organizations as tenants:

- **Tampa Fire Department** (Professional Plan)
- **Hillsborough County EMS** (Enterprise Plan)
- **Transportation Agencies** (Event Management)
- **Custom organizations**

### Subscription Plans

| Plan | Price | Users | Incidents/Events | Storage | Features |
|------|-------|-------|------------------|---------|----------|
| **Starter** | $99/month | 5 | 500 | 5 GB | Basic features |
| **Professional** | $299/month | 25 | 2,000 | 20 GB | + Custom branding, Analytics, API |
| **Enterprise** | $799/month | 100 | 10,000 | 100 GB | + Priority support |

### SaaS Features

- ✅ **Multi-tenant isolation**
- ✅ **Subscription management**
- ✅ **Usage tracking and limits**
- ✅ **Billing and invoicing**
- ✅ **Analytics and reporting**
- ✅ **Custom branding**
- ✅ **API access**
- ✅ **Priority support**

### SaaS API Endpoints

```http
# Tenant Management
POST   /api/saas/tenants                    # Create tenant
GET    /api/saas/tenants/{id}               # Get tenant
PUT    /api/saas/tenants/{id}               # Update tenant
DELETE /api/saas/tenants/{id}               # Delete tenant

# Subscription Management
POST   /api/saas/tenants/{id}/subscriptions # Create subscription
GET    /api/saas/subscriptions/{id}         # Get subscription
POST   /api/saas/subscriptions/{id}/cancel  # Cancel subscription

# Billing & Usage
PUT    /api/saas/tenants/{id}/billing       # Update billing info
POST   /api/saas/tenants/{id}/invoices      # Generate invoice
GET    /api/saas/tenants/{id}/usage         # Usage report
GET    /api/saas/plans                      # Available plans
```

## 🚨 Emergency Management Features

### Core Functionality

#### Incident Management
- **Real-time incident tracking**
- **Priority-based response coordination**
- **Resource allocation and dispatch**
- **Incident status updates**
- **Historical incident analysis**

#### Responder Management
- **GPS location tracking**
- **Mobile responder interface**
- **Shift scheduling**
- **Performance monitoring**
- **Equipment tracking**

#### Resource Management
- **Equipment inventory**
- **Vehicle tracking**
- **Personnel management**
- **Resource allocation**
- **Maintenance scheduling**

### Real-time Features

- **Live incident updates** via SignalR
- **GPS tracking** for responders
- **Emergency alerts** and notifications
- **Real-time analytics** dashboard
- **Live chat** for coordination

## 🎯 Event Management Features

### Complete Event Lifecycle

#### Event Planning & Management
- **Multiple event types** - In-person, virtual, and hybrid events
- **Session scheduling** - Detailed agenda management with time slots
- **Speaker coordination** - Speaker profiles, assignments, and management
- **Event branding** - Custom logos, colors, and CSS styling
- **Status tracking** - Draft, Published, RegistrationOpen, InProgress, Completed

#### Registration & Payment
- **Online registration forms** - Comprehensive attendee data capture
- **Group registrations** - Bulk registration with automated confirmations
- **Payment processing** - Optional payment integration with invoicing
- **Check-in functionality** - On-site and virtual check-in capabilities
- **Special requirements** - Dietary restrictions, accessibility needs

#### Communication & Engagement
- **Email notifications** - Invites, reminders, confirmations, updates
- **Calendar integration** - Outlook and Google Calendar support
- **SMS notifications** - Optional mobile messaging for updates
- **Real-time updates** - Live event status and session updates

#### Analytics & Reporting
- **Real-time dashboards** - Live attendee data and event metrics
- **Session analytics** - Attendance tracking and engagement metrics
- **Export capabilities** - Excel and PDF report generation
- **Performance insights** - Registration trends and event success metrics

### Event Management API Endpoints

```http
# Events
GET    /api/eventmanagement/events              # List all events
POST   /api/eventmanagement/events              # Create new event
GET    /api/eventmanagement/events/{id}         # Get specific event
PUT    /api/eventmanagement/events/{id}         # Update event
DELETE /api/eventmanagement/events/{id}         # Delete event

# Sessions
GET    /api/eventmanagement/sessions            # List all sessions
POST   /api/eventmanagement/sessions            # Create new session
GET    /api/eventmanagement/sessions/{id}       # Get specific session
PUT    /api/eventmanagement/sessions/{id}       # Update session
DELETE /api/eventmanagement/sessions/{id}       # Delete session

# Registrations
POST   /api/eventmanagement/registrations       # Create registration
GET    /api/eventmanagement/registrations/{id}  # Get registration
POST   /api/eventmanagement/registrations/{id}/checkin # Check-in attendee
POST   /api/eventmanagement/registrations/{id}/cancel # Cancel registration

# Speakers
GET    /api/eventmanagement/speakers            # List all speakers
POST   /api/eventmanagement/speakers            # Create new speaker
GET    /api/eventmanagement/speakers/{id}       # Get specific speaker
PUT    /api/eventmanagement/speakers/{id}       # Update speaker
DELETE /api/eventmanagement/speakers/{id}       # Delete speaker

# Analytics
GET    /api/eventmanagement/events/{id}/analytics # Event analytics
```

## 📱 User Interfaces

### EMS Dashboard (`index.html`)
- **Incident overview** with real-time updates
- **Resource status** monitoring
- **Analytics and reporting**
- **Quick action buttons**
- **Emergency alerts**

### Event Management (`event-management.html`)
- **Event creation** and management
- **Session scheduling** with drag-and-drop
- **Registration tracking** and check-in
- **Speaker management** and assignments
- **Analytics dashboard** with real-time metrics

### Mobile Responder Interface (`mobile-responder.html`)
- **GPS location sharing**
- **Incident status updates**
- **Photo capture** for documentation
- **Emergency contact** integration
- **Offline capability**

### SaaS Dashboard (`saas-dashboard.html`)
- **Tenant management**
- **Subscription tracking**
- **Billing and invoicing**
- **Usage analytics**
- **Plan management**

### Authentication Pages
- **Login** (`login.html`) - Secure authentication
- **Registration** (`register.html`) - User onboarding
- **Admin Panel** (`admin.html`) - System administration

## 🔧 API Reference

### Core Endpoints

#### Authentication
```http
POST /api/auth/login
{
  "username": "admin",
  "password": "admin123"
}
```

#### Incidents
```http
GET    /api/incidents              # List incidents
POST   /api/incidents              # Create incident
GET    /api/incidents/{id}         # Get incident
PUT    /api/incidents/{id}         # Update incident
DELETE /api/incidents/{id}         # Delete incident
```

#### Responders
```http
GET    /api/responders             # List responders
POST   /api/responders             # Create responder
GET    /api/responders/{id}        # Get responder
PUT    /api/responders/{id}        # Update responder
```

#### Equipment
```http
GET    /api/equipment              # List equipment
POST   /api/equipment              # Create equipment
GET    /api/equipment/{id}         # Get equipment
PUT    /api/equipment/{id}         # Update equipment
```

### Real-time Hub

```javascript
// Connect to SignalR hub
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/emsHub")
    .build();

// Listen for incident updates
connection.on("IncidentUpdated", (incident) => {
    console.log("New incident:", incident);
});

// Send location update
connection.invoke("UpdateLocation", {
    responderId: 1,
    latitude: 27.9506,
    longitude: -82.4572
});
```

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
docker build -t rexusops360-system .

# Run the container
docker run -p 5000:5000 rexusops360-system

# Or use Docker Compose
docker-compose up --build
```

### AWS Deployment

#### Prerequisites
- AWS account with appropriate permissions
- AWS CLI installed and configured
- Docker and Docker Compose installed

#### Quick Deployment
```bash
# Make deployment script executable
chmod +x deploy-aws.sh

# Run the deployment
./deploy-aws.sh
```

#### Manual AWS Deployment
```bash
# Create S3 bucket for artifacts
BUCKET_NAME="rexusops360-deployment-$(date +%s)"
aws s3 mb s3://$BUCKET_NAME --region us-east-1

# Upload application files
tar -czf rexusops360-app.tar.gz -C . .
aws s3 cp rexusops360-app.tar.gz s3://$BUCKET_NAME/
aws s3 cp docker-compose.yml s3://$BUCKET_NAME/
aws s3 cp Dockerfile s3://$BUCKET_NAME/

# Deploy CloudFormation stack
aws cloudformation deploy \
    --template-file aws-deploy.yml \
    --stack-name rexusops360-production \
    --parameter-overrides Environment=Production InstanceType=t3.medium \
    --capabilities CAPABILITY_IAM \
    --region us-east-1
```

### Azure Deployment
```bash
# Deploy to Azure App Service
az webapp up --name rexusops360-system --resource-group RexusOps360-RG
```

## 📊 Monitoring & Analytics

### System Metrics
- **Response times** - API performance monitoring
- **User activity** - Usage analytics
- **Incident statistics** - Emergency response metrics
- **Resource utilization** - System performance
- **SaaS metrics** - Revenue and growth tracking

### Health Checks
```http
GET /health                    # System health status
GET /api/health               # API health check
GET /api/analytics/overview   # Analytics overview
```

## 🔧 Configuration

### Environment Variables
```bash
# Database
ConnectionStrings__DefaultConnection="Server=localhost;Database=RexusOps360Db;..."

# JWT Authentication
Jwt__Key="YourSuperSecretKeyHere12345678901234567890"
Jwt__Issuer="RexusOps360"
Jwt__Audience="RexusOps360Users"

# SaaS Configuration
Security__Jwt__ExpirationHours=8
Security__Password__MinLength=8
Security__RateLimit__RequestsPerMinute=100

# Event Management
EventManagement__MaxEventsPerTenant=100
EventManagement__MaxRegistrationsPerEvent=1000
EventManagement__DefaultTimeZone="UTC"
```

### App Settings
```json
{
  "Security": {
    "Jwt": {
      "Key": "YourSuperSecretKeyHere12345678901234567890",
      "Issuer": "RexusOps360",
      "Audience": "RexusOps360Users",
      "ExpirationHours": 8
    },
    "Password": {
      "MinLength": 8,
      "RequireUppercase": true,
      "RequireLowercase": true,
      "RequireDigit": true,
      "RequireSpecialCharacter": true
    },
    "RateLimit": {
      "RequestsPerMinute": 100,
      "MaxFailedAttempts": 5,
      "LockoutDurationMinutes": 15
    }
  },
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

## 🧪 Testing

### Run Tests
```bash
cd RexusOps360.API.Tests
dotnet test
```

### Test Coverage
- ✅ **Authentication tests** - Login, registration, validation
- ✅ **SaaS tests** - Tenant management, subscriptions
- ✅ **Event Management tests** - Event creation, registration, analytics
- ✅ **API tests** - Endpoint functionality
- ✅ **Integration tests** - End-to-end workflows

### Test Credentials
```json
{
  "admin": {
    "username": "admin",
    "password": "admin123",
    "role": "Admin"
  },
  "dispatcher": {
    "username": "dispatcher1",
    "password": "pass123",
    "role": "Dispatcher"
  },
  "eventmanager": {
    "username": "eventmanager1",
    "password": "pass123",
    "role": "EventManager"
  },
  "responder": {
    "username": "responder1",
    "password": "pass123",
    "role": "Responder"
  }
}
```

## 💰 Cost Analysis

### AWS Production Costs (Monthly)
- **EC2 t3.medium**: ~$30
- **RDS db.t3.micro**: ~$15
- **Application Load Balancer**: ~$20
- **Data Transfer**: ~$5
- **CloudWatch**: ~$5
- **Total**: ~$75/month

### Cost Optimization
- Free tier eligible components
- Reserved instances for predictable usage
- Spot instances for non-critical workloads
- S3 lifecycle policies for cost control

## 🔄 CI/CD Pipeline

### GitHub Actions Workflow
1. **Build** - Compile and test application
2. **Security Scan** - Vulnerability assessment
3. **Deploy Staging** - Automated staging deployment
4. **Deploy Production** - Manual production deployment
5. **Notify** - Deployment status notifications

### Deployment Stages
- **Development** - Local development environment
- **Staging** - Pre-production testing
- **Production** - Live AWS infrastructure

## 🛠️ Development Roadmap

### ✅ **Phase 1: Core System (Complete)**
- [x] Basic EMS functionality
- [x] Incident management
- [x] Responder tracking
- [x] Equipment management
- [x] Authentication system

### ✅ **Phase 2: Advanced Features (Complete)**
- [x] Real-time analytics and reporting
- [x] Mobile-responsive design
- [x] Advanced security features
- [x] Event Management System
- [x] SaaS multi-tenancy
- [x] Comprehensive testing

### 🚀 **Phase 3: Enterprise Features (Next)**
- [ ] **OAuth2/SSO Integration** - Google, Microsoft, and custom SSO providers
- [ ] **Advanced Analytics** - Machine learning and AI-powered insights
- [ ] **Multi-tenant Architecture** - Multi-city support and tenant isolation
- [ ] **Mobile Applications** - Native iOS and Android apps
- [ ] **IoT Integration** - Smart sensors and wearable devices

### 📱 **Phase 4: Mobile Applications**
- [ ] **iOS Application** - Native Swift/SwiftUI app for responders
- [ ] **Android Application** - Native Kotlin/Compose app for responders
- [ ] **Offline Capabilities** - Local data storage and sync
- [ ] **Push Notifications** - Real-time mobile alerts
- [ ] **GPS Integration** - Native location services

### 🤖 **Phase 5: AI & IoT Integration**
- [ ] **Natural Language Processing** - Voice-to-text incident reporting
- [ ] **Computer Vision** - Image analysis for incident photos
- [ ] **Predictive Analytics** - Incident prediction and prevention
- [ ] **Smart Sensors** - Environmental monitoring sensors
- [ ] **Wearable Devices** - Responder health and location tracking

## 📞 Support

### Documentation
- [API Documentation](http://localhost:5000) - Swagger UI
- [Frontend Guide](frontend/) - User interface documentation
- [Event Management Guide](frontend/event-management.html) - Event management
- [SaaS Guide](frontend/saas-dashboard.html) - SaaS management

### Contact
- **Email**: support@rexusops360.com
- **Phone**: (813) 555-0123
- **Emergency**: 911

### Emergency Contacts
- **Tampa Fire Department**: (813) 274-7000
- **Hillsborough County EMS**: (813) 272-5900
- **Emergency**: 911

## 🏆 Features Summary

### ✅ Implemented Features

#### Authentication & Security
- [x] JWT token authentication
- [x] Role-based access control
- [x] Password security with hashing
- [x] Rate limiting and lockout
- [x] Input validation
- [x] CORS configuration
- [x] Security headers

#### SaaS Multi-Tenancy
- [x] Multi-tenant architecture
- [x] Subscription management
- [x] Billing and invoicing
- [x] Usage tracking
- [x] Plan enforcement
- [x] Analytics dashboard

#### Emergency Management
- [x] Incident management
- [x] Responder tracking
- [x] Resource management
- [x] Real-time updates
- [x] GPS tracking
- [x] Mobile interface

#### Event Management
- [x] Multi-tenant event creation and management
- [x] Session scheduling and agenda management
- [x] Speaker coordination and assignment
- [x] Event branding and customization
- [x] Registration system with payment processing
- [x] Check-in functionality
- [x] Analytics and reporting

#### User Interfaces
- [x] Main EMS dashboard
- [x] Event management interface
- [x] Mobile responder interface
- [x] SaaS management dashboard
- [x] Authentication pages
- [x] Admin panel

#### API & Backend
- [x] RESTful API endpoints
- [x] SignalR real-time communication
- [x] Entity Framework Core
- [x] Comprehensive testing
- [x] Error handling
- [x] Logging

### 🚀 Ready for Production

The RexusOps360 system is **production-ready** with:

- ✅ **Enterprise-grade security**
- ✅ **Multi-tenant SaaS capabilities**
- ✅ **Real-time emergency response**
- ✅ **Complete event management**
- ✅ **Comprehensive testing**
- ✅ **Scalable architecture**
- ✅ **Modern UI/UX**
- ✅ **Complete documentation**
- ✅ **AWS deployment ready**

---

**Built with ❤️ for Tampa, FL Emergency Services & Transportation Agencies**

*RexusOps360 - Empowering Emergency Response & Event Management* 