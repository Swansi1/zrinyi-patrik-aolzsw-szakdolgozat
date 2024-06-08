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
    