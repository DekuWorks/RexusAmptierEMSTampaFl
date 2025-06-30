from flask import Flask
from flask_cors import CORS
from api.endpoints import api_bp

app = Flask(__name__)
CORS(app)

# Register API blueprint
app.register_blueprint(api_bp, url_prefix='/api')

@app.route('/')
def home():
    return {'message': 'EMS Tampa-FL Amptier Backend Running!'}

if __name__ == '__main__':
    app.run(debug=True) 