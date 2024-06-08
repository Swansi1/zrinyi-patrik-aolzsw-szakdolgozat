from redminelib import Redmine

class Server:
    
    def __init__(self, ip, uname, pwd) -> None:
        self._ip = ip
        self._uname = uname,
        self._pwd = pwd,
        self._redmine = Redmine(ip, username=uname, password=pwd)
    
    @property
    def ip(self):
        return self._ip
    
    @property
    def uname(self):
        return self._uname
    
    @property
    def pwd(self):
        return self._pwd    
    @property
    def redmine(self):
        return self._redmine