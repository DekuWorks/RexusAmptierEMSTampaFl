from flask import Blueprint, request, jsonify, current_app
from werkzeug.utils import secure_filename
import sqlite3
import os
from datetime import datetime
import requests
from geopy.geocoders import Nominatim
from functools import wraps
from werkzeug.security import check_password_hash

api_bp = Blueprint('api', __name__)

def get_db():
    """Get database connection"""
    conn = sqlite3.connect('ems_tampa.db')
    conn.row_factory = sqlite3.Row
    return conn

def allowed_file(filename):
    """Check if file extension is allowed"""
    ALLOWED_EXTENSIONS = {'png', 'jpg', 'jpeg', 'gif', 'pdf'}
    return '.' in filename and filename.rsplit('.', 1)[1].lower() in ALLOWED_EXTENSIONS

def get_geolocation(location):
    """Get latitude and longitude for a location"""
    try:
        geolocator = Nominatim(user_agent="ems_tampa_app")
        loc = geolocator.geocode(location, timeout=10)
        if loc:
            return loc.latitude, loc.longitude
    except Exception as e:
        print(f"Geolocation error: {e}")
    return None, None

def get_weather_data():
    """Get weather data for Tampa, FL"""
    api_key = os.getenv("WEATHER_API_KEY")
    if not api_key:
        return None
    
    try:
        url = f"https://api.openweathermap.org/data/2.5/weather?q=Tampa,FL&appid={api_key}&units=metric"
        response = requests.get(url, timeout=10)
        if response.status_code == 200:
            data = response.json()
            return {
                "temp": data["main"]["temp"],
                "description": data["weather"][0]["description"].title(),
                "humidity": data["main"]["humidity"],
                "wind_speed": data["wind"]["speed"]
            }
    except Exception as e:
        print(f"Weather API error: {e}")
    return None

def require_auth(f):
    """Decorator to require authentication"""
    @wraps(f)
    def decorated_function(*args, **kwargs):
        # For API endpoints, we'll use a simple token-based auth
        # In a real application, you'd use JWT tokens
        auth_header = request.headers.get('Authorization')
        if not auth_header:
            return jsonify({'error': 'Authentication required'}), 401
        
        # For demo purposes, accept any Authorization header
        # In production, validate the token
        return f(*args, **kwargs)
    return decorated_function

def require_role(allowed_roles):
    """Decorator to require specific roles"""
    def decorator(f):
        @wraps(f)
        def decorated_function(*args, **kwargs):
            # For API endpoints, we'll check user role from request
            user_role = request.headers.get('X-User-Role', 'public')
            if user_role not in allowed_roles:
                return jsonify({'error': 'Insufficient permissions'}), 403
            return f(*args, **kwargs)
        return decorated_function
    return decorator

@api_bp.route('/incidents', methods=['GET'])
@require_auth
def get_incidents():
    """Get all incidents with optional filtering"""
    try:
        db = get_db()
        
        # Get query parameters for filtering
        status = request.args.get('status')
        priority = request.args.get('priority')
        user_role = request.headers.get('X-User-Role', 'public')
        user_id = request.headers.get('X-User-ID')
        
        query = "SELECT * FROM incidents WHERE 1=1"
        params = []
        
        if status:
            query += " AND status = ?"
            params.append(status)
        
        if priority:
            query += " AND priority = ?"
            params.append(priority)
        
        # Public users can only see their own incidents
        if user_role == 'public' and user_id:
            query += " AND user_id = ?"
            params.append(user_id)
        
        query += " ORDER BY created_at DESC"
        
        incidents = db.execute(query, params).fetchall()
        db.close()
        
        return jsonify({
            'incidents': [dict(incident) for incident in incidents]
        })
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@api_bp.route('/incidents', methods=['POST'])
@require_auth
def create_incident():
    """Create a new incident"""
    try:
        data = request.form.to_dict()
        user_id = request.headers.get('X-User-ID')
        user_role = request.headers.get('X-User-Role', 'public')
        
        # Validate required fields
        required_fields = ['type', 'location', 'description', 'priority']
        for field in required_fields:
            if not data.get(field):
                return jsonify({'error': f'{field} is required'}), 400
        
        # Get geolocation
        lat, lng = get_geolocation(data['location'])
        
        # Handle file upload
        photo_path = None
        if 'photo' in request.files:
            file = request.files['photo']
            if file and allowed_file(file.filename):
                filename = secure_filename(file.filename)
                timestamp = datetime.now().strftime('%Y%m%d_%H%M%S')
                filename = f"{timestamp}_{filename}"
                file.save(os.path.join(current_app.config['UPLOAD_FOLDER'], filename))
                photo_path = f"uploads/{filename}"
        
        db = get_db()
        cursor = db.cursor()
        
        cursor.execute('''
            INSERT INTO incidents (type, location, description, priority, latitude, longitude, photo_path, reported_by, user_id)
            VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)
        ''', (
            data['type'],
            data['location'],
            data['description'],
            data['priority'],
            lat,
            lng,
            photo_path,
            request.headers.get('X-User-Name', 'Anonymous'),
            user_id
        ))
        
        incident_id = cursor.lastrowid
        
        # Create notification for new incident
        cursor.execute('''
            INSERT INTO notifications (title, message, category, area)
            VALUES (?, ?, ?, ?)
        ''', (
            f"New {data['type']} Incident",
            f"New {data['priority']} priority incident reported at {data['location']}",
            "incident",
            data['location']
        ))
        
        db.commit()
        db.close()
        
        return jsonify({
            'message': 'Incident created successfully',
            'incident_id': incident_id
        }), 201
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@api_bp.route('/incidents/<int:incident_id>', methods=['PUT'])
@require_auth
@require_role(['admin', 'dispatcher', 'responder'])
def update_incident(incident_id):
    """Update an incident"""
    try:
        data = request.get_json()
        db = get_db()
        
        # Check if incident exists
        incident = db.execute('SELECT * FROM incidents WHERE id = ?', (incident_id,)).fetchone()
        if not incident:
            db.close()
            return jsonify({'error': 'Incident not found'}), 404
        
        # Update fields
        update_fields = []
        params = []
        
        for field in ['status', 'assigned_responders', 'equipment_needed']:
            if field in data:
                update_fields.append(f"{field} = ?")
                params.append(data[field])
        
        if update_fields:
            params.append(incident_id)
            query = f"UPDATE incidents SET {', '.join(update_fields)}, updated_at = ? WHERE id = ?"
            params.insert(-1, datetime.now())
            
            db.execute(query, params)
            db.commit()
        
        db.close()
        
        return jsonify({'message': 'Incident updated successfully'})
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@api_bp.route('/incidents/<int:incident_id>', methods=['DELETE'])
@require_auth
@require_role(['admin'])
def delete_incident(incident_id):
    """Delete an incident"""
    try:
        db = get_db()
        
        # Check if incident exists
        incident = db.execute('SELECT * FROM incidents WHERE id = ?', (incident_id,)).fetchone()
        if not incident:
            db.close()
            return jsonify({'error': 'Incident not found'}), 404
        
        # Delete incident
        db.execute('DELETE FROM incidents WHERE id = ?', (incident_id,))
        db.commit()
        db.close()
        
        return jsonify({'message': 'Incident deleted successfully'})
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@api_bp.route('/responders', methods=['GET'])
@require_auth
def get_responders():
    """Get all responders"""
    try:
        db = get_db()
        responders = db.execute('SELECT * FROM responders ORDER BY created_at DESC').fetchall()
        db.close()
        
        return jsonify({
            'responders': [dict(responder) for responder in responders]
        })
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@api_bp.route('/responders', methods=['POST'])
@require_auth
@require_role(['admin', 'dispatcher'])
def create_responder():
    """Create a new responder"""
    try:
        data = request.get_json()
        
        # Validate required fields
        required_fields = ['name', 'role', 'contact_number']
        for field in required_fields:
            if not data.get(field):
                return jsonify({'error': f'{field} is required'}), 400
        
        db = get_db()
        cursor = db.cursor()
        
        cursor.execute('''
            INSERT INTO responders (name, role, contact_number, current_location)
            VALUES (?, ?, ?, ?)
        ''', (
            data['name'],
            data['role'],
            data['contact_number'],
            data.get('current_location', '')
        ))
        
        responder_id = cursor.lastrowid
        db.commit()
        db.close()
        
        return jsonify({
            'message': 'Responder added successfully',
            'responder_id': responder_id
        }), 201
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@api_bp.route('/equipment', methods=['GET'])
@require_auth
def get_equipment():
    """Get all equipment"""
    try:
        db = get_db()
        equipment = db.execute('SELECT * FROM equipment ORDER BY created_at DESC').fetchall()
        db.close()
        
        return jsonify({
            'equipment': [dict(item) for item in equipment]
        })
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@api_bp.route('/equipment', methods=['POST'])
@require_auth
@require_role(['admin', 'dispatcher'])
def create_equipment():
    """Create new equipment"""
    try:
        data = request.get_json()
        
        # Validate required fields
        required_fields = ['name', 'type', 'quantity']
        for field in required_fields:
            if not data.get(field):
                return jsonify({'error': f'{field} is required'}), 400
        
        db = get_db()
        cursor = db.cursor()
        
        cursor.execute('''
            INSERT INTO equipment (name, type, quantity, available_quantity, location)
            VALUES (?, ?, ?, ?, ?)
        ''', (
            data['name'],
            data['type'],
            data['quantity'],
            data['quantity'],  # Initially all available
            data.get('location', '')
        ))
        
        equipment_id = cursor.lastrowid
        db.commit()
        db.close()
        
        return jsonify({
            'message': 'Equipment added successfully',
            'equipment_id': equipment_id
        }), 201
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@api_bp.route('/dashboard/stats')
@require_auth
def get_dashboard_stats():
    """Get dashboard statistics"""
    try:
        db = get_db()
        
        # Get basic stats
        total_incidents = db.execute('SELECT COUNT(*) FROM incidents').fetchone()[0]
        active_incidents = db.execute('SELECT COUNT(*) FROM incidents WHERE status = ?', ('active',)).fetchone()[0]
        total_responders = db.execute('SELECT COUNT(*) FROM responders').fetchone()[0]
        available_responders = db.execute('SELECT COUNT(*) FROM responders WHERE status = ?', ('available',)).fetchone()[0]
        total_equipment = db.execute('SELECT COUNT(*) FROM equipment').fetchone()[0]
        
        # Get category stats
        category_stats = db.execute('''
            SELECT type, COUNT(*) as count 
            FROM incidents 
            GROUP BY type 
            ORDER BY count DESC
        ''').fetchall()
        
        # Get recent incidents for timeline
        recent_incidents = db.execute('''
            SELECT * FROM incidents 
            ORDER BY created_at DESC 
            LIMIT 7
        ''').fetchall()
        
        db.close()
        
        return jsonify({
            'total_incidents': total_incidents,
            'active_incidents': active_incidents,
            'total_responders': total_responders,
            'available_responders': available_responders,
            'total_equipment': total_equipment,
            'category_stats': [dict(stat) for stat in category_stats],
            'recent_incidents': [dict(incident) for incident in recent_incidents]
        })
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@api_bp.route('/incidents/locations')
@require_auth
def get_incident_locations():
    """Get incident locations for map"""
    try:
        db = get_db()
        locations = db.execute('''
            SELECT id, type, location, priority, status, latitude, longitude
            FROM incidents 
            WHERE latitude IS NOT NULL AND longitude IS NOT NULL
        ''').fetchall()
        db.close()
        
        return jsonify({
            'locations': [dict(location) for location in locations]
        })
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@api_bp.route('/weather')
def get_weather():
    """Get weather data"""
    weather_data = get_weather_data()
    if weather_data:
        return jsonify(weather_data)
    return jsonify({'error': 'Weather data unavailable'}), 500

@api_bp.route('/notifications', methods=['GET'])
@require_auth
def get_notifications():
    """Get notifications"""
    try:
        db = get_db()
        notifications = db.execute('SELECT * FROM notifications ORDER BY created_at DESC LIMIT 10').fetchall()
        db.close()
        
        return jsonify({
            'notifications': [dict(notification) for notification in notifications]
        })
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@api_bp.route('/notifications', methods=['POST'])
@require_auth
@require_role(['admin', 'dispatcher'])
def create_notification():
    """Create a new notification"""
    try:
        data = request.get_json()
        
        # Validate required fields
        required_fields = ['title', 'message', 'category', 'area']
        for field in required_fields:
            if not data.get(field):
                return jsonify({'error': f'{field} is required'}), 400
        
        db = get_db()
        cursor = db.cursor()
        
        cursor.execute('''
            INSERT INTO notifications (title, message, category, area)
            VALUES (?, ?, ?, ?)
        ''', (
            data['title'],
            data['message'],
            data['category'],
            data['area']
        ))
        
        notification_id = cursor.lastrowid
        db.commit()
        db.close()
        
        return jsonify({
            'message': 'Notification created successfully',
            'notification_id': notification_id
        }), 201
    except Exception as e:
        return jsonify({'error': str(e)}), 500 

@api_bp.route('/complaint', methods=['POST'])
def submit_complaint():
    """Handle user complaint form submission with reCAPTCHA verification"""
    import requests
    recaptcha_secret = 'YOUR_SECRET_KEY'  # Replace with your actual secret key
    name = request.form.get('name')
    email = request.form.get('email')
    complaint = request.form.get('complaint')
    recaptcha_response = request.form.get('g-recaptcha-response')

    if not (name and email and complaint and recaptcha_response):
        return jsonify({'error': 'All fields and reCAPTCHA are required.'}), 400

    # Verify reCAPTCHA
    verify_url = 'https://www.google.com/recaptcha/api/siteverify'
    payload = {
        'secret': recaptcha_secret,
        'response': recaptcha_response
    }
    r = requests.post(verify_url, data=payload)
    result = r.json()
    if not result.get('success'):
        return jsonify({'error': 'Invalid reCAPTCHA. Please try again.'}), 400

    # Here you would save the complaint to the database or send an email, etc.
    # For now, just return a success message.
    return jsonify({'message': 'Complaint submitted successfully!'}), 200 

@api_bp.route('/login', methods=['POST'])
def api_login():
    data = request.get_json()
    username = data.get('username')
    password = data.get('password')

    if not username or not password:
        return jsonify({'error': 'Username and password required.'}), 400

    db = get_db()
    user = db.execute('SELECT * FROM users WHERE username = ? AND is_active = 1', (username,)).fetchone()
    db.close()

    if user and check_password_hash(user['password_hash'], password):
        return jsonify({'message': 'Login successful', 'role': user['role'], 'full_name': user['full_name']}), 200
    else:
        return jsonify({'error': 'Invalid username or password.'}), 401 