using Microsoft.AspNetCore.SignalR;
using RexusOps360.API.Models;

namespace RexusOps360.API.Hubs
{
    public class EmsHub : Hub
    {
        private static readonly Dictionary<string, string> _userConnections = new();
        private static readonly Dictionary<string, string> _responderLocations = new();

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            await Clients.All.SendAsync("UserConnected", Context.ConnectionId);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Remove user connection
            var userToRemove = _userConnections.FirstOrDefault(x => x.Value == Context.ConnectionId);
            if (!string.IsNullOrEmpty(userToRemove.Key))
            {
                _userConnections.Remove(userToRemove.Key);
            }

            await base.OnDisconnectedAsync(exception);
            await Clients.All.SendAsync("UserDisconnected", Context.ConnectionId);
        }

        // Join a specific group (e.g., dispatchers, responders, admin)
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("UserJoinedGroup", Context.ConnectionId, groupName);
        }

        // Leave a specific group
        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("UserLeftGroup", Context.ConnectionId, groupName);
        }

        // Register user with their role for targeted messaging
        public async Task RegisterUser(string userId, string role)
        {
            _userConnections[userId] = Context.ConnectionId;
            await Groups.AddToGroupAsync(Context.ConnectionId, role);
            await Clients.All.SendAsync("UserRegistered", userId, role);
        }

        // Update responder location
        public async Task UpdateResponderLocation(string responderId, string location, double? latitude, double? longitude)
        {
            _responderLocations[responderId] = location;
            var locationData = new
            {
                ResponderId = responderId,
                Location = location,
                Latitude = latitude,
                Longitude = longitude,
                Timestamp = DateTime.UtcNow
            };

            await Clients.Group("dispatchers").SendAsync("ResponderLocationUpdated", locationData);
            await Clients.Group("admin").SendAsync("ResponderLocationUpdated", locationData);
        }

        // Send incident update to all relevant parties
        public async Task SendIncidentUpdate(object incidentData)
        {
            await Clients.Group("dispatchers").SendAsync("IncidentUpdated", incidentData);
            await Clients.Group("admin").SendAsync("IncidentUpdated", incidentData);
            await Clients.Group("responders").SendAsync("IncidentUpdated", incidentData);
        }

        // Send emergency alert to all users
        public async Task SendEmergencyAlert(string message, string priority, string area)
        {
            var alertData = new
            {
                Message = message,
                Priority = priority,
                Area = area,
                Timestamp = DateTime.UtcNow
            };

            await Clients.All.SendAsync("EmergencyAlert", alertData);
        }

        // Send notification to specific role
        public async Task SendNotificationToRole(string role, string title, string message, string type)
        {
            var notificationData = new
            {
                Title = title,
                Message = message,
                Type = type,
                Timestamp = DateTime.UtcNow
            };

            await Clients.Group(role).SendAsync("Notification", notificationData);
        }

        // Send equipment status update
        public async Task SendEquipmentUpdate(object equipmentData)
        {
            await Clients.Group("dispatchers").SendAsync("EquipmentUpdated", equipmentData);
            await Clients.Group("admin").SendAsync("EquipmentUpdated", equipmentData);
        }

        // Send weather alert
        public async Task SendWeatherAlert(string alert, string severity)
        {
            var weatherData = new
            {
                Alert = alert,
                Severity = severity,
                Timestamp = DateTime.UtcNow
            };

            await Clients.All.SendAsync("WeatherAlert", weatherData);
        }

        // Get all connected users
        public async Task GetConnectedUsers()
        {
            var connectedUsers = _userConnections.Select(kvp => new
            {
                UserId = kvp.Key,
                ConnectionId = kvp.Value
            }).ToList();

            await Clients.Caller.SendAsync("ConnectedUsers", connectedUsers);
        }

        // Get all responder locations
        public async Task GetResponderLocations()
        {
            var locations = _responderLocations.Select(kvp => new
            {
                ResponderId = kvp.Key,
                Location = kvp.Value
            }).ToList();

            await Clients.Caller.SendAsync("ResponderLocations", locations);
        }

        // Send chat message between dispatcher and responder
        public async Task SendChatMessage(string fromUserId, string toUserId, string message)
        {
            var chatData = new
            {
                FromUserId = fromUserId,
                ToUserId = toUserId,
                Message = message,
                Timestamp = DateTime.UtcNow
            };

            // Send to specific users if they're connected
            if (_userConnections.ContainsKey(toUserId))
            {
                await Clients.Client(_userConnections[toUserId]).SendAsync("ChatMessage", chatData);
            }
            if (_userConnections.ContainsKey(fromUserId))
            {
                await Clients.Client(_userConnections[fromUserId]).SendAsync("ChatMessage", chatData);
            }
        }

        // Update incident status
        public async Task UpdateIncidentStatus(int incidentId, string status, string updatedBy)
        {
            var statusData = new
            {
                IncidentId = incidentId,
                Status = status,
                UpdatedBy = updatedBy,
                Timestamp = DateTime.UtcNow
            };

            await Clients.All.SendAsync("IncidentStatusUpdated", statusData);
        }

        // Send system health update
        public async Task SendSystemHealthUpdate(object healthData)
        {
            await Clients.Group("admin").SendAsync("SystemHealthUpdated", healthData);
        }
    }
} 