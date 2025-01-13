from flask import Flask, request, jsonify
from ..models.truck_model import TruckEntity
from ..services.service import *
from datetime import datetime
import logging

app = Flask(__name__)

@app.route('/trucks', methods=['POST'])
def create_truck(request):
    logging.info('Processing request to create a new truck')
    try:
        req_body = request.json
        plate_number = req_body.get("plate_number")
        name = req_body.get("name")
        description = req_body.get("description")
        note = req_body.get("note")

        if not plate_number or not name:
            return jsonify({"error": "Missing required fields"}), 400
        
        truck = TruckEntity(plate_number, name, description, note)
        TableOperations().create_entity(truck)
        return jsonify({
            "plate_number": plate_number,
            "name": name,
            "description": description,
            "note": note
        }), 201
    except Exception as e:
        logging.error(f"Error creating the truck: {e}")
        return jsonify({"error": "Failed to create the truck"}), 500

@app.route('/trucks', methods=['GET'])
def return_all_truck():
    logging.info('Processing request to list all trucks')
    try:
        trucks = TableOperations().list_all_entities()
        return jsonify(trucks), 200
    except Exception as e:
        logging.error(f"Error returning all the trucks: {e}")
        return jsonify({"error": "Failed to return trucks"}), 500

@app.route('/trucks/<string:truck_id>', methods=['GET'])
def return_truck(truck_id):
    logging.info('Processing request to list a truck searched by plate number')
    try:
        truck = TableOperations().return_one_entity(truck_id)
        if not truck:
            return jsonify({"error": f"Truck with ID {truck_id} not found"}), 404
        return jsonify(truck), 200
    except Exception as e:
        logging.error(f"Error returning truck with ID {truck_id}: {e}")
        return jsonify({"error": "Failed to return truck"}), 500

@app.route('/trucks/<string:truck_id>', methods=['PUT'])
def update_truck(truck_id, request):
    logging.info('Processing request to update a truck')
    try:
        truck = TableOperations().return_one_entity(truck_id)
        if not truck:
            return jsonify({"error": f"Truck with ID {truck_id} not found"}), 404
        
        req_body = request.json
        updated_truck = TruckEntity(
            plate_number=truck_id,
            name=req_body.get("name", truck["name"]),
            description=req_body.get("description", truck["description"]),
            note=req_body.get("note", truck["note"])
        )
        
        TableOperations().update_entity(updated_truck)
        return jsonify({"message": "Truck updated successfully"}), 200
    except Exception as e:
        logging.error(f"Error updating truck with ID {truck_id}: {e}")
        return jsonify({"error": "Failed to update truck"}), 500

@app.route('/trucks/<string:truck_id>', methods=['DELETE'])
def delete_truck(truck_id):
    logging.info('Processing request to delete truck by id')
    try:
        truck = TableOperations().return_one_entity(truck_id)
        if not truck:
            return jsonify({"error": f"Truck with ID {truck_id} not found"}), 404
        TableOperations().delete_entity(truck_id)
        return jsonify({"message": f"Truck with ID {truck_id} successfully deleted"}), 200
    except Exception as e:
        logging.error(f"Error deleting truck with ID {truck_id}: {e}")
        return jsonify({"error": "Failed to delete truck"}), 500

@app.route('/trucks/available/<string:date>', methods=['GET'])
def get_available_trucks_date(date):
    logging.info('Processing request to list available trucks')
    try:
        search_date = datetime.strptime(date, '%Y-%m-%d').date()
        available = TableOperations().get_availability(search_date)
        return jsonify(available), 200
    except Exception as e:
        logging.error(f"Error filtering available trucks: {e}")
        return jsonify({"error": "Failed to return trucks"}), 500

if __name__ == '__main__':
    app.run(debug=True)
