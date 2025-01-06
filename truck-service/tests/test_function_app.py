import pytest
#from function_app import TableOperations, TruckEntity
from function_app import *
from models import TableOperations, TruckEntity
from controllers import create_truck, return_all_truck, return_truck, update_truck, delete_truck, get_available_trucks_date
from unittest.mock import MagicMock, patch
import azure.functions as func
import json


@pytest.fixture  
def truck1():  
  
    truck1 = TruckEntity(  
        plate_number = "123-ABC", 
        name = "King Karma", 
        description = "Can carry 120 kg kebab meat",
        note= "all good"
    )  
    yield truck1  

@pytest.fixture  
def truck2():  
  
    truck2 = TruckEntity(  
        plate_number = "456-DEF", 
        name = "Queen Karma", 
        description = "Can carry 220 kg kebab meat",
        note= "fix front right lights"
    )  
    yield truck2

@pytest.fixture  
def mock_db_connection():
    mock_conn = MagicMock()
    mock_cursor = MagicMock()
    mock_conn.__enter__.return_value = mock_conn
    mock_conn.cursor.return_value = mock_cursor

    return mock_conn

@patch('function_app.TableOperations.get_connection')
def test_create_entity(mock_get_connection):
    mock_get_connection.return_value = "mocked_connection"

    db = TableOperations()
    #create= db.create_entity(truck2)
    db.create_entity = MagicMock(return_value={"plate_number": "123-ABC", "name": "King Karma", "description": "Can carry 120 kg kebab meat", "note": "all good"})
    create = db.create_entity(mock_get_connection)

    assert create["plate_number"] == "123-ABC"
    assert create["name"] == "King Karma"


@patch('function_app.TableOperations.get_connection')
def test_list_all_entities(mock_get_connection):
    mock_cursor = MagicMock()
    mock_cursor.fetchall.return_value = [
        {"plate_number": "123-ABC", "name": "King Karma", "description": "Can carry 120 kg kebab meat", "note": "all good"},
        {"plate_number": "456-DEF", "name": "Queen Karma", "description": "Can carry 220 kg kebab meat", "note": "fix front right lights"}
    ]
    mock_connection = MagicMock()
    mock_connection.cursor.return_value = mock_cursor

    mock_get_connection.return_value = mock_connection

    db = TableOperations()

    all_entities = db.list_all_entities()
    assert len(all_entities) == 2
    assert all_entities[0]["plate_number"] == "123-ABC"
    assert all_entities[1]["plate_number"] == "456-DEF"

@patch('function_app.TableOperations.get_connection')
def test_return_one_entity(mock_get_connection):
    mock_cursor = MagicMock()
    mock_cursor.fetchone.return_value = ("123-ABC", "King Karma", "available", "Can carry 120 kg kebab meat", "all good")
    mock_connection = MagicMock()
    mock_connection.cursor.return_value = mock_cursor

    mock_get_connection.return_value = mock_connection


    db = TableOperations()
    res = db.return_one_entity("123-ABC")

    assert res["plate_number"] == "123-ABC"
    assert res["name"] == "King Karma"

@patch('function_app.TableOperations.get_connection')
def test_update_entity(mock_get_connection):
    mock_connection = MagicMock()
    mock_cursor = MagicMock()
    mock_connection.cursor.return_value = mock_cursor

    mock_get_connection.return_value = mock_connection

    db = TableOperations()
    db.create_entity = MagicMock(return_value={"plate_number": "123-ABC", "name": "King Karma", "description": "Can carry 120 kg kebab meat", "note": "all good"})
    create = db.create_entity(mock_get_connection)

    mock_req = MagicMock(spec=func.HttpRequest)
    mock_req.route_params = {"id": "123-ABC"}
    mock_req.get_json.return_value = {
        "name": "King Karma II",
        "description": "Can carry 120 kg kebab meat",
        "note": "updated"
    }


    res = update_truck(mock_req)

    assert res.status_code == 200
    assert "updated" in res.get_body().decode() 



@patch('function_app.TableOperations.get_connection')
def test_delete_entity(mock_get_connection):
    mock_cursor = MagicMock()
    mock_cursor.fetchall.return_value = [
        {"plate_number": "123-ABC", "name": "King Karma", "description": "Can carry 120 kg kebab meat", "note": "all good"},
        {"plate_number": "456-DEF", "name": "Queen Karma", "description": "Can carry 220 kg kebab meat", "note": "fix front right lights"}
    ]
    mock_connection = MagicMock()
    mock_connection.cursor.return_value = mock_cursor

    mock_get_connection.return_value = mock_connection

    mock_req = MagicMock(spec=func.HttpRequest)
    mock_req.route_params = {"id": "123-ABC"}

    res = delete_truck(mock_req)

    assert res.status_code == 200

####################

@pytest.fixture
def mock_table_operations():
    with patch('function_app.TableOperations') as mock_operations:
        yield mock_operations

def test_create_truck_success(mock_table_operations):
    mock_create = mock_table_operations.return_value.create_entity
    mock_create.return_value = True

    req = func.HttpRequest(
        method='POST',
        body=json.dumps({
            "plate_number": "123-ABC",
            "name": "King Karma",
            "description": "Can carry 120 kg kebab meat",
            "note": "all good"
        }).encode('utf-8'),
        url='/api/trucks'
    )

    resp = create_truck(req)
    assert resp.status_code == 201
    assert json.loads(resp.get_body())["plate_number"] == "123-ABC"
    #mock_create.assert_called_once()

def test_return_all_trucks_success(mock_table_operations):
    mock_list = mock_table_operations.return_value.list_all_entities
    mock_list.return_value = [
        {"plate_number": "123-ABC", "name": "King Karma", "description": "Can carry 120 kg kebab meat", "note": "all good"},
        {"plate_number": "456-DEF", "name": "Queen Karma", "description": "Can carry 220 kg kebab meat", "note": "fix front right lights"}
    ]

    req = func.HttpRequest(method='GET', url='/api/trucks', body=b'')
    resp = return_all_truck(req)

    assert resp.status_code == 200
    #assert len(json.loads(resp.get_body())) == 2
    #mock_list.assert_called_once()

def test_return_truck_success(mock_table_operations):
    mock_return = mock_table_operations.return_value.return_one_entity
    mock_return.return_value = {"plate_number": "123-ABC", "name": "King Karma", "description": "Can carry 120 kg kebab meat", "note": "all good"}

    req = func.HttpRequest(
        method='GET',
        url='/api/trucks/123-ABC',
        body=b'',
        route_params={"id": "123-ABC"}
    )
    resp = return_truck(req)

    assert resp.status_code == 200
    assert json.loads(resp.get_body())["plate_number"] == "123-ABC"
    #mock_return.assert_called_once_with("123-ABC")

@patch('function_app.TableOperations.get_connection')
def test_update_truck_success(mock_table_operations):
    mock_return = mock_table_operations.return_value.return_one_entity
    mock_return.return_value = {"plate_number": "123-ABC", "name": "King Karma", "description": "Can carry 120 kg kebab meat", "note": "all good"}
    mock_update = mock_table_operations.return_value.update_entity
    mock_update.return_value = True

    req = func.HttpRequest(
        method='PUT',
        url='/api/trucks/123-ABC',
        body=json.dumps({
            "name": "King Karma II",
            "description": "Updated description",
            "note": "Updated note"
        }).encode('utf-8'),
        #url='/api/trucks/123-ABC'
        route_params={"id": "123-ABC"}
    )

    resp = update_truck(req)

    assert resp.status_code == 200


@patch('function_app.TableOperations.get_connection')
def test_delete_truck_success(mock_table_operations):
    mock_return = mock_table_operations.return_value.return_one_entity
    mock_return.return_value = {"plate_number": "123-ABC", "name": "King Karma", "description": "Can carry 120 kg kebab meat", "note": "all good"}
    mock_delete = mock_table_operations.return_value.delete_entity
    mock_delete.return_value = True

    req = func.HttpRequest(
        method='DELETE',
        url='/api/trucks/123-ABC',
        body=b'',
        route_params={"id": "124-ABC"}
    )
    
    resp = delete_truck(req)

    assert resp.status_code == 200

