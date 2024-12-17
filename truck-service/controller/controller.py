
import azure.functions as func
import logging
import json
from enum import Enum
from datetime import *
from model.truck_model import TruckEntity

from service.db_operations import TableOperations


#create new truck
def create_truck(req: func.HttpRequest) -> func.HttpResponse:
    logging.info('Python HTTP trigger function processed a request to create a new truck')
    
    try:

        req_body= req.get_json()
        plate_number = req_body.get("plate_number")
        name = req_body.get("name")
        available = req_body.get("available", True)
        description = req_body.get("description")
        note = req_body.get("note")

        #if status not in {member.value for member in Status}:
        #    return func.HttpResponse("Invalid status.", status_code=400)

        if not plate_number or not name:
            return func.HttpResponse("Missing required fields", status_code=400)
        
        truck = TruckEntity(plate_number, name, description, note)

        TableOperations().create_entity(truck)
        return func.HttpResponse(
                json.dumps({
                    "plate_number": plate_number,
                    "name": name,
                    "description": description,
                    "note": note
                }),
                status_code=201,
                mimetype="application/json"
            )
    except Exception as e:
        logging.error(f"Error creating the truck: {e}")
        return func.HttpResponse("Failed to create the truck", status_code=500)


#return all trucks
def return_all_truck(req: func.HttpRequest) -> func.HttpResponse:
    logging.info('Python HTTP trigger function processed a request to list all trucks')

    try:
        trucks= TableOperations().list_all_entities()
        return func.HttpResponse(json.dumps(trucks), status_code=200, mimetype="application/json")
    except Exception as e:
        logging.error(f"Error returning all the trucks: {e}")
        return func.HttpResponse("Failed to return trucks", status_code=500)


#return one truck by ID
def return_truck(req: func.HttpRequest) -> func.HttpResponse:
    logging.info('Python HTTP trigger function processed a request to list a truck searched by plate number')

    truck_id = req.route_params.get("id")
    if not truck_id:
        return func.HttpResponse("Truck ID missing", status_code=400)
    
    try:
        truck= TableOperations().return_one_entity(truck_id)
        if not truck:
            return func.HttpResponse(f"Truck with ID {truck_id} not found", status_code=404)
        return func.HttpResponse(json.dumps(truck), status_code=200, mimetype="application/json")
    except Exception as e:
        logging.error(f"Error returning truck with ID {truck_id}: {e}")
        return func.HttpResponse("Failed to return truck.", status_code=500)
    

#update truck
def update_truck(req: func.HttpRequest) -> func.HttpResponse:
    logging.info('Python HTTP trigger function processed a request to update a truck')

    truck_id = req.route_params.get("id")
    if not truck_id:
        return func.HttpResponse("Truck ID missing", status_code=400)
    
    try:
        truck = TableOperations().return_one_entity(truck_id)
        if not truck:
            return func.HttpResponse(f"Truck with ID {truck_id} not found", status_code=404)
        
        req_body = req.get_json()
        #new_status =req_body.get("status")
        #if new_status and new_status not in Status.__members__:
        #if new_status and new_status not in [status.value for status in Status]:
        #    return func.HttpResponse("Invalid status.", status_code=400)
    
        updatedtruck = TruckEntity(
            plate_number=truck_id,
            name=req_body.get("name", truck["name"]),
            description=req_body.get("description", truck["description"]),
            note=req_body.get("note", truck["note"])
        )

        
        TableOperations().update_entity(updatedtruck)
        return func.HttpResponse("Truck updated successfully.", status_code=200)
    
    except Exception as e:
        logging.error(f"Error updating truck with ID {truck_id}: {e}")
        return func.HttpResponse("Failed to update truck.", status_code=500)
    

#delete truck
def delete_truck(req: func.HttpRequest) -> func.HttpResponse:
    logging.info('Python HTTP trigger function processed a request to delete truck by id.')

    truck_id = req.route_params.get("id")
    if not truck_id:
        return func.HttpResponse("Truck ID missing", status_code=400)
    
    try:
        truck = TableOperations().return_one_entity(truck_id)
        if not truck:
            return func.HttpResponse(f"Truck with ID {truck_id} not found", status_code=404)

        TableOperations().delete_entity(truck_id)
        return func.HttpResponse(f"Truck with ID {truck_id} successfully deleted.", status_code=200)
    except Exception as e:
        logging.error(f"Error deleting truck with ID {truck_id}: {e}")
        return func.HttpResponse("Failed to delete truck.", status_code=500)
    

#filter available trucksa based on date
def get_available_trucks_date(req: func.HttpRequest) -> func.HttpResponse:
    logging.info('Python HTTP trigger function processed a request to list available trucks')
    truck_avalable_date= req.route_params.get("date")
    search_date = datetime.strptime(truck_avalable_date, '%Y-%m-%d').date()


    
    if not truck_avalable_date:
        return func.HttpResponse("Availability date missing", status_code=400)
    try:
        available = TableOperations().get_availability(search_date)
        return func.HttpResponse(json.dumps(available), status_code=200, mimetype="application/json")
    except Exception as e:
        logging.error(f"Error filtering available  trucks: {e}")
        return func.HttpResponse("Failed to return trucks", status_code=500)
