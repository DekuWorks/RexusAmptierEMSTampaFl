# Public Incident Reporting System

## Overview

The RexusOps360 EMS now includes a **Public Incident Reporting System** that allows anyone to report emergency incidents without needing to log in or create an account. This feature makes the EMS system more accessible to the public and ensures that emergency situations can be reported quickly and easily.

## 🎯 Key Features

### ✅ **Anonymous Reporting**
- No login required
- No account creation needed
- Anyone can report incidents immediately

### ✅ **Comprehensive Form**
- Reporter information (name, phone, email)
- Detailed incident location
- Incident type and priority selection
- Description and additional details
- People involved, injuries, vehicles, hazards

### ✅ **Emergency Contact Information**
- Prominent display of 911 emergency number
- EMS dispatch contact information
- Clear guidance for emergency vs non-emergency situations

### ✅ **Smart Validation**
- Required field validation
- Phone number format validation
- Email format validation (optional)
- Priority level validation

### ✅ **Demo Mode Support**
- Works without backend for testing
- Simulates API responses
- Form validation and submission simulation

## 📁 Files Created/Modified

### New Files:
- `frontend/public-incident-report.html` - Main public reporting page
- `RexusOps360.API/Controllers/PublicController.cs` - C# API controller
- `test-public-incident-report.ps1` - Testing script
- `PUBLIC_INCIDENT_REPORTING.md` - This documentation

### Modified Files:
- `frontend/landing.html` - Added public reporting section

## 🚀 How to Use

### For Public Users:
1. **Access the Form**: Navigate to `frontend/public-incident-report.html`
2. **Fill Out Information**: Complete all required fields
3. **Submit Report**: Click "Submit Incident Report"
4. **Receive Confirmation**: Get incident ID and reference number

### For EMS Staff:
1. **View Reports**: Public reports appear in the main incident list
2. **Process Reports**: Handle like any other incident
3. **Contact Reporter**: Use provided contact information if needed

## 🔧 Technical Implementation

### Frontend (HTML/JavaScript)
```javascript
// API Configuration
const API_BASE = 'http://localhost:5169/api';
const DEMO_MODE = true; // Set to false for production

// Form submission with validation
document.getElementById('incidentForm').addEventListener('submit', async function(e) {
    // Collect and validate form data
    // Submit to API or simulate in demo mode
    // Show success/error messages
});
```

### Backend (C# API)
```csharp
[HttpPost("incidents")]
public async Task<IActionResult> SubmitIncidentReport([FromBody] PublicIncidentRequest request)
{
    // Validate request data
    // Check rate limiting
    // Create incident record
    // Send notifications
    // Return response with incident ID
}
```

## 📋 Form Fields

### Reporter Information
- **Name** (Required): Full name of person reporting
- **Phone** (Required): Contact phone number
- **Email** (Optional): Contact email address
- **Relation** (Optional): Relationship to incident

### Incident Location
- **Address** (Required): Street address
- **City** (Required): City name
- **State** (Required): State abbreviation
- **ZIP Code** (Required): Postal code
- **Landmark** (Optional): Nearby landmark
- **Cross Street** (Optional): Intersection

### Incident Details
- **Type** (Required): Medical, Fire, Traffic, Crime, etc.
- **Priority** (Required): Low, Medium, High, Emergency
- **Description** (Required): Detailed description
- **Time** (Required): When incident occurred
- **Status** (Optional): Ongoing, Resolved, etc.

### Additional Information
- **People Involved** (Optional): Number of people
- **Injuries** (Optional): None, Minor, Serious, Critical
- **Vehicles Involved** (Optional): Number of vehicles
- **Hazards** (Optional): Fire, Chemical, Electrical, etc.
- **Additional Info** (Optional): Extra details

## 🔒 Security Features

### Rate Limiting
- Maximum 5 submissions per hour per IP address
- Prevents spam and abuse
- Configurable limits

### Input Validation
- Server-side validation of all fields
- Phone number format validation
- Email format validation
- Priority level validation

### IP Tracking
- Logs IP address for security
- Helps identify potential abuse
- Assists with incident investigation

### Audit Logging
- All public submissions are logged
- Tracks who submitted what and when
- Maintains audit trail for compliance

## 🧪 Testing

### Demo Mode Testing
1. Open `frontend/public-incident-report.html`
2. Fill out the form with test data
3. Submit the form
4. Verify success message appears
5. Check that form resets

### API Testing
1. Start the C# backend: `dotnet run --project RexusOps360.API`
2. Set `DEMO_MODE = false` in the HTML file
3. Run the test script: `.\test-public-incident-report.ps1`
4. Verify API responses

### Manual Testing
```powershell
# Test the public API endpoint
$response = Invoke-RestMethod -Uri "http://localhost:5169/api/public/emergency-info" -Method GET
Write-Host $response.data.emergencyNumber
```

## 📊 API Endpoints

### Public Endpoints (No Authentication Required)

#### `POST /api/public/incidents`
Submit a public incident report
```json
{
  "reporterName": "John Doe",
  "reporterPhone": "5551234567",
  "incidentAddress": "123 Main Street",
  "incidentCity": "Tampa",
  "incidentState": "FL",
  "incidentZip": "33601",
  "incidentType": "medical",
  "incidentPriority": "high",
  "incidentDescription": "Person collapsed on sidewalk"
}
```

#### `GET /api/public/emergency-info`
Get emergency contact information
```json
{
  "emergencyNumber": "911",
  "emsDispatch": "(813) 555-EMER",
  "policeDispatch": "(813) 555-POLICE",
  "fireDispatch": "(813) 555-FIRE"
}
```

#### `GET /api/public/alerts`
Get current emergency alerts
```json
{
  "alerts": [
    {
      "title": "Severe Weather Warning",
      "message": "Severe thunderstorms expected",
      "type": "weather",
      "priority": "medium"
    }
  ]
}
```

## 🎨 User Interface

### Design Features
- **Responsive Design**: Works on desktop, tablet, and mobile
- **Emergency Styling**: Red color scheme for urgency
- **Clear Navigation**: Easy to find and use
- **Form Validation**: Real-time feedback
- **Success Messages**: Clear confirmation of submission

### Accessibility
- **Screen Reader Support**: Proper ARIA labels
- **Keyboard Navigation**: Full keyboard accessibility
- **High Contrast**: Readable text and colors
- **Mobile Friendly**: Touch-optimized interface

## 🔄 Integration Points

### With Main EMS System
- Public reports appear in incident dashboard
- Same processing workflow as authenticated reports
- Integration with notification system
- Audit trail maintained

### With External Systems
- Can integrate with 911 systems
- Supports emergency alert systems
- Compatible with other emergency management platforms

## 🚨 Emergency Guidelines

### When to Use Public Reporting
- **Non-emergency incidents** that need attention
- **Additional information** about ongoing emergencies
- **Follow-up reports** for existing incidents
- **Community concerns** that don't require immediate response

### When to Call 911
- **Immediate life-threatening situations**
- **Active crimes in progress**
- **Medical emergencies requiring immediate attention**
- **Fires or hazardous material incidents**

## 📈 Future Enhancements

### Planned Features
- **Photo/Video Upload**: Allow media attachments
- **GPS Location**: Automatic location detection
- **Multi-language Support**: Spanish and other languages
- **SMS Notifications**: Text message confirmations
- **Integration with 911**: Direct connection to emergency services

### Technical Improvements
- **Real-time Validation**: Instant field validation
- **Auto-complete**: Address and location suggestions
- **Offline Support**: Work without internet connection
- **Progressive Web App**: Installable web application

## 🛠️ Troubleshooting

### Common Issues

#### Form Won't Submit
- Check that all required fields are filled
- Verify phone number format
- Ensure email format is valid (if provided)

#### Demo Mode Not Working
- Check browser console for JavaScript errors
- Verify `DEMO_MODE = true` in the HTML file
- Clear browser cache and reload

#### API Not Responding
- Ensure C# backend is running
- Check API base URL configuration
- Verify network connectivity

### Debug Steps
1. Open browser developer tools
2. Check console for error messages
3. Verify network requests in Network tab
4. Test API endpoints directly with Postman

## 📞 Support

For technical support or questions about the public incident reporting system:

- **Development Team**: RexusOps360 Development Team
- **Documentation**: This file and inline code comments
- **Testing**: Use the provided test scripts
- **Issues**: Check browser console and server logs

---

**Version**: 1.0.0  
**Last Updated**: 2025-01-17  
**Author**: RexusOps360 Development Team 