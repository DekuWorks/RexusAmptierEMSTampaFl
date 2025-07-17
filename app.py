from flask import Flask, render_template, request, jsonify, session, redirect, url_for, flash, send_from_directory
from flask_cors import CORS
import sqlite3
import os
from werkzeug.utils import secure_filename
from werkzeug.security import generate_password_hash, check_password_hash
from datetime import datetime
import requests
from geopy.geocoders import Nominatim
from dotenv import load_dotenv
from functools import wraps

# Load environment variables
load_dotenv()

app = Flask(__name__)
app.secret_key = os.getenv('SECRET_KEY', 'ems_tampa_secret_key_2025')

CORS(app)

# Configuration
app.config['UPLOAD_FOLDER'] = 'uploads'
app.config['MAX_CONTENT_LENGTH'] = 16 * 1024 * 1024  # 16MB max file size
ALLOWED_EXTENSIONS = {'png', 'jpg', 'jpeg', 'gif', 'pdf'}

# Create uploads directory
os.makedirs(app.config['UPLOAD_FOLDER'], exist_ok=True)

# Database setup
DATABASE = 'ems_tampa.db'

def init_db():
    """Initialize the database with schema"""
    conn = sqlite3.connect(DATABASE)
    cursor = conn.cursor()
    
    # Create incidents table
    cursor.execute('''
        CREATE TABLE IF NOT EXISTS incidents (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            type TEXT NOT NULL,
            location TEXT NOT NULL,
            description TEXT,
            priority TEXT NOT NULL,
            status TEXT DEFAULT 'active',
            latitude REAL,
            longitude REAL,
            photo_path TEXT,
            created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
            updated_at DATETIME DEFAULT CURRENT_TIMESTAMP,
            assigned_responders TEXT,
            equipment_needed TEXT,
            reported_by TEXT,
            user_id INTEGER
        )
    ''')
    
    # Create responders table
    cursor.execute('''
        CREATE TABLE IF NOT EXISTS responders (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            name TEXT NOT NULL,
            role TEXT NOT NULL,
            contact_number TEXT NOT NULL,
            current_location TEXT,
            status TEXT DEFAULT 'available',
            specializations TEXT,
            created_at DATETIME DEFAULT CURRENT_TIMESTAMP
        )
    ''')
    
    # Create equipment table
    cursor.execute('''
        CREATE TABLE IF NOT EXISTS equipment (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            name TEXT NOT NULL,
            type TEXT NOT NULL,
            quantity INTEGER NOT NULL,
            available_quantity INTEGER NOT NULL,
            location TEXT,
            status TEXT DEFAULT 'available',
            last_maintenance DATETIME,
            created_at DATETIME DEFAULT CURRENT_TIMESTAMP
        )
    ''')
    
    # Create notifications table
    cursor.execute('''
        CREATE TABLE IF NOT EXISTS notifications (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            title TEXT NOT NULL,
            message TEXT NOT NULL,
            category TEXT NOT NULL,
            area TEXT NOT NULL,
            created_at DATETIME DEFAULT CURRENT_TIMESTAMP
        )
    ''')
    
    # Create users table
    cursor.execute('''
        CREATE TABLE IF NOT EXISTS users (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            username TEXT NOT NULL UNIQUE,
            email TEXT NOT NULL UNIQUE,
            password_hash TEXT NOT NULL,
            full_name TEXT NOT NULL,
            role TEXT NOT NULL DEFAULT 'public',
            phone TEXT,
            address TEXT,
            is_active BOOLEAN DEFAULT 1,
            created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
            last_login DATETIME
        )
    ''')
    
    # Insert default admin user if not exists
    cursor.execute('''
        INSERT OR IGNORE INTO users (username, email, password_hash, full_name, role)
        VALUES (?, ?, ?, ?, ?)
    ''', (
        'admin',
        'admin@ems-tampa.com',
        generate_password_hash('admin123'),
        'System Administrator',
        'admin'
    ))
    
    # Insert default dispatcher
    cursor.execute('''
        INSERT OR IGNORE INTO users (username, email, password_hash, full_name, role)
        VALUES (?, ?, ?, ?, ?)
    ''', (
        'dispatcher',
        'dispatcher@ems-tampa.com',
        generate_password_hash('dispatch123'),
        'Emergency Dispatcher',
        'dispatcher'
    ))
    
    # Insert default responder
    cursor.execute('''
        INSERT OR IGNORE INTO users (username, email, password_hash, full_name, role)
        VALUES (?, ?, ?, ?, ?)
    ''', (
        'responder',
        'responder@ems-tampa.com',
        generate_password_hash('respond123'),
        'Emergency Responder',
        'responder'
    ))
    
    conn.commit()
    conn.close()

def get_db():
    """Get database connection"""
    conn = sqlite3.connect(DATABASE)
    conn.row_factory = sqlite3.Row
    return conn

def get_user_data():
    """Get user data for dashboard"""
    if 'user_id' not in session:
        return None
    
    db = get_db()
    user = db.execute('SELECT * FROM users WHERE id = ?', (session['user_id'],)).fetchone()
    db.close()
    return user

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

def login_required(f):
    """Decorator to require login"""
    @wraps(f)
    def decorated_function(*args, **kwargs):
        if 'user_id' not in session:
            flash('Please log in to access this page.', 'error')
            return redirect(url_for('login'))
        return f(*args, **kwargs)
    return decorated_function

def role_required(allowed_roles):
    """Decorator to require specific roles"""
    def decorator(f):
        @wraps(f)
        def decorated_function(*args, **kwargs):
            if 'user_id' not in session:
                flash('Please log in to access this page.', 'error')
                return redirect(url_for('login'))
            
            db = get_db()
            user = db.execute('SELECT role FROM users WHERE id = ?', (session['user_id'],)).fetchone()
            db.close()
            
            if not user or user['role'] not in allowed_roles:
                flash('Access denied. Insufficient permissions.', 'error')
                return redirect(url_for('dashboard'))
            
            return f(*args, **kwargs)
        return decorated_function
    return decorator

# Initialize database
init_db()

@app.route('/')
def home():
    return redirect(url_for('login'))

@app.route('/login', methods=['GET', 'POST'])
def login():
    if request.method == 'POST':
        username = request.form.get('username')
        password = request.form.get('password')
        
        if not username or not password:
            flash('Please provide both username and password.', 'error')
            return render_template('login.html')
        
        db = get_db()
        user = db.execute('SELECT * FROM users WHERE username = ? AND is_active = 1', (username,)).fetchone()
        db.close()
        
        if user and check_password_hash(user['password_hash'], password):
            session['user_id'] = user['id']
            session['username'] = user['username']
            session['role'] = user['role']
            session['full_name'] = user['full_name']
            
            # Update last login
            db = get_db()
            db.execute('UPDATE users SET last_login = ? WHERE id = ?', (datetime.now(), user['id']))
            db.commit()
            db.close()
            
            flash(f'Welcome back, {user["full_name"]}!', 'success')
            return redirect(url_for('dashboard'))
        else:
            flash('Invalid username or password.', 'error')
    
    return render_template('login.html')

@app.route('/register', methods=['GET', 'POST'])
def register():
    if request.method == 'POST':
        username = request.form.get('username')
        email = request.form.get('email')
        password = request.form.get('password')
        full_name = request.form.get('full_name')
        phone = request.form.get('phone')
        address = request.form.get('address')
        
        if not all([username, email, password, full_name]):
            flash('Please fill in all required fields.', 'error')
            return render_template('register.html')
        
        db = get_db()
        
        # Check if username or email already exists
        existing_user = db.execute('SELECT id FROM users WHERE username = ? OR email = ?', (username, email)).fetchone()
        if existing_user:
            flash('Username or email already exists.', 'error')
            db.close()
            return render_template('register.html')
        
        # Create new user (default role is 'public')
        password_hash = generate_password_hash(password)
        cursor = db.cursor()
        cursor.execute('''
            INSERT INTO users (username, email, password_hash, full_name, phone, address, role)
            VALUES (?, ?, ?, ?, ?, ?, ?)
        ''', (username, email, password_hash, full_name, phone, address, 'public'))
        
        user_id = cursor.lastrowid
        db.commit()
        db.close()
        
        # Auto-login after registration
        session['user_id'] = user_id
        session['username'] = username
        session['role'] = 'public'
        session['full_name'] = full_name
        
        flash('Registration successful! Welcome to EMS Tampa-FL Amptier.', 'success')
        return redirect(url_for('dashboard'))
    
    return render_template('register.html')

@app.route('/logout')
def logout():
    session.clear()
    flash('You have been logged out successfully.', 'info')
    return redirect(url_for('login'))

@app.route('/dashboard')
@login_required
def dashboard():
    user = get_user_data()
    weather = get_weather_data()
    
    db = get_db()
    
    # Get dashboard stats
    total_incidents = db.execute('SELECT COUNT(*) FROM incidents').fetchone()[0]
    active_incidents = db.execute('SELECT COUNT(*) FROM incidents WHERE status = ?', ('active',)).fetchone()[0]
    total_responders = db.execute('SELECT COUNT(*) FROM responders').fetchone()[0]
    available_responders = db.execute('SELECT COUNT(*) FROM responders WHERE status = ?', ('available',)).fetchone()[0]
    
    # Get recent incidents
    recent_incidents = db.execute('SELECT * FROM incidents ORDER BY created_at DESC LIMIT 5').fetchall()
    
    # Get notifications
    notifications = db.execute('SELECT * FROM notifications ORDER BY created_at DESC LIMIT 5').fetchall()
    
    db.close()
    
    return render_template('dashboard.html',
                         user=user,
                         weather=weather,
                         total_incidents=total_incidents,
                         active_incidents=active_incidents,
                         total_responders=total_responders,
                         available_responders=available_responders,
                         recent_incidents=recent_incidents,
                         notifications=notifications)

@app.route('/admin')
@login_required
@role_required(['admin'])
def admin_panel():
    db = get_db()
    
    # Get all users
    users = db.execute('SELECT * FROM users ORDER BY created_at DESC').fetchall()
    
    # Get system stats
    total_users = db.execute('SELECT COUNT(*) FROM users').fetchone()[0]
    total_incidents = db.execute('SELECT COUNT(*) FROM incidents').fetchone()[0]
    total_responders = db.execute('SELECT COUNT(*) FROM responders').fetchone()[0]
    total_equipment = db.execute('SELECT COUNT(*) FROM equipment').fetchone()[0]
    
    db.close()
    
    return render_template('admin.html',
                         users=users,
                         total_users=total_users,
                         total_incidents=total_incidents,
                         total_responders=total_responders,
                         total_equipment=total_equipment)

@app.route('/uploads/<filename>')
def uploaded_file(filename):
    """Serve uploaded files"""
    return send_from_directory(app.config['UPLOAD_FOLDER'], filename)

@app.route('/api/weather')
def weather():
    """Get weather data for Tampa"""
    weather_data = get_weather_data()
    if weather_data:
        return jsonify(weather_data)
    return jsonify({'error': 'Weather data unavailable'}), 500

if __name__ == '__main__':
    app.run(debug=True, port=3002) 