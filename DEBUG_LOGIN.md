# 🔧 EMS Login Debug Guide

## ✅ Step 1: Start the Backend

Run the backend startup script:
```powershell
.\start-backend.ps1
```

Or manually:
```bash
cd RexusOps360.API
dotnet dev-certs https --trust
dotnet run
```

You should see:
```
Now listening on: http://localhost:5169
Now listening on: https://localhost:7049
```

## ✅ Step 2: Verify Backend is Running

1. **Check Swagger UI**: Open http://localhost:5169 in your browser
2. **Test Health Endpoint**: Visit http://localhost:5169/health
3. **Check API Endpoints**: Visit http://localhost:5169/api/auth/login (should show 405 Method Not Allowed, which is correct for GET)

## ✅ Step 3: Test Login API

Use curl or Postman to test the login endpoint:

```bash
curl -X POST http://localhost:5169/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}'
```

Expected response:
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "token": "eyJ...",
    "user": {
      "id": 1,
      "username": "admin",
      "full_name": "System Administrator",
      "role": "admin"
    }
  }
}
```

## ✅ Step 4: Check Frontend Configuration

The frontend is now configured to use:
- **API Base URL**: `http://localhost:5169/api`
- **Login Endpoint**: `POST /api/auth/login`
- **Error Handling**: Proper network error detection

## ✅ Step 5: Debug Network Issues

### Open Browser DevTools:
1. Press F12 or right-click → Inspect
2. Go to **Network** tab
3. Try to login
4. Look for the request to `/api/auth/login`

### Common Issues:

#### ❌ 404 Error
- Backend not running
- Wrong URL in frontend
- Check if backend is on port 5169

#### ❌ CORS Error
- Backend CORS is configured correctly
- Check browser console for CORS errors
- Try different browser or incognito mode

#### ❌ 500 Server Error
- Check backend console for error messages
- Database connection issues
- Check if database is created

#### ❌ No Response
- Backend not running
- Firewall blocking connection
- Wrong port number

## ✅ Step 6: Test with Demo Credentials

The frontend has demo credentials pre-filled:
- **Admin**: admin / admin123
- **Dispatcher**: dispatcher1 / pass123  
- **Responder**: responder1 / pass123

## ✅ Step 7: Verify Database

The backend uses in-memory database for development. Check if users exist:

```bash
# Check backend logs for database initialization
# Look for "Database initialization error" messages
```

## 🔧 Troubleshooting Commands

### Check if backend is running:
```bash
netstat -an | findstr :5169
```

### Kill existing processes:
```bash
taskkill /F /IM dotnet.exe
```

### Clear browser cache:
- Press Ctrl+Shift+Delete
- Clear all data
- Restart browser

### Test API directly:
```bash
# Test health endpoint
curl http://localhost:5169/health

# Test login endpoint
curl -X POST http://localhost:5169/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}'
```

## 🚨 Emergency Fixes

### If backend won't start:
1. Delete `bin` and `obj` folders in RexusOps360.API
2. Run `dotnet restore`
3. Run `dotnet build`
4. Run `dotnet run`

### If login still fails:
1. Check browser console for errors
2. Verify backend is running on correct port
3. Try different browser
4. Check firewall settings

### If CORS issues persist:
1. Restart backend
2. Clear browser cache
3. Try incognito mode
4. Check if antivirus is blocking

## 📞 Support

If issues persist:
1. Check backend console for error messages
2. Check browser console for JavaScript errors
3. Verify all files are saved
4. Restart both frontend and backend 