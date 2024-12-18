

#from get_conn import get_connection_uri
import azure.functions as func
import logging
from datetime import *
from model.truck_model import TruckEntity
from model.models import Connect

class TableOperations(object):


    def create_entity(self, entity: TruckEntity):
        conn= None
        try:
            conn = Connect().get_connection()
            cursor = conn.cursor()

            cursor.execute("""INSERT INTO trucks (plate_number, name, description, note) VALUES (%s, %s, %s, %s);""", 
                        (entity.plate_number, entity.name, entity.description, entity.note))
                    
            conn.commit()
            cursor.close()
            conn.close()
            return True

        except Exception as e:
            logging.error(f"Error creating truck: {e}")
            if conn:
                conn.rollback()
            return False


    def list_all_entities(self):

        try:
            conn = Connect().get_connection()
            cursor = conn.cursor()
            cursor.execute("SELECT * FROM trucks;")
            rows = cursor.fetchall()

            cursor.close()
            conn.close()
            return rows

        except Exception as e:
            logging.error(f"Error returning all trucks: {e}")
            raise
        
            
    def return_one_entity(self, plate_number: str):
        try:
            conn = Connect().get_connection()
            cursor = conn.cursor()
            cursor.execute("SELECT * FROM trucks WHERE plate_number = %s;", (plate_number,))
            row = cursor.fetchone()

            if row:
                    return {
                        "plate_number": row[0],
                        "name": row[1],
                        "description": row[2],
                        "note": row[3]
                    }
            else:
                return None

        except Exception as e:
            logging.error(f"Error returning truck by ID: {e}")
            raise

        finally:
            cursor.close()
            conn.close()


    def update_entity(self, entity: TruckEntity):
        conn= None
        try:
            conn = Connect().get_connection()
            cursor = conn.cursor()
            cursor.execute("UPDATE trucks SET name = %s, description = %s, note = %s WHERE plate_number = %s;", 
                            (entity.name, entity.description, entity.note, entity.plate_number))
            conn.commit()

            cursor.close()
            conn.close()

        except Exception as e:
            logging.error(f"Error updating truck: {e}")
            conn.rollback()
            return False


    def delete_entity(self, plate_number: str):
        conn= None
        try:
            conn = Connect().get_connection()
            cursor = conn.cursor()

            cursor.execute("DELETE FROM trucks WHERE plate_number = %s;", (plate_number,))
            conn.commit()

            cursor.close()
            conn.close()


        except Exception as e:
            logging.error(f"Error deleting truck: {e}")
            conn.rollback()
            return False



    #eliminates the need for bool in truck table - mixing static and non static data is no good
    #where not exists = like left join but the negation
    def get_availability(self, get_date):
        conn= None
        try:
            conn = Connect().get_connection()
            cursor = conn.cursor()
            cursor.execute("""
                    SELECT t.plate_number, t.name
                    FROM trucks t
                    WHERE NOT EXISTS (
                        SELECT 1
                        FROM truck_availability ta
                        WHERE ta.plate_number = t.plate_number 
                        AND ta.busy_date = %s
                    )
                """, (get_date,))
            
                        # delete old ones??


            rows = cursor.fetchall()
            cursor.close()
            conn.close()
            return rows


        except Exception as e:
            logging.error(f"Error updating avaiability: {e}")
            conn.rollback()
            return False