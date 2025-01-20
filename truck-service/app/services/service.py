import json
from flask import Flask, jsonify, request
import os


dfile = os.path.join(os.path.dirname(__file__), '..', '..', 'data', 'trucks.json')

def load_json():
    with open(dfile,'r') as f:
        data = json.load(f)
    return data

def save_json(data):
    with open(dfile,'w') as f:
        json.dump(data, f, indent=4)
