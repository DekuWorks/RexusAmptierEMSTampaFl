from flask import Blueprint, request, jsonify
from datetime import datetime
import json

# Create API blueprint
api_bp = Blueprint('api', __name__)

# In-memory storage for demo purposes (in production, use a database)
emergency_incidents = []
emergency_responders = []
equipment_inventory = []

@api_bp.route('/health', methods=['GET'])
def health_check():
    """Health check endpoint"""
    return jsonify({
        'status': 'healthy',
        'timestamp': datetime.now().isoformat(),
        'service': 'EMS Tampa-FL Amptier API'
    })

@api_bp.route('/incidents', methods=['GET'])
def get_incidents():
    """Get all emergency incidents"""
    return jsonify({
        'incidents': emergency_incidents,
        'count': len(emergency_incidents)
    })

@api_bp.route('/incidents', methods=['POST'])
def create_incident():
    """Create a new emergency incident"""
    data = request.get_json()
    
    if not data:
        return jsonify({'error': 'No data provided'}), 400
    
    required_fields = ['type', 'location', 'description', 'priority']
    for field in required_fields:
        if field not in data:
            return jsonify({'error': f'Missing required field: {field}'}), 400
    
    incident = {
        'id': len(emergency_incidents) + 1,
        'type': data['type'],
        'location': data['location'],
        'description': data['description'],
        'priority': data['priority'],
        'status': 'active',
        'created_at': datetime.now().isoformat(),
        'updated_at': datetime.now().isoformat(),
        'assigned_responders': data.get('assigned_responders', []),
        'equipment_needed': data.get('equipment_needed', [])
    }
    
    emergency_incidents.append(incident)
    
    return jsonify({
        'message': 'Incident created successfully',
        'incident': incident
    }), 201

@api_bp.route('/incidents/<int:incident_id>', methods=['GET'])
def get_incident(incident_id):
    """Get a specific incident by ID"""
    incident = next((inc for inc in emergency_incidents if inc['id'] == incident_id), None)
    
    if not incident:
        return jsonify({'error': 'Incident not found'}), 404
    
    return jsonify(incident)

@api_bp.route('/incidents/<int:incident_id>', methods=['PUT'])
def update_incident(incident_id):
    """Update an incident"""
    incident = next((inc for inc in emergency_incidents if inc['id'] == incident_id), None)
    
    if not incident:
        return jsonify({'error': 'Incident not found'}), 404
    
    data = request.get_json()
    
    if not data:
        return jsonify({'error': 'No data provided'}), 400
    
    # Update allowed fields
    allowed_fields = ['status', 'description', 'priority', 'assigned_responders', 'equipment_needed']
    for field in allowed_fields:
        if field in data:
            incident[field] = data[field]
    
    incident['updated_at'] = datetime.now().isoformat()
    
    return jsonify({
        'message': 'Incident updated successfully',
        'incident': incident
    })

@api_bp.route('/responders', methods=['GET'])
def get_responders():
    """Get all emergency responders"""
    return jsonify({
        'responders': emergency_responders,
        'count': len(emergency_responders)
    })

@api_bp.route('/responders', methods=['POST'])
def create_responder():
    """Create a new emergency responder"""
    data = request.get_json()
    
    if not data:
        return jsonify({'error': 'No data provided'}), 400
    
    required_fields = ['name', 'role', 'contact_number']
    for field in required_fields:
        if field not in data:
            return jsonify({'error': f'Missing required field: {field}'}), 400
    
    responder = {
        'id': len(emergency_responders) + 1,
        'name': data['name'],
        'role': data['role'],
        'contact_number': data['contact_number'],
        'status': 'available',
        'current_location': data.get('current_location', ''),
        'specializations': data.get('specializations', []),
        'created_at': datetime.now().isoformat()
    }
    
    emergency_responders.append(responder)
    
    return jsonify({
        'message': 'Responder created successfully',
        'responder': responder
    }), 201

@api_bp.route('/equipment', methods=['GET'])
def get_equipment():
    """Get all equipment inventory"""
    return jsonify({
        'equipment': equipment_inventory,
        'count': len(equipment_inventory)
    })

@api_bp.route('/equipment', methods=['POST'])
def add_equipment():
    """Add new equipment to inventory"""
    data = request.get_json()
    
    if not data:
        return jsonify({'error': 'No data provided'}), 400
    
    required_fields = ['name', 'type', 'quantity']
    for field in required_fields:
        if field not in data:
            return jsonify({'error': f'Missing required field: {field}'}), 400
    
    equipment = {
        'id': len(equipment_inventory) + 1,
        'name': data['name'],
        'type': data['type'],
        'quantity': data['quantity'],
        'available_quantity': data.get('available_quantity', data['quantity']),
        'location': data.get('location', ''),
        'status': 'available',
        'last_maintenance': data.get('last_maintenance', ''),
        'created_at': datetime.now().isoformat()
    }
    
    equipment_inventory.append(equipment)
    
    return jsonify({
        'message': 'Equipment added successfully',
        'equipment': equipment
    }), 201

@api_bp.route('/dashboard/stats', methods=['GET'])
def get_dashboard_stats():
    """Get dashboard statistics"""
    active_incidents = [inc for inc in emergency_incidents if inc['status'] == 'active']
    available_responders = [resp for resp in emergency_responders if resp['status'] == 'available']
    available_equipment = [eq for eq in equipment_inventory if eq['status'] == 'available']
    
    return jsonify({
        'total_incidents': len(emergency_incidents),
        'active_incidents': len(active_incidents),
        'total_responders': len(emergency_responders),
        'available_responders': len(available_responders),
        'total_equipment': len(equipment_inventory),
        'available_equipment': len(available_equipment),
        'last_updated': datetime.now().isoformat()
    }) 