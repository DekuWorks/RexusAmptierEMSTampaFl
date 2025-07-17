using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace RexusOps360.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WebController : ControllerBase
    {
        [HttpGet("dashboard")]
        public IActionResult Dashboard()
        {
            var html = @"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>EMS Tampa-FL Amptier - Dashboard</title>
    <link href=""https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css"" rel=""stylesheet"">
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }

        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            color: #333;
        }

        .header {
            background: rgba(255, 255, 255, 0.95);
            backdrop-filter: blur(10px);
            border-radius: 0 0 20px 20px;
            padding: 20px;
            margin-bottom: 30px;
            box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
        }

        .header-content {
            max-width: 1400px;
            margin: 0 auto;
            display: flex;
            justify-content: space-between;
            align-items: center;
        }

        .header-left h1 {
            color: #2c3e50;
            font-size: 2.5rem;
            margin-bottom: 5px;
        }

        .header-left p {
            color: #7f8c8d;
            font-size: 1.1rem;
        }

        .logo-section {
            margin-bottom: 15px;
        }

        .logo-bar {
            display: flex;
            justify-content: flex-start;
            align-items: center;
            gap: 20px;
        }

        .logo-img {
            height: 40px;
            width: auto;
            border-radius: 8px;
            box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1);
        }

        .user-info {
            display: flex;
            align-items: center;
            gap: 15px;
        }

        .user-avatar {
            width: 50px;
            height: 50px;
            background: linear-gradient(135deg, #3498db, #2980b9);
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            color: white;
            font-size: 1.5rem;
        }

        .user-details {
            text-align: right;
        }

        .user-name {
            font-weight: 600;
            color: #2c3e50;
            font-size: 1.1rem;
        }

        .user-role {
            color: #7f8c8d;
            font-size: 0.9rem;
            text-transform: capitalize;
        }

        .logout-btn {
            background: #e74c3c;
            color: white;
            border: none;
            padding: 8px 15px;
            border-radius: 8px;
            cursor: pointer;
            font-size: 0.9rem;
            transition: all 0.3s ease;
            text-decoration: none;
        }

        .logout-btn:hover {
            background: #c0392b;
        }

        .container {
            max-width: 1400px;
            margin: 0 auto;
            padding: 0 20px;
        }

        .stats-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
            gap: 20px;
            margin-bottom: 30px;
        }

        .stat-card {
            background: rgba(255, 255, 255, 0.95);
            backdrop-filter: blur(10px);
            border-radius: 15px;
            padding: 25px;
            text-align: center;
            box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
            transition: transform 0.3s ease;
        }

        .stat-card:hover {
            transform: translateY(-5px);
        }

        .stat-card i {
            font-size: 2.5rem;
            margin-bottom: 15px;
            color: #3498db;
        }

        .stat-card h3 {
            font-size: 2rem;
            color: #2c3e50;
            margin-bottom: 5px;
        }

        .stat-card p {
            color: #7f8c8d;
            font-size: 1rem;
        }

        .weather-widget {
            background: linear-gradient(135deg, #74b9ff 0%, #0984e3 100%);
            color: white;
            padding: 20px;
            border-radius: 15px;
            text-align: center;
            margin-bottom: 30px;
        }

        .weather-widget h3 {
            font-size: 1.2rem;
            margin-bottom: 10px;
        }

        .weather-widget .temp {
            font-size: 2.5rem;
            font-weight: bold;
            margin-bottom: 5px;
        }

        .weather-widget .desc {
            font-size: 1rem;
            opacity: 0.9;
        }

        .content-grid {
            display: grid;
            grid-template-columns: 2fr 1fr;
            gap: 30px;
            margin-bottom: 30px;
        }

        .main-content {
            background: rgba(255, 255, 255, 0.95);
            backdrop-filter: blur(10px);
            border-radius: 15px;
            padding: 30px;
            box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
        }

        .sidebar {
            background: rgba(255, 255, 255, 0.95);
            backdrop-filter: blur(10px);
            border-radius: 15px;
            padding: 30px;
            box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
        }

        .section-title {
            color: #2c3e50;
            margin-bottom: 20px;
            font-size: 1.5rem;
            font-weight: 600;
        }

        .incident-item {
            background: #f8f9fa;
            border-radius: 10px;
            padding: 20px;
            margin-bottom: 15px;
            border-left: 4px solid #3498db;
            transition: all 0.3s ease;
        }

        .incident-item:hover {
            transform: translateX(5px);
            box-shadow: 0 5px 15px rgba(0, 0, 0, 0.1);
        }

        .incident-item h4 {
            color: #2c3e50;
            margin-bottom: 10px;
        }

        .incident-item p {
            color: #7f8c8d;
            margin-bottom: 5px;
        }

        .status-badge {
            display: inline-block;
            padding: 4px 12px;
            border-radius: 20px;
            font-size: 0.8rem;
            font-weight: 600;
            text-transform: uppercase;
        }

        .status-active {
            background: #e74c3c;
            color: white;
        }

        .priority-high {
            background: #e74c3c;
            color: white;
        }

        .priority-medium {
            background: #f39c12;
            color: white;
        }

        .priority-low {
            background: #27ae60;
            color: white;
        }

        .notification-item {
            background: #fff3cd;
            border: 1px solid #ffeaa7;
            border-radius: 8px;
            padding: 15px;
            margin-bottom: 10px;
        }

        .notification-item h4 {
            color: #856404;
            margin-bottom: 5px;
            font-size: 1rem;
        }

        .notification-item p {
            color: #856404;
            font-size: 0.9rem;
        }

        .quick-actions {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 15px;
            margin-bottom: 20px;
        }

        .action-btn {
            background: linear-gradient(135deg, #3498db, #2980b9);
            color: white;
            border: none;
            padding: 15px;
            border-radius: 10px;
            cursor: pointer;
            font-size: 0.9rem;
            font-weight: 600;
            transition: all 0.3s ease;
            text-decoration: none;
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 8px;
        }

        .action-btn:hover {
            transform: translateY(-2px);
            box-shadow: 0 5px 15px rgba(52, 152, 219, 0.3);
        }

        .action-btn.secondary {
            background: linear-gradient(135deg, #27ae60, #229954);
        }

        .role-badge {
            display: inline-block;
            padding: 4px 8px;
            border-radius: 12px;
            font-size: 0.7rem;
            font-weight: 600;
            text-transform: uppercase;
        }

        .role-admin {
            background: #e74c3c;
            color: white;
        }

        .role-dispatcher {
            background: #f39c12;
            color: white;
        }

        .role-responder {
            background: #3498db;
            color: white;
        }

        .role-public {
            background: #95a5a6;
            color: white;
        }

        @media (max-width: 768px) {
            .header-content {
                flex-direction: column;
                gap: 15px;
            }
            
            .user-info {
                flex-direction: column;
                text-align: center;
            }
            
            .content-grid {
                grid-template-columns: 1fr;
            }
            
            .quick-actions {
                grid-template-columns: 1fr;
            }
        }
    </style>
</head>
<body>
    <div class=""header"">
        <div class=""header-content"">
            <div class=""header-left"">
                <div class=""logo-section"">
                    <div class=""logo-bar"">
                        <img src=""/images/rexus.png"" alt=""Rexus Logo"" class=""logo-img"">
                        <img src=""/images/amptier.png"" alt=""Amptier Logo"" class=""logo-img"">
                    </div>
                </div>
                <h1><i class=""fas fa-ambulance""></i> EMS Tampa-FL Amptier</h1>
                <p>Emergency Management System Dashboard</p>
            </div>
            <div class=""user-info"">
                <div class=""user-avatar"">
                    <i class=""fas fa-user""></i>
                </div>
                <div class=""user-details"">
                    <div class=""user-name"">System Administrator</div>
                    <div class=""user-role"">
                        <span class=""role-badge role-admin"">admin</span>
                    </div>
                </div>
                <a href=""/web/login"" class=""logout-btn"">
                    <i class=""fas fa-sign-out-alt""></i> Logout
                </a>
            </div>
        </div>
    </div>

    <div class=""container"">
        <div class=""weather-widget"">
            <h3><i class=""fas fa-cloud-sun""></i> Tampa Weather</h3>
            <div class=""temp"">24°C</div>
            <div class=""desc"">Partly Cloudy</div>
        </div>

        <div class=""stats-grid"">
            <div class=""stat-card"">
                <i class=""fas fa-exclamation-triangle""></i>
                <h3>12</h3>
                <p>Total Incidents</p>
            </div>
            <div class=""stat-card"">
                <i class=""fas fa-user-md""></i>
                <h3>8</h3>
                <p>Total Responders</p>
            </div>
            <div class=""stat-card"">
                <i class=""fas fa-clock""></i>
                <h3>3</h3>
                <p>Active Incidents</p>
            </div>
            <div class=""stat-card"">
                <i class=""fas fa-users""></i>
                <h3>6</h3>
                <p>Available Responders</p>
            </div>
        </div>

        <div class=""content-grid"">
            <div class=""main-content"">
                <h2 class=""section-title"">Recent Incidents</h2>
                <div class=""incident-item"">
                    <h4>Medical Emergency</h4>
                    <p><strong>Location:</strong> Downtown Tampa</p>
                    <p><strong>Description:</strong> Cardiac arrest reported</p>
                    <p><strong>Priority:</strong> <span class=""status-badge priority-high"">high</span></p>
                    <p><strong>Status:</strong> <span class=""status-badge status-active"">active</span></p>
                    <p><strong>Created:</strong> 2025-01-17 12:30:00</p>
                </div>
                <div class=""incident-item"">
                    <h4>Traffic Accident</h4>
                    <p><strong>Location:</strong> I-275 North</p>
                    <p><strong>Description:</strong> Multi-vehicle collision</p>
                    <p><strong>Priority:</strong> <span class=""status-badge priority-medium"">medium</span></p>
                    <p><strong>Status:</strong> <span class=""status-badge status-active"">active</span></p>
                    <p><strong>Created:</strong> 2025-01-17 11:45:00</p>
                </div>
            </div>

            <div class=""sidebar"">
                <h2 class=""section-title"">Quick Actions</h2>
                <div class=""quick-actions"">
                    <a href=""/web/incidents"" class=""action-btn"">
                        <i class=""fas fa-exclamation-triangle""></i>
                        Report Incident
                    </a>
                    <a href=""/web/incidents"" class=""action-btn secondary"">
                        <i class=""fas fa-search""></i>
                        View Incidents
                    </a>
                </div>

                <div class=""quick-actions"">
                    <a href=""/web/admin"" class=""action-btn"">
                        <i class=""fas fa-cog""></i>
                        Admin Panel
                    </a>
                    <a href=""/web/responders"" class=""action-btn secondary"">
                        <i class=""fas fa-user-md""></i>
                        Manage Responders
                    </a>
                </div>

                <h2 class=""section-title"">Recent Notifications</h2>
                <div class=""notification-item"">
                    <h4>New Incident Reported</h4>
                    <p>Medical emergency reported in Downtown Tampa</p>
                    <p><small>2025-01-17 12:30:00</small></p>
                </div>
                <div class=""notification-item"">
                    <h4>Responder Available</h4>
                    <p>EMT Unit 3 is now available for dispatch</p>
                    <p><small>2025-01-17 12:15:00</small></p>
                </div>
            </div>
        </div>
    </div>
</body>
</html>";

            return Content(html, "text/html");
        }

        [HttpGet("login")]
        public IActionResult Login()
        {
            var html = @"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>EMS Tampa-FL Amptier - Login</title>
    <link href=""https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css"" rel=""stylesheet"">
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }

        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            color: #333;
        }

        .login-container {
            background: rgba(255, 255, 255, 0.95);
            backdrop-filter: blur(10px);
            border-radius: 20px;
            padding: 40px;
            box-shadow: 0 20px 40px rgba(0, 0, 0, 0.1);
            width: 100%;
            max-width: 400px;
            text-align: center;
        }

        .logo-section {
            margin-bottom: 20px;
        }

        .logo-bar {
            display: flex;
            justify-content: space-around;
            align-items: center;
            margin-bottom: 10px;
        }

        .logo-img {
            width: 60px;
            height: auto;
            border-radius: 10px;
            box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1);
        }

        .logo {
            width: 80px;
            height: 80px;
            margin: 0 auto 20px;
            background: linear-gradient(135deg, #3498db, #2980b9);
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            color: white;
            font-size: 2rem;
        }

        .login-container h1 {
            color: #2c3e50;
            margin-bottom: 10px;
            font-size: 1.8rem;
        }

        .login-container p {
            color: #7f8c8d;
            margin-bottom: 30px;
            font-size: 0.9rem;
        }

        .form-group {
            margin-bottom: 20px;
            text-align: left;
        }

        .form-group label {
            display: block;
            margin-bottom: 8px;
            font-weight: 600;
            color: #2c3e50;
            font-size: 0.9rem;
        }

        .form-group input {
            width: 100%;
            padding: 12px 15px;
            border: 2px solid #e0e0e0;
            border-radius: 10px;
            font-size: 1rem;
            transition: all 0.3s ease;
            background: rgba(255, 255, 255, 0.9);
        }

        .form-group input:focus {
            outline: none;
            border-color: #3498db;
            box-shadow: 0 0 0 3px rgba(52, 152, 219, 0.1);
        }

        .btn {
            background: linear-gradient(135deg, #3498db, #2980b9);
            color: white;
            border: none;
            padding: 15px 30px;
            border-radius: 10px;
            cursor: pointer;
            font-size: 1rem;
            font-weight: 600;
            width: 100%;
            transition: all 0.3s ease;
            margin-bottom: 20px;
        }

        .btn:hover {
            transform: translateY(-2px);
            box-shadow: 0 10px 20px rgba(52, 152, 219, 0.3);
        }

        .register-link {
            color: #7f8c8d;
            font-size: 0.9rem;
        }

        .register-link a {
            color: #3498db;
            text-decoration: none;
            font-weight: 600;
        }

        .register-link a:hover {
            text-decoration: underline;
        }

        @media (max-width: 480px) {
            .login-container {
                margin: 20px;
                padding: 30px 20px;
            }
        }
    </style>
</head>
<body>
    <div class=""login-container"">
        <div class=""logo-section"">
            <div class=""logo-bar"">
                <img src=""/images/rexus.png"" alt=""Rexus Logo"" class=""logo-img"">
                <img src=""/images/amptier.png"" alt=""Amptier Logo"" class=""logo-img"">
            </div>
        </div>
        <div class=""logo"">
            <i class=""fas fa-ambulance""></i>
        </div>
        <h1>EMS Tampa-FL Amptier</h1>
        <p>Emergency Management System</p>

        <form method=""POST"" action=""/api/auth/login"">
            <div class=""form-group"">
                <label for=""username"">Username</label>
                <input type=""text"" id=""username"" name=""username"" required placeholder=""Enter your username"">
            </div>
            <div class=""form-group"">
                <label for=""password"">Password</label>
                <input type=""password"" id=""password"" name=""password"" required placeholder=""Enter your password"">
            </div>
            <button type=""submit"" class=""btn"">
                <i class=""fas fa-sign-in-alt""></i> Sign In
            </button>
        </form>

        <div class=""register-link"">
            Don't have an account? <a href=""/web/register"">Register here</a>
        </div>
    </div>
</body>
</html>";

            return Content(html, "text/html");
        }

        [HttpGet("register")]
        public IActionResult Register()
        {
            var html = @"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>EMS Tampa-FL Amptier - Register</title>
    <link href=""https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css"" rel=""stylesheet"">
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }

        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            color: #333;
            padding: 20px;
        }

        .register-container {
            background: rgba(255, 255, 255, 0.95);
            backdrop-filter: blur(10px);
            border-radius: 20px;
            padding: 40px;
            box-shadow: 0 20px 40px rgba(0, 0, 0, 0.1);
            width: 100%;
            max-width: 500px;
        }

        .logo-section {
            text-align: center;
            margin-bottom: 20px;
        }

        .logo-bar {
            display: flex;
            justify-content: center;
            gap: 20px;
        }

        .logo-img {
            width: 100px;
            height: auto;
        }

        .logo {
            width: 80px;
            height: 80px;
            margin: 0 auto 20px;
            background: linear-gradient(135deg, #3498db, #2980b9);
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            color: white;
            font-size: 2rem;
        }

        .register-container h1 {
            color: #2c3e50;
            margin-bottom: 10px;
            font-size: 1.8rem;
            text-align: center;
        }

        .register-container p {
            color: #7f8c8d;
            margin-bottom: 30px;
            font-size: 0.9rem;
            text-align: center;
        }

        .form-grid {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 15px;
        }

        .form-group {
            margin-bottom: 20px;
        }

        .form-group.full-width {
            grid-column: 1 / -1;
        }

        .form-group label {
            display: block;
            margin-bottom: 8px;
            font-weight: 600;
            color: #2c3e50;
            font-size: 0.9rem;
        }

        .form-group input,
        .form-group select {
            width: 100%;
            padding: 12px 15px;
            border: 2px solid #e0e0e0;
            border-radius: 10px;
            font-size: 1rem;
            transition: all 0.3s ease;
            background: rgba(255, 255, 255, 0.9);
        }

        .form-group input:focus,
        .form-group select:focus {
            outline: none;
            border-color: #3498db;
            box-shadow: 0 0 0 3px rgba(52, 152, 219, 0.1);
        }

        .btn {
            background: linear-gradient(135deg, #3498db, #2980b9);
            color: white;
            border: none;
            padding: 15px 30px;
            border-radius: 10px;
            cursor: pointer;
            font-size: 1rem;
            font-weight: 600;
            width: 100%;
            transition: all 0.3s ease;
            margin-bottom: 20px;
        }

        .btn:hover {
            transform: translateY(-2px);
            box-shadow: 0 10px 20px rgba(52, 152, 219, 0.3);
        }

        .login-link {
            color: #7f8c8d;
            font-size: 0.9rem;
            text-align: center;
        }

        .login-link a {
            color: #3498db;
            text-decoration: none;
            font-weight: 600;
        }

        .login-link a:hover {
            text-decoration: underline;
        }

        .required {
            color: #e74c3c;
        }

        @media (max-width: 600px) {
            .form-grid {
                grid-template-columns: 1fr;
            }
            
            .register-container {
                padding: 30px 20px;
            }
        }
    </style>
</head>
<body>
    <div class=""register-container"">
        <div class=""logo-section"">
            <div class=""logo-bar"">
                <img src=""/images/rexus.png"" alt=""Rexus Logo"" class=""logo-img"">
                <img src=""/images/amptier.png"" alt=""Amptier Logo"" class=""logo-img"">
            </div>
        </div>
        <div class=""logo"">
            <i class=""fas fa-user-plus""></i>
        </div>
        <h1>Create Account</h1>
        <p>Join EMS Tampa-FL Amptier Emergency Management System</p>

        <form method=""POST"" action=""/api/auth/register"">
            <div class=""form-grid"">
                <div class=""form-group"">
                    <label for=""username"">Username <span class=""required"">*</span></label>
                    <input type=""text"" id=""username"" name=""username"" required placeholder=""Choose a username"">
                </div>
                <div class=""form-group"">
                    <label for=""email"">Email <span class=""required"">*</span></label>
                    <input type=""email"" id=""email"" name=""email"" required placeholder=""Enter your email"">
                </div>
            </div>

            <div class=""form-grid"">
                <div class=""form-group"">
                    <label for=""full_name"">Full Name <span class=""required"">*</span></label>
                    <input type=""text"" id=""full_name"" name=""full_name"" required placeholder=""Enter your full name"">
                </div>
                <div class=""form-group"">
                    <label for=""phone"">Phone Number</label>
                    <input type=""tel"" id=""phone"" name=""phone"" placeholder=""Enter your phone number"">
                </div>
            </div>

            <div class=""form-group full-width"">
                <label for=""address"">Address</label>
                <input type=""text"" id=""address"" name=""address"" placeholder=""Enter your address"">
            </div>

            <div class=""form-grid"">
                <div class=""form-group"">
                    <label for=""password"">Password <span class=""required"">*</span></label>
                    <input type=""password"" id=""password"" name=""password"" required placeholder=""Choose a password"">
                </div>
                <div class=""form-group"">
                    <label for=""confirm_password"">Confirm Password <span class=""required"">*</span></label>
                    <input type=""password"" id=""confirm_password"" name=""confirm_password"" required placeholder=""Confirm your password"">
                </div>
            </div>

            <button type=""submit"" class=""btn"">
                <i class=""fas fa-user-plus""></i> Create Account
            </button>
        </form>

        <div class=""login-link"">
            Already have an account? <a href=""/web/login"">Sign in here</a>
        </div>
    </div>
</body>
</html>";

            return Content(html, "text/html");
        }
    }
} 