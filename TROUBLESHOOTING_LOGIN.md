# Login Troubleshooting Guide

## 🔍 Current Issue: "Invalid Credentials" Error

### Possible Causes:

1. **C# Backend Not Running**
   - The C# API server needs to be running on port 5169
   - Check if the server is started

2. **Frontend Configuration**
   - DEMO_MODE is set to `true` for testing
   - API_BASE points to `http://localhost:5169/api`

3. **Network Connection**
   - Browser might be blocking the connection
   - CORS issues between frontend and backend

## 🛠️ Troubleshooting Steps:

### Step 1: Check if C# Backend is Running
```powershell
# Check if port 5169 is in use
netstat -ano | findstr ":5169"

# Start the C# backend if not running
cd RexusOps360.API
dotnet run --urls "http://localhost:5169"
```

### Step 2: Test API Endpoints
```powershell
# Test health endpoint
curl http://localhost:5169/health

# Test login endpoint
curl -X POST http://localhost:5169/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"pass123"}'
```

### Step 3: Check Browser Console
1. Open browser developer tools (F12)
2. Go to Console tab
3. Try to login and check for errors
4. Look for network errors or CORS issues

### Step 4: Verify Demo Mode
- Currently set to `DEMO_MODE = true` for testing
- This bypasses the C# API and uses local demo authentication
- Demo credentials should work: admin/pass123

## 🎯 Quick Fixes:

### Option 1: Use Demo Mode (Recommended for Testing)
- DEMO_MODE is already set to `true`
- Try logging in with: admin / pass123
- This should work immediately

### Option 2: Start C# Backend
```powershell
# Navigate to API directory
cd RexusOps360.API

# Start the server
dotnet run --urls "http://localhost:5169"

# Wait for "Now listening on: http://localhost:5169"
```

### Option 3: Test API Directly
```powershell
# Test the login API
Invoke-RestMethod -Uri "http://localhost:5169/api/auth/login" `
  -Method POST `
  -ContentType "application/json" `
  -Body '{"username":"admin","password":"pass123"}'
```

## 🔧 Demo Credentials:
- **Admin**: admin / pass123
- **Dispatcher**: dispatcher1 / pass123  
- **Responder**: responder1 / pass123

## 📋 Expected Behavior:

### With Demo Mode (DEMO_MODE = true):
- ✅ Should work immediately
- ✅ No backend required
- ✅ Shows "Demo login successful!"

### With Real API (DEMO_MODE = false):
- ✅ Requires C# backend running
- ✅ Uses JWT authentication
- ✅ More secure but requires server

## 🚨 Common Issues:

1. **"Network error"** - Backend not running
2. **"Invalid credentials"** - Wrong username/password
3. **CORS errors** - Browser blocking cross-origin requests
4. **Port conflicts** - Another service using port 5169

## ✅ Current Status:
- Demo mode is ENABLED for testing
- Should work with demo credentials
- C# backend can be started for full functionality 