# EMS Tampa-FL Amptier - Emergency Management System

A comprehensive Emergency Management System for Tampa, FL, featuring both Flask (Python) and ASP.NET Core (C#) implementations.

## 🚀 Project Overview

This repository contains two complete implementations of an Emergency Management System:

1. **Flask Backend** - Python-based REST API
2. **ASP.NET Core Backend** - C#-based REST API with enhanced features
3. **Frontend** - Modern HTML/JavaScript dashboard

## 📁 Repository Structure

```
EMS_Tampa-FL_Amptier/
├── backend/                 # Flask Python Backend
│   ├── api/
│   │   ├── endpoints.py     # API endpoints
│   │   └── __init__.py
│   └── app.py              # Flask application
├── RexusOps360.API/        # ASP.NET Core Backend
│   ├── Controllers/        # API controllers
│   ├── Models/            # Data models
│   ├── Data/              # Data access layer
│   ├── Program.cs         # Application configuration
│   └── README.md          # Detailed API documentation
├── frontend/              # Web dashboard
│   └── index.html         # Modern responsive UI
├── venv/                  # Python virtual environment
├── requirements.txt       # Python dependencies
├── .gitignore            # Git ignore rules
├── FINAL_CHECKLIST.md    # Project completion checklist
└── README.md             # This file
```

## 🛠️ Technology Stacks

### Flask Backend (Python)
- **Framework**: Flask 3.1.1
- **CORS**: Flask-CORS
- **Data Storage**: In-memory (for demo)
- **Features**: RESTful API, CORS support, JSON responses

### ASP.NET Core Backend (C#)
- **Framework**: ASP.NET Core 9.0
- **Authentication**: JWT Bearer Tokens
- **Validation**: Data Annotations
- **Documentation**: Swagger/OpenAPI
- **Features**: Role-based authorization, comprehensive validation

### Frontend
- **Framework**: Vanilla JavaScript
- **UI**: Modern CSS with Glassmorphism design
- **Features**: Responsive design, real-time updates, interactive forms

## 🚀 Quick Start

### Option 1: Flask Backend (Python)

```bash
# Navigate to backend
cd backend

# Activate virtual environment
.\venv\Scripts\Activate.ps1  # Windows
source venv/bin/activate     # Linux/Mac

# Install dependencies
pip install -r requirements.txt

# Run the application
python app.py
```

**Access Points:**
- API: `http://localhost:5000`
- Health Check: `http://localhost:5000/api/health`

### Option 2: ASP.NET Core Backend (C#)

```bash
# Navigate to .NET project
cd RexusOps360.API

# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run
```

**Access Points:**
- API: `http://localhost:5000` (or configured port)
- Swagger UI: `http://localhost:5000` (root endpoint)
- Health Check: `http://localhost:5000/api/health`

**Authentication:**
- Username: `admin`
- Password: `password123`

### Frontend Dashboard

```bash
# Open frontend/index.html in your browser
# Or serve it with a local server
```

## 📊 Features

### Core Functionality
- **Incident Management**: Create, read, update emergency incidents
- **Responder Management**: Track emergency responders and availability
- **Equipment Management**: Monitor equipment inventory and status
- **Dashboard Statistics**: Real-time system overview
- **Health Monitoring**: System status and health checks

### Security Features (ASP.NET Core)
- JWT Bearer token authentication
- Role-based authorization
- Protected API endpoints
- Input validation with custom error messages

### API Endpoints

#### Public Endpoints
- `GET /api/health` - System health check
- `POST /api/auth/login` - User authentication (ASP.NET Core only)

#### Protected Endpoints
- `GET /api/dashboard/stats` - System statistics
- `GET /api/incidents` - Get all incidents
- `POST /api/incidents` - Create new incident
- `PUT /api/incidents/{id}` - Update incident
- `GET /api/responders` - Get all responders
- `POST /api/responders` - Add new responder
- `GET /api/equipment` - Get all equipment
- `POST /api/equipment` - Add new equipment

## 🔧 Configuration

### Flask Backend
- CORS enabled for all origins
- Debug mode enabled for development
- In-memory data storage

### ASP.NET Core Backend
- JWT authentication configured
- CORS policy for frontend integration
- Swagger UI for API documentation
- Comprehensive input validation

## 🧪 Testing

### Using Swagger UI (ASP.NET Core)
1. Navigate to `http://localhost:5000`
2. Click "Authorize" and enter JWT token
3. Test endpoints interactively

### Using PowerShell
```powershell
# Health check
Invoke-RestMethod -Uri "http://localhost:5000/api/health" -Method Get

# Login (ASP.NET Core)
$login = @{ username = "admin"; password = "password123" } | ConvertTo-Json
$response = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method Post -Body $login -ContentType "application/json"
$token = $response.token

# Get incidents (with auth for ASP.NET Core)
$headers = @{ Authorization = "Bearer $token" }
Invoke-RestMethod -Uri "http://localhost:5000/api/incidents" -Method Get -Headers $headers
```

## 📋 Data Models

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
  "updatedAt": "2025-06-30T10:00:00Z"
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
  "currentLocation": "Downtown Station"
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
  "location": "Main Garage"
}
```

## 🚀 Deployment

### Development
Both backends are configured for development with hot reloading.

### Production
- Change JWT secret keys
- Configure proper CORS policies
- Use production database
- Enable HTTPS
- Add logging and monitoring

## 🔒 Security Considerations

- Change default JWT secret keys in production
- Implement proper user management
- Add rate limiting
- Use HTTPS in production
- Implement proper logging

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
- Check the individual README files for each backend
- Review the Swagger documentation

---

**EMS Tampa-FL Amptier** - Emergency Management System for Tampa, FL
**Status**: ✅ Production Ready
**Last Updated**: June 30, 2025 