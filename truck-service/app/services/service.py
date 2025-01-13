import logging
from datetime import *
from ..models.truck_model import TruckEntity
from ..models.model import Connect


class TableOperations:
    def __init__(self):
        self.connect = Connect()

    def create_entity(self, entity: TruckEntity):
        with self.connect.get_connection() as conn:
            with conn.cursor() as cursor:
                try:
                    cursor.execute("""
                        INSERT INTO trucks (plate_number, name, description, note) 
                        VALUES (%s, %s, %s, %s);
                    """, (entity.plate_number, entity.name, entity.description, entity.note))
                    conn.commit()
                    return True
                except Exception as e:
                    logging.error(f"Error creating truck: {e}")
                    conn.rollback()
                    raise

    def list_all_entities(self):
        with self.connect.get_connection() as conn:
            with conn.cursor() as cursor:
                try:
                    cursor.execute("SELECT * FROM trucks;")
                    return cursor.fetchall()
                except Exception as e:
                    logging.error(f"Error returning all trucks: {e}")
                    raise

    def return_one_entity(self, plate_number: str):
        with self.connect.get_connection() as conn:
            with conn.cursor() as cursor:
                try:
                    cursor.execute("SELECT * FROM trucks WHERE plate_number = %s;", (plate_number,))
                    row = cursor.fetchone()
                    if row:
                        return {
                            "plate_number": row[0],
                            "name": row[1],
                            "description": row[2],
                            "note": row[3]
                        }
                    return None
                except Exception as e:
                    logging.error(f"Error returning truck by ID: {e}")
                    raise

    def update_entity(self, entity: TruckEntity):
        with self.connect.get_connection() as conn:
            with conn.cursor() as cursor:
                try:
                    cursor.execute("""
                        UPDATE trucks 
                        SET name = %s, description = %s, note = %s 
                        WHERE plate_number = %s;
                    """, (entity.name, entity.description, entity.note, entity.plate_number))
                    conn.commit()
                    return True
                except Exception as e:
                    logging.error(f"Error updating truck: {e}")
                    conn.rollback()
                    raise

    def delete_entity(self, plate_number: str):
        with self.connect.get_connection() as conn:
            with conn.cursor() as cursor:
                try:
                    cursor.execute("DELETE FROM trucks WHERE plate_number = %s;", (plate_number,))
                    conn.commit()
                    return True
                except Exception as e:
                    logging.error(f"Error deleting truck: {e}")
                    conn.rollback()
                    raise

    def get_availability(self, get_date):
        with self.connect.get_connection() as conn:
            with conn.cursor() as cursor:
                try:
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
                    return cursor.fetchall()
                except Exception as e:
                    logging.error(f"Error updating availability: {e}")
                    raise

