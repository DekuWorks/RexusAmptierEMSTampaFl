# RexusOps360 EMS API

A comprehensive Emergency Management System API built with ASP.NET Core Web API for managing incidents, responders, and equipment in Tampa, FL.

## 🚀 Features

### Core Functionality
- **Incident Management**: Create, read, update incidents with priority levels
- **Responder Management**: Track emergency responders and their availability
- **Equipment Management**: Monitor equipment inventory and status
- **Dashboard Statistics**: Real-time overview of system metrics
- **Health Monitoring**: System health checks and status reporting

### Security & Validation
- **JWT Authentication**: Secure API endpoints with token-based authentication
- **Role-based Authorization**: Protect sensitive operations
- **Input Validation**: Comprehensive data validation with custom error messages
- **CORS Support**: Cross-origin resource sharing enabled

### API Documentation
- **Swagger UI**: Interactive API documentation at root endpoint
- **OpenAPI Specification**: Standard API documentation format
- **Example Requests**: Ready-to-use API examples

## 🛠️ Technology Stack

- **Framework**: ASP.NET Core 9.0
- **Authentication**: JWT Bearer Tokens
- **Validation**: Data Annotations
- **Documentation**: Swagger/OpenAPI
- **Data Storage**: In-Memory (for demo purposes)

## 📋 Prerequisites

- .NET 9.0 SDK or later
- Visual Studio 2022 or VS Code
- PowerShell (for Windows)

## 🚀 Quick Start

### 1. Clone and Navigate
```bash
cd RexusOps360.API
```

### 2. Restore Dependencies
```bash
dotnet restore
```

### 3. Build the Project
```bash
dotnet build
```

### 4. Run the API
```bash
dotnet run
```

The API will be available at:
- **API Base URL**: `http://localhost:5000`
- **Swagger UI**: `http://localhost:5000` (root endpoint)
- **Health Check**: `http://localhost:5000/api/health`

## 🔐 Authentication

### Login
```bash
POST /api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "password123"
}
```

### Using JWT Token
Include the token in the Authorization header:
```
Authorization: Bearer <your-jwt-token>
```

## 📊 API Endpoints

### Public Endpoints
- `GET /api/health` - System health check
- `POST /api/auth/login` - User authentication

### Protected Endpoints (Require JWT Token)

#### Dashboard
- `GET /api/dashboard/stats` - Get system statistics

#### Incidents
- `GET /api/incidents` - Get all incidents
- `GET /api/incidents/{id}` - Get specific incident
- `POST /api/incidents` - Create new incident
- `PUT /api/incidents/{id}` - Update incident

#### Responders
- `GET /api/responders` - Get all responders
- `POST /api/responders` - Add new responder

#### Equipment
- `GET /api/equipment` - Get all equipment
- `POST /api/equipment` - Add new equipment

## 📝 Data Models

### Incident
```json
{
  "id": 1,
  "type": "Medical Emergency",
  "location": "123 Main St, Tampa, FL",
  "description": "Patient experiencing chest pain",
  "priority": "High",
  "status": "Active",
  "createdAt": "2025-06-30T10:00:00Z",
  "updatedAt": "2025-06-30T10:00:00Z",
  "assignedResponders": [1, 2],
  "equipmentNeeded": ["Ambulance", "Defibrillator"]
}
```

### Responder
```json
{
  "id": 1,
  "name": "John Smith",
  "role": "Paramedic",
  "contactNumber": "(813) 555-0101",
  "status": "Available",
  "currentLocation": "Downtown Station",
  "specializations": ["Cardiac", "Trauma"],
  "createdAt": "2025-06-30T10:00:00Z"
}
```

### Equipment
```json
{
  "id": 1,
  "name": "Ambulance",
  "type": "Transport",
  "quantity": 5,
  "availableQuantity": 4,
  "status": "Available",
  "location": "Main Garage",
  "lastMaintenance": "2025-06-15T10:00:00Z",
  "createdAt": "2025-06-30T10:00:00Z"
}
```

## 🔧 Configuration

### JWT Settings
The API uses default JWT settings. For production, update `appsettings.json`:

```json
{
  "Jwt": {
    "Key": "YourSuperSecretKey123!@#",
    "Issuer": "RexusOps360",
    "Audience": "RexusOps360Users"
  }
}
```

### CORS Policy
The API allows all origins for development. Configure in `Program.cs` for production.

## 🧪 Testing

### Using Swagger UI
1. Navigate to `http://localhost:5000`
2. Click "Authorize" and enter your JWT token
3. Test endpoints interactively

### Using PowerShell
```powershell
# Health check
Invoke-RestMethod -Uri "http://localhost:5000/api/health" -Method Get

# Login
$login = @{ username = "admin"; password = "password123" } | ConvertTo-Json
$response = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method Post -Body $login -ContentType "application/json"
$token = $response.token

# Get incidents (with auth)
$headers = @{ Authorization = "Bearer $token" }
Invoke-RestMethod -Uri "http://localhost:5000/api/incidents" -Method Get -Headers $headers
```

### Using curl
```bash
# Health check
curl -X GET "http://localhost:5000/api/health"

# Login
curl -X POST "http://localhost:5000/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"password123"}'

# Get incidents (with auth)
curl -X GET "http://localhost:5000/api/incidents" \
  -H "Authorization: Bearer <your-token>"
```

## 📁 Project Structure

```
RexusOps360.API/
├── Controllers/
│   ├── AuthController.cs
│   ├── DashboardController.cs
│   ├── IncidentsController.cs
│   ├── RespondersController.cs
│   └── EquipmentController.cs
├── Models/
│   ├── Incident.cs
│   ├── Responder.cs
│   └── Equipment.cs
├── Data/
│   └── InMemoryStore.cs
├── Program.cs
└── README.md
```

## 🚀 Deployment

### Development
```bash
dotnet run
```

### Production
```bash
dotnet publish -c Release
dotnet RexusOps360.API.dll
```

## 🔒 Security Considerations

- Change default JWT secret key in production
- Implement proper user management system
- Add rate limiting for production use
- Use HTTPS in production
- Implement proper logging and monitoring

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License.

## 🆘 Support

For support and questions:
- Create an issue in the repository
- Contact the development team
- Check the Swagger documentation

---

**RexusOps360 EMS API** - Emergency Management System for Tampa, FL 