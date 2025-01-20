import pytest
from flask import Flask, request, jsonify
from app.app import create_app
from unittest.mock import MagicMock, patch
import json
from unittest.mock import MagicMock, patch
import os


@pytest.fixture
def client():
    os.environ['CONFIG_TYPE'] = 'config.TestingConfig'
    flask_app = create_app()

    with flask_app.test_client() as client:
        with flask_app.app_context():
            yield client


@pytest.fixture
def mock_trucks():
    return [
        {"plate_number": "ABC123", "name": "Truck 1", "description": "Description 1", "note": "Note 1"},
        {"plate_number": "DEF456", "name": "Truck 2", "description": "Description 2", "note": "Note 2"},
    ]

@patch('app.services.service.load_json')
@patch('app.services.service.save_json')
def test_create_truck(mock_save_json, mock_load_json, client, mock_trucks):
    mock_load_json.return_value = mock_trucks
    new_truck = {
        "plate_number": "test-1",
        "name": "New Truck",
        "description": "New Description",
        "note": "New Note",
    }
    response = client.post('/trucks', json=new_truck)
    
    assert response.status_code == 201
    assert response.json["plate_number"] == "test-1"

@patch('app.services.service.load_json')
def test_create_truck_duplicate(mock_load_json, client, mock_trucks):
    mock_load_json.return_value = mock_trucks
    response = client.post('/trucks', json={
        "plate_number": "test-1",
        "name": "Duplicate Truck",
    })
    
    assert response.status_code == 400
    assert "Truck already exists" in response.json["error"]

@patch('app.services.service.load_json')
def test_return_all_trucks(mock_load_json, client, mock_trucks):
    mock_load_json.return_value = mock_trucks
    response = client.get('/trucks')
    
    assert response.status_code == 200

@patch('app.services.service.load_json')
def test_return_truck(mock_load_json, client, mock_trucks):
    mock_load_json.return_value = mock_trucks
    response = client.get('/trucks/test-1')
    
    assert response.status_code == 200
    assert response.json["name"] == "New Truck"

@patch('app.services.service.load_json')
def test_return_truck_not_found(mock_load_json, client, mock_trucks):
    mock_load_json.return_value = mock_trucks
    response = client.get('/trucks/non-existent')
    
    assert response.status_code == 404
    assert "not found" in response.json["error"].lower()

@patch('app.services.service.load_json')
@patch('app.services.service.save_json')
def test_update_truck(mock_save_json, mock_load_json, client, mock_trucks):
    mock_load_json.return_value = mock_trucks
    updated_data = {
        "name": "Updated Truck",
        "description": "Updated Description",
    }
    response = client.put('/trucks/test-1', json=updated_data)
    
    assert response.status_code == 200
    assert "updated" in response.json["message"]

@patch('app.services.service.load_json')
@patch('app.services.service.save_json')
def test_delete_truck(mock_save_json, mock_load_json, client, mock_trucks):
    mock_load_json.return_value = mock_trucks
    response = client.delete('/trucks/test-1')
    
    assert response.status_code == 200
    assert "deleted" in response.json["message"]
