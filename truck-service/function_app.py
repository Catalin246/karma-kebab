import azure.functions as func
from controllers import create_truck, return_all_truck, return_truck, update_truck, delete_truck, get_available_trucks_date
from models import TruckEntity, TableOperations


app = func.FunctionApp(http_auth_level=func.AuthLevel.ANONYMOUS)

#create new truck
app.route(route="trucks", methods=["POST"])(create_truck)

#return all trucks
app.route(route="trucks", methods=["GET"])(return_all_truck)

#return one truck by ID
app.route(route="trucks/{id}", methods=["GET"])(return_truck)    

#update truck
app.route(route="trucks/{id}", methods=["PUT"])(update_truck)

#delete truck
app.route(route="trucks/{id}", methods=["DELETE"])(delete_truck)

#filter available trucks based on date
app.route(route="trucks/available/{date}", methods=["GET"])(get_available_trucks_date)

#port 3006