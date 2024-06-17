import mysql.connector

class Database:
    def __init__(self, host, user, pwd, database) -> None:
        self.mydb = mysql.connector.connect(
            host=host,
            user=user,
            password=pwd,
            database=database
        )

    def update_journal(self, date, user_id, id):
        mycursor = self.mydb.cursor()
        sql = "UPDATE journals SET created_on = %s, user_id = %s WHERE id = %s"
        val = (date, user_id, id)

        mycursor.execute(sql, val)
        self.mydb.commit()

    def get_workflows(self, tracker_ids):
        try:
            mycursor = self.mydb.cursor(dictionary=True)

            format_strings = ','.join(['%s'] * len(tracker_ids))
            sql = f"SELECT * FROM workflows WHERE tracker_id IN ({format_strings})"

            mycursor.execute(sql, tuple(tracker_ids))
            workflows = mycursor.fetchall()

            return workflows
        except mysql.connector.Error as err:
            return {'error': str(err)}

    def insert_workflow(self, workflow, new_tracker_id, old_status_id, new_status_id):
        try:
            mycursor = self.mydb.cursor()
            sql = "INSERT INTO workflows (tracker_id, old_status_id, new_status_id, role_id, type) VALUES (%s, %s, %s, %s, %s)"
            val = (new_tracker_id, old_status_id, new_status_id, workflow['role_id'], workflow['type'])

            mycursor.execute(sql, val)
            self.mydb.commit()
            return mycursor.lastrowid
        except Exception as e:
            print(f"Error: {e}")
