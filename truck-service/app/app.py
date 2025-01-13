from flask import Flask, request, jsonify
from .services.service import TableOperations
from .controllers.controller import create_truck, return_all_truck, return_truck, update_truck, delete_truck, get_available_trucks_date
from .models.model import Connect

def create_app():
    # Initialize Flask app
    app = Flask(__name__)

    # Initialize the database
    def initialize_database():
        connect = Connect()
        connect.create_table()
        connect.create_table_truck_sched()

    initialize_database()

    # Create a new truck
    @app.route('/trucks', methods=['POST'])
    def create_truck_route():
        return create_truck(request)

    # Return all trucks
    @app.route('/trucks', methods=['GET'])
    def return_all_truck_route():
        return return_all_truck()

    # Return one truck by ID
    @app.route('/trucks/<string:id>', methods=['GET'])
    def return_truck_route(id):
        return return_truck(id)

    # Update a truck
    @app.route('/trucks/<string:id>', methods=['PUT'])
    def update_truck_route(id):
        return update_truck(id, request)

    # Delete a truck
    @app.route('/trucks/<string:id>', methods=['DELETE'])
    def delete_truck_route(id):
        return delete_truck(id)

    # Filter available trucks based on date
    @app.route('/trucks/available/<string:date>', methods=['GET'])
    def get_available_trucks_date_route(date):
        return get_available_trucks_date(date)
    
    

    return app


#if __name__ == '__main__':
#    create_app().run(debug=False, port=3006)
