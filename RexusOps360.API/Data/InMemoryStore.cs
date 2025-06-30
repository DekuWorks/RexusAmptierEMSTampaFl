using RexusOps360.API.Models;

namespace RexusOps360.API.Data
{
    public class InMemoryStore
    {
        private static readonly List<Incident> _incidents = new();
        private static readonly List<Responder> _responders = new();
        private static readonly List<Equipment> _equipment = new();
        
        private static int _nextIncidentId = 1;
        private static int _nextResponderId = 1;
        private static int _nextEquipmentId = 1;

        static InMemoryStore()
        {
            SeedData();
        }

        // Incident operations
        public static List<Incident> GetAllIncidents() => _incidents.ToList();
        
        public static Incident? GetIncidentById(int id) => _incidents.FirstOrDefault(i => i.Id == id);
        
        public static Incident CreateIncident(Incident incident)
        {
            incident.Id = _nextIncidentId++;
            incident.CreatedAt = DateTime.UtcNow;
            incident.UpdatedAt = DateTime.UtcNow;
            _incidents.Add(incident);
            return incident;
        }
        
        public static Incident? UpdateIncident(int id, Incident updatedIncident)
        {
            var existing = _incidents.FirstOrDefault(i => i.Id == id);
            if (existing == null) return null;
            
            existing.Type = updatedIncident.Type;
            existing.Location = updatedIncident.Location;
            existing.Description = updatedIncident.Description;
            existing.Priority = updatedIncident.Priority;
            existing.Status = updatedIncident.Status;
            existing.AssignedResponders = updatedIncident.AssignedResponders;
            existing.EquipmentNeeded = updatedIncident.EquipmentNeeded;
            existing.UpdatedAt = DateTime.UtcNow;
            
            return existing;
        }

        public static bool DeleteIncident(int id)
        {
            var incident = _incidents.FirstOrDefault(i => i.Id == id);
            if (incident == null) return false;
            
            return _incidents.Remove(incident);
        }

        // Responder operations
        public static List<Responder> GetAllResponders() => _responders.ToList();
        
        public static Responder CreateResponder(Responder responder)
        {
            responder.Id = _nextResponderId++;
            responder.CreatedAt = DateTime.UtcNow;
            _responders.Add(responder);
            return responder;
        }

        // Equipment operations
        public static List<Equipment> GetAllEquipment() => _equipment.ToList();
        
        public static Equipment CreateEquipment(Equipment equipment)
        {
            equipment.Id = _nextEquipmentId++;
            equipment.CreatedAt = DateTime.UtcNow;
            if (equipment.AvailableQuantity == 0)
                equipment.AvailableQuantity = equipment.Quantity;
            _equipment.Add(equipment);
            return equipment;
        }

        // Dashboard statistics
        public static object GetDashboardStats()
        {
            var activeIncidents = _incidents.Count(i => i.Status == "active");
            var availableResponders = _responders.Count(r => r.Status == "available");
            var availableEquipment = _equipment.Count(e => e.Status == "available");
            
            return new
            {
                total_incidents = _incidents.Count,
                active_incidents = activeIncidents,
                total_responders = _responders.Count,
                available_responders = availableResponders,
                total_equipment = _equipment.Count,
                available_equipment = availableEquipment,
                last_updated = DateTime.UtcNow
            };
        }

        private static void SeedData()
        {
            // Seed mock incidents
            CreateIncident(new Incident
            {
                Type = "Medical Emergency",
                Location = "123 Main St, Tampa, FL",
                Description = "Patient experiencing chest pain",
                Priority = "high",
                Status = "active"
            });

            CreateIncident(new Incident
            {
                Type = "Traffic Accident",
                Location = "456 Oak Ave, Tampa, FL",
                Description = "Two-vehicle collision on highway",
                Priority = "medium",
                Status = "active"
            });

            // Seed mock responders
            CreateResponder(new Responder
            {
                Name = "John Smith",
                Role = "Paramedic",
                ContactNumber = "(813) 555-0101",
                CurrentLocation = "Downtown Station",
                Specializations = new List<string> { "Cardiac", "Trauma" },
                Status = "available"
            });

            CreateResponder(new Responder
            {
                Name = "Sarah Johnson",
                Role = "EMT",
                ContactNumber = "(813) 555-0102",
                CurrentLocation = "North Station",
                Specializations = new List<string> { "Pediatric" },
                Status = "available"
            });

            CreateResponder(new Responder
            {
                Name = "Mike Davis",
                Role = "Firefighter",
                ContactNumber = "(813) 555-0103",
                CurrentLocation = "Central Station",
                Specializations = new List<string> { "Fire Suppression", "Rescue" },
                Status = "available"
            });

            // Seed mock equipment
            CreateEquipment(new Equipment
            {
                Name = "Ambulance",
                Type = "Transport",
                Quantity = 5,
                AvailableQuantity = 4,
                Location = "Main Garage",
                Status = "available"
            });

            CreateEquipment(new Equipment
            {
                Name = "Defibrillator",
                Type = "Medical",
                Quantity = 10,
                AvailableQuantity = 8,
                Location = "Medical Supply Room",
                Status = "available"
            });

            CreateEquipment(new Equipment
            {
                Name = "Radio Communication System",
                Type = "Communication",
                Quantity = 15,
                AvailableQuantity = 12,
                Location = "Communication Center",
                Status = "available"
            });
        }
    }
} 