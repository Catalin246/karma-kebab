import pytest
#from function_app import TableOperations, TruckEntity
from app.app import create_app
from app.controllers.controller import create_truck, return_all_truck, return_truck, update_truck, delete_truck, get_available_trucks_date
from unittest.mock import MagicMock, patch
from app.models.model import Connect
from app.models.truck_model import TruckEntity
from app.services.service import TableOperations

@pytest.fixture
def client():
    """Fixture to provide a test client for the Flask app."""
    app = create_app()  # Call the function to get the app instance
    app.testing = True  # Ensure the app is in testing mode
    with app.test_client() as client:  # Now you can use test_client
        yield client

@pytest.fixture
def truck1():
    return TruckEntity(
        plate_number="123-ABC",
        name="King Karma",
        description="Can carry 120 kg kebab meat",
        note="all good",
    )

@pytest.fixture
def truck2():
    return TruckEntity(
        plate_number="456-DEF",
        name="Queen Karma",
        description="Can carry 220 kg kebab meat",
        note="fix front right lights",
    )

@pytest.fixture
def mock_db_connection():
    mock_conn = MagicMock()
    mock_cursor = MagicMock()
    mock_conn.__enter__.return_value = mock_conn
    mock_conn.cursor.return_value = mock_cursor
    return mock_conn

@patch('app.models.model.Connect.get_connection')
def test_create_truck(mock_get_connection, client):
    mock_get_connection.return_value = MagicMock()

    payload = {
        "plate_number": "123-ABC",
        "name": "King Karma",
        "description": "Can carry 120 kg kebab meat",
        "note": "all good",
    }

    with patch.object(TableOperations, 'create_entity', return_value=payload):
        response = client.post('/trucks', json=payload)

    assert response.status_code == 201
    assert response.json["plate_number"] == "123-ABC"

@patch('app.models.model.Connect.get_connection')
def test_return_all_trucks(mock_get_connection, client):
    mock_get_connection.return_value = MagicMock()
    mock_data = [
        {"plate_number": "123-ABC", "name": "King Karma", "description": "Can carry 120 kg kebab meat", "note": "all good"},
        {"plate_number": "456-DEF", "name": "Queen Karma", "description": "Can carry 220 kg kebab meat", "note": "fix front right lights"},
    ]

    with patch.object(TableOperations, 'list_all_entities', return_value=mock_data):
        response = client.get('/trucks')

    assert response.status_code == 200
    assert len(response.json) == 2
    assert response.json[0]["plate_number"] == "123-ABC"

@patch('app.models.model.Connect.get_connection')
def test_return_truck(mock_get_connection, client):
    mock_get_connection.return_value = MagicMock()
    mock_data = {"plate_number": "123-ABC", "name": "King Karma", "description": "Can carry 120 kg kebab meat", "note": "all good"}

    with patch.object(TableOperations, 'return_one_entity', return_value=mock_data):
        response = client.get('/trucks/123-ABC')

    assert response.status_code == 200
    assert response.json["plate_number"] == "123-ABC"

@patch('app.models.model.Connect.get_connection')
def test_update_truck(mock_get_connection, client):
    mock_get_connection.return_value = MagicMock()
    mock_data = {"plate_number": "123-ABC", "name": "King Karma", "description": "Can carry 120 kg kebab meat", "note": "all good"}
    updated_data = {"name": "King Karma II", "description": "Updated description", "note": "Updated note"}

    with patch.object(TableOperations, 'return_one_entity', return_value=mock_data):
        with patch.object(TableOperations, 'update_entity', return_value=True):
            response = client.put('/trucks/123-ABC', json=updated_data)

    assert response.status_code == 200
    assert "updated" in response.json["message"]

@patch('app.models.model.Connect.get_connection')
def test_delete_truck(mock_get_connection, client):
    mock_get_connection.return_value = MagicMock()
    mock_data = {"plate_number": "123-ABC", "name": "King Karma", "description": "Can carry 120 kg kebab meat", "note": "all good"}

    with patch.object(TableOperations, 'return_one_entity', return_value=mock_data):
        with patch.object(TableOperations, 'delete_entity', return_value=True):
            response = client.delete('/trucks/123-ABC')

    assert response.status_code == 200
    assert "deleted" in response.json["message"]
