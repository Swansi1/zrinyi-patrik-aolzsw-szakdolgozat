import mysql.connector

class Database:
    def __init__(self, host, user, pwd, database, port) -> None:
        self.mydb = mysql.connector.connect(
            host=host,
            user=user,
            password=pwd,
            database=database,
            port=port,
            collation='utf8mb4_general_ci'
        )

    def update_journal(self, date, user_id, id):
        mycursor = self.mydb.cursor()
        sql = "UPDATE journals SET created_on = %s, user_id = %s WHERE id = %s"
        val = (date, user_id, id)

        mycursor.execute(sql, val)
        self.mydb.commit()

    def get_redmine_settings(self):
        try:
            mycursor = self.mydb.cursor(dictionary=True)
            sql = "SELECT * FROM settings"

            mycursor.execute(sql)
            settings = mycursor.fetchall()

            return settings
        except mysql.connector.Error as err:
            return {'error': str(err)}
