import psycopg
#from get_conn import get_connection_uri
import os
import urllib.parse
import azure.functions as func
import logging
from datetime import *



os.environ['DBHOST'] = "db"
os.environ['DBNAME'] = "truck"
os.environ['DBUSER'] = "postgres"
os.environ['DBPASS'] = "postgres"
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

    def create_table(self):
        try:
            conn = Connect().get_connection()
            cursor = conn.cursor()
            cursor.execute("""
                CREATE TABLE IF NOT EXISTS trucks (
                    plate_number VARCHAR(255) PRIMARY KEY,
                    name VARCHAR(255) NOT NULL,
                    description TEXT,
                    note TEXT
                );
                """)

            conn.commit()
            cursor.close()
            conn.close()
            return True
            
        except Exception as e:
            logging.error(f"Error creating trucks table or it already exists: {e}")
            


    #second table for truck availability
    def create_table_truck_sched(self):
        try:
            conn = Connect().get_connection()
            cursor = conn.cursor()
            cursor.execute("""
                CREATE TABLE IF NOT EXISTS TruckAvailability (
                    id bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    plate_number VARCHAR(255),
                    BusyDate DATE NOT NULL,
                    FOREIGN KEY (plate_number) REFERENCES trucks(plate_number)
                );
                """)

            conn.commit()
            cursor.close()
            conn.close()
            return True
            
        except Exception as e:
            logging.error(f"Error creating trucks table or it already exists: {e}") 



    
        