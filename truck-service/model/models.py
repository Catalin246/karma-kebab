import psycopg
#from get_conn import get_connection_uri
import os
import urllib.parse
import azure.functions as func
import logging
from datetime import *



os.environ['DBHOST'] = "localhost"
os.environ['DBNAME'] = "truck"
os.environ['DBUSER'] = "postgres"
os.environ['DBPASS'] = "Cica08"
os.environ['SSLMODE'] = "disable"

class Connect:


    def __init__(self):
        #load_dotenv(find_dotenv())
        self.dbhost = os.environ['DBHOST']
        self.dbname = os.environ['DBNAME']
        self.dbuser = urllib.parse.quote(os.environ['DBUSER'])
        self.dbpass = os.environ['DBPASS']
        self.sslmode = os.environ['SSLMODE']


        #self.table_name = "trucks"

    def get_connection(self):
        try:
            db_uri = f"postgresql://{self.dbuser}:{self.dbpass}@{self.dbhost}/{self.dbname}?sslmode={self.sslmode}"
            return psycopg.connect(db_uri)
        except Exception as e:
            logging.error(f"Error connecting to PostgreSQL: {e}")
            raise

    
        