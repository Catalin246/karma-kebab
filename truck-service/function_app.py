
import os
import json

# Function to load environment variables from local.settings.json
def load_local_settings():
    settings_file = "local.settings.json"
    
    with open(settings_file, "r") as file:
        settings = json.load(file)
    
    if "Values" in settings:
        values = settings["Values"]
        for key, value in values.items():
            os.environ[key] = value
            print(f"{key} loaded successfully: {value}")
    else:
        print("No 'Values' found in local.settings.json")

# Load environment variables before any other logic
load_local_settings()


import azure.functions as func
import logging
import sys
import json
from enum import Enum
import os
from azure.data.tables import TableClient, TableServiceClient, TableEntity
from dotenv import find_dotenv, load_dotenv
from typing_extensions import TypedDict



app = func.FunctionApp(http_auth_level=func.AuthLevel.ANONYMOUS)

class Status(Enum):
    AVAILABLE= "available"
    ON_DUTY = "on_duty"
    MAINTENANCE= "maintenance"

class TruckEntity(TypedDict, total=False):
    PartitionKey: str
    RowKey: str
    plate_number: str
    name: str
    status: str
    description: str
    note: str

from azure.data.tables import TableClient
from azure.core.exceptions import ResourceExistsError, HttpResponseError

class TableOperations(object):
    def __init__(self):
        #load_dotenv(find_dotenv())
        print("Environment variables loaded:")
        print(os.environ) 
        self.access_key = os.environ["TABLES_PRIMARY_STORAGE_ACCOUNT_KEY"]
        self.endpoint_suffix = os.environ.get("TABLES_STORAGE_ENDPOINT_SUFFIX", "table.core.windows.net")
        self.account_name = os.environ["TABLES_STORAGE_ACCOUNT_NAME"]

        if not self.access_key:
            raise ValueError("Missing TABLES_PRIMARY_STORAGE_ACCOUNT_KEY")
        #self.endpoint = f"{self.account_name}.table.{self.endpoint_suffix}"
        #self.connection_string = f"DefaultEndpointsProtocol=https;AccountName={self.account_name};AccountKey={self.access_key};EndpointSuffix={self.endpoint_suffix}"
        #self.table_name = "trucks"

        if "localhost" in self.endpoint_suffix:
            self.endpoint = f"http://{self.endpoint_suffix}/{self.account_name}"
        else:
            self.endpoint = f"https://{self.account_name}.table.{self.endpoint_suffix}"

        self.connection_string = (
            f"DefaultEndpointsProtocol=http;"
            f"AccountName={self.account_name};"
            f"AccountKey={self.access_key};"
            f"TableEndpoint={self.endpoint};"
        )
        self.table_name = "trucks"

        """self.entity: TruckEntity = {
            "PartitionKey": "status",
            "RowKey": "plate_number",
            "plate_number": "1-ABC-23",
            "name": "Karma King",
            "status": "available",
            "description": "can carry 120kg kebab",
            "note": "fix front right lamp"
        }
        """
    def create_entity(self, entity: TruckEntity):

        service_client = TableServiceClient.from_connection_string(self.connection_string)
        
        try:
            tables = list(service_client.list_tables())
            if self.table_name not in tables:
                service_client.create_table(self.table_name)
            
            with TableClient.from_connection_string(self.connection_string, self.table_name) as table_client:
                resp = table_client.create_entity(entity=entity)
                
                if resp is None:
                    logging.error("Failed to create entity: Response was None.")
                    return None
                return resp
        except ResourceExistsError:
            logging.warning("Entity already exists")
            return None
        except HttpResponseError as e:
            logging.error(f"HTTP Response Error: {e}")
            return None

    def list_all_entities(self):
        from azure.data.tables import TableClient

        with TableClient.from_connection_string(self.connection_string, self.table_name) as table_client:
            return list(table_client.list_entities())

    def return_one_entity(self, partition_key: str, row_key: str):
        from azure.data.tables import TableClient

        with TableClient.from_connection_string(self.connection_string, self.table_name) as table_client:
            return table_client.get_entity(partition_key=partition_key, row_key=row_key)

    def update_entity(self, entity: TruckEntity):
        from azure.data.tables import TableClient
        from azure.data.tables import UpdateMode

        with TableClient.from_connection_string(self.connection_string, self.table_name) as table_client:
            table_client.update_entity(entity=entity, mode=UpdateMode.REPLACE)

    def delete_entity(self, partition_key: str, row_key: str):
        from azure.data.tables import TableClient

        with TableClient.from_connection_string(self.connection_string, self.table_name) as table_client:
            table_client.delete_entity(partition_key=partition_key, row_key=row_key)


#create new truck
@app.route(route="trucks", methods=["POST"])
def create_truck(req: func.HttpRequest) -> func.HttpResponse:
    logging.info('Python HTTP trigger function processed a request to create a new truck')
    try:

        req_body= req.get_json()
        status =req_body.get("status")
        if status not in {member.value for member in Status}:

            return func.HttpResponse("Invalid status.", status_code=400)
        #id = plate_number

        truck = {
            "PartitionKey": req_body.get("status", "available"),
            "RowKey": req_body.get("plate_number"),
            "plate_number": req_body.get("plate_number"),
            "name": req_body.get("name"),
            "status": status,
            "description": req_body.get("description"),
            "note": req_body.get("note"),
        }

        if not all([truck['plate_number'], truck['name']]):
            return func.HttpResponse("Missing required fields", status_code=400)
        TableOperations().create_entity(truck)
        #return func.HttpResponse(f"Truck created.", status_code=201)
        return func.HttpResponse(
                json.dumps(truck),
                status_code=201,
                mimetype="application/json"
            )
    except Exception as e:
        logging.error(f"Error creating the truck: {e}")
        return func.HttpResponse("Failed to create the truck", status_code=500)


#return all trucks
@app.route(route="trucks", methods=["GET"])
def return_all_truck(req: func.HttpRequest) -> func.HttpResponse:
    logging.info('Python HTTP trigger function processed a request to list all trucks')

    try:
        trucks= TableOperations().list_all_entities()
        return func.HttpResponse(json.dumps(trucks), status_code=200, mimetype="application/json")
    except Exception as e:
        logging.error(f"Error returning all the trucks: {e}")
        return func.HttpResponse("Failed to return trucks", status_code=500)


#return one truck by ID
@app.route(route="trucks/{id}", methods=["GET"])
def return_truck(req: func.HttpRequest) -> func.HttpResponse:
    logging.info('Python HTTP trigger function processed a request to list a truck searched by plate number')

    #truck_id = req.params["plate_number"]
    truck_id = req.route_params.get("id")
    if not truck_id:
        return func.HttpResponse("Truck ID missing", status_code=400)
    
    try:
        truck= TableOperations().return_one_entity(partition_key="available", row_key=truck_id)
        if not truck:
            return func.HttpResponse(f"Truck with ID {truck_id} not found", status_code=404)
        return func.HttpResponse(json.dumps(truck), status_code=200, mimetype="application/json")
    except Exception as e:
        logging.error(f"Error returning truck with ID {truck_id}: {e}")
        return func.HttpResponse("Failed to return truck.", status_code=500)
    

#update truck
@app.route(route="trucks/{id}", methods=["PUT"])
def update_truck(req: func.HttpRequest) -> func.HttpResponse:
    logging.info('Python HTTP trigger function processed a request to update a truck')

    truck_id = req.route_params.get("id")
    if not truck_id:
        return func.HttpResponse("Truck ID missing", status_code=400)
    
    try:
        truck = TableOperations().return_one_entity(partition_key="available", row_key=truck_id)
        if not truck:
            return func.HttpResponse(f"Truck with ID {truck_id} not found", status_code=404)
        
        req_body = req.get_json()
        new_status =req_body.get("status")
        if new_status and new_status not in Status.__members__:
            return func.HttpResponse("Invalid status.", status_code=400)
    
        truck.update({
            "name": req_body.get("name", truck["name"]),
            "status": new_status or truck["status"],
            "description": req_body.get("description", truck["description"]),
            "note": req_body.get("note", truck["note"]),
        })
        TableOperations().update_entity(truck)
        return func.HttpResponse("Truck updated successfully.", status_code=200)
    
    except Exception as e:
        logging.error(f"Error updating truck with ID {truck_id}: {e}")
        return func.HttpResponse("Failed to update truck.", status_code=500)
    

#delete truck
@app.route(route="trucks/{id}", methods=["DELETE"])
def delete_truck(req: func.HttpRequest) -> func.HttpResponse:
    logging.info('Python HTTP trigger function processed a request to delete truck by id.')

    #truck_id = req.params["plate_number"]
    truck_id = req.route_params.get("id")
    if not truck_id:
        return func.HttpResponse("Truck ID missing", status_code=400)
    
    try:
        truck = TableOperations().return_one_entity(partition_key="available", row_key=truck_id)
        if not truck:
            return func.HttpResponse(f"Truck with ID {truck_id} not found", status_code=404)

        TableOperations().delete_entity(partition_key="status", row_key=truck_id)
        return func.HttpResponse(f"Truck with ID {truck_id} successfully deleted.", status_code=200)
    except Exception as e:
        logging.error(f"Error deleting truck with ID {truck_id}: {e}")
        return func.HttpResponse("Failed to delete truck.", status_code=500)
    