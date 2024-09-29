from server import Server
from db import Database
import json
import requests
import re

class Controller:
    def __init__(self) -> None:
        self.source_server = ""
        self.source_project = ""
        self.issues_from_source = ""
        self.destination_server = ""
        self.destination_project = ""
        self.issue_ids_map = {}

    def read_redmine_projects(self):
        with open('./resources/projects.json') as redmine_projects_file:
            redmine_projects_json = json.load(redmine_projects_file)
        return redmine_projects_json

    def read_redmines_json(self):
        with open('./resources/redmines.json') as redmines_file:
            redmines_json = json.load(redmines_file)
        return redmines_json

    def read_database_json(self):
        with open('./resources/database.json') as db_file:
            db_json = json.load(db_file)
        return db_json["database"]

    def read_users_json(self):
        with open('./resources/users.json') as users_file:
            users_json = json.load(users_file)
        return users_json

    def read_trackers_json(self):
        with open('./resources/trackers.json') as trackers_file:
            trackers_json = json.load(trackers_file)
        return trackers_json["trackers"]

    def read_statuses_json(self):
        with open('./resources/statuses.json') as statuses_file:
            statuses_json = json.load(statuses_file)
        return statuses_json

    def trackers_migration(self, database, tracker_ids, status_ids):
        database = Database(database["host"], database["user"], database["password"], database["database"], database["port"])
        workflows = database.get_workflows(tracker_ids)
        
        try:
            for workflow in workflows:
                print(workflow['tracker_id'])
                print(tracker_ids[str(workflow['tracker_id'])])
                new_tracker_id = database.insert_workflow(workflow, tracker_ids[str(workflow['tracker_id'])], status_ids[str(workflow['old_status_id'])], status_ids[str(workflow['new_status_id'])])
                print(new_tracker_id)
                tracker_ids[str(workflow['tracker_id'])] = new_tracker_id
                
            for tracker_id in tracker_ids:
                print(tracker_id)
        except Exception as e:
            print(e)
            
        return tracker_ids
        
    def connect_to_source_server(self, source):
        source_server = Server(source["ip"], source["username"], source["password"])
        source_project = source_server.redmine.project.get(source["identifier"])
        self.source_server = source_server
        self.source_project = source_project
        self.issues_from_source = source_server.redmine.issue.all(include=['relations', 'attachments'])

    def connect_to_destination_server(self, destination, source):
        destination_server = Server(destination["ip"], destination["username"], destination["password"])
        self.destination_server = destination_server

        if destination["identifier"] == "" or destination["identifier"] is None:
            destination["identifier"] = self.create_new_project_in_Redmine(self.source_project, source["identifier"])

        destination_project = self.destination_server.redmine.project.get(destination["identifier"])
        self.destination_project = destination_project

    def create_new_project_in_Redmine(self, old_project, identifier):
        project = self.destination_server.redmine.project.new()
        project.name = old_project.name
        project.identifier = identifier
        project.description = old_project.description
        project.homepage = old_project.homepage
        project.is_public = old_project.is_public
        project.inherit_members = old_project.inherit_members
        project.save()
        return project.identifier

    def get_journals(self):
        journals = {}
        for issue in self.issues_from_source:
            journals[issue.id] = issue.journals
        return journals

    def create_issues(self):
        issue_ids = {}
        for old_issue in self.issues_from_source:
            subject = old_issue.subject
            description = old_issue.description
            for journal in old_issue.journals:
                for detail in journal.details:
                    if detail["name"] == "subject":
                        subject = detail["old_value"]
                    elif detail["name"] == "description":
                        description = detail["old_value"]

            issue = self.destination_server.redmine.issue.new()
            issue.project_id = self.destination_project.id
            issue.subject = subject
            issue.description = description
            issue.save()
            issue_ids[old_issue.id] = issue.id

        self.issue_ids_map = issue_ids

    def update_issue_description_references(self, description, issue_ids):
        pattern = r"#(\d+)"
        match = re.search(pattern, description)
        if match:
            old_issue_id = int(match.group(1))
            if old_issue_id in issue_ids:
                new_issue_ref = issue_ids[old_issue_id]
            else:
                new_issue_ref = old_issue_id + " (not migrated)"

            updated_description = re.sub(pattern, r"#{}".format(new_issue_ref), description)
            return updated_description

        return description

    def upload_historys(self, journals, tracker_ids, status_ids, user_ids):
        for issue in self.issues_from_source:
            for journal in journals[issue.id]:
                redmine_issue = self.destination_server.redmine.issue.get(self.issue_ids_map[issue.id])
                isUpdated = False
                for detail in journal.details:
                    if detail["property"] == "attachment":
                        for attachment in issue.attachments:
                            if str(attachment.id) == detail["name"]:
                                self.upload_attachment(redmine_issue, attachment)
                    if self.journal_updater(redmine_issue, detail["name"], detail["new_value"], tracker_ids, status_ids, user_ids):
                        isUpdated = True
                if isUpdated:
                    redmine_issue.notes = str(issue.id) + ":" + str(journal["id"])
                    redmine_issue.save()

    def upload_attachment(self, issue, attachment):
        # download file
        url = attachment.content_url
        response = requests.get(url)
        file = response.content
        filename = attachment.filename
        content_type = attachment.content_type
        description = attachment.description

        # upload file:
        url = self.destination_server.ip + "/uploads.json"
        response = requests.post(url, data=file, headers = {"Content-Type": "application/octet-stream"}, auth = (self.destination_server.uname[0], self.destination_server.pwd[0]))
        if response.status_code == 201:
            token = json.loads(response.text)['upload']['token']
        else:
            print("The upload of the file was unsuccessful. Status code: " + str(response.status_code))

        # update issue with the token
        issue.uploads = [{"token": token, "filename": filename, "content_type": content_type, "description": description}]

    def journal_updater(self, issue, name, value, tracker_ids, status_ids, user_ids):
        if name == "tracker_id":
            tracker_id = tracker_ids[value]
            issue.tracker_id = tracker_id
        elif name == "subject":
            issue.subject = value
        elif name == "description":
            issue.description = value
        elif name == "status_id":
            status_id = status_ids[value]
            issue.status_id = status_id
        elif name == "priority_id":
            issue.priority_id = value
        elif name == "assigned_to_id":
            assigned = user_ids[value]
            issue.assigned_to_id = assigned
        elif name == "parent_id":
            parent_id = self.issue_ids_map[int(value)]
            issue.parent_issue_id = parent_id
        elif name == "start_date":
            issue.start_date = value
        elif name == "due_date":
            issue.due_date = value
        elif name == "estimated_hours":
            issue.estimated_hours = value
        elif name == "done_ratio":
            issue.done_ratio = value
        elif name == "is_private":
            private = value == "1"
            issue.is_private = private
        else:
            return False
        return True

    def update_issues(self, user_ids, tracker_ids, status_ids):
        for old_issue in self.issues_from_source:
            try:
                new_parent_id = self.issue_ids_map[old_issue.parent.id]
            except:
                new_parent_id = None
            try:
                new_user_id = user_ids[str(old_issue.assigned_to.id)]
            except:
                new_user_id = None
            try:
                new_tracker_id = tracker_ids[str(old_issue.tracker.id)]
            except:
                new_tracker_id = None
            try:
                new_status_id = status_ids[str(old_issue.status.id)]
            except:
                new_status_id = None

            description = self.update_issue_description_references(old_issue.description, self.issue_ids_map)

            self.destination_server.redmine.issue.update(
                self.issue_ids_map[old_issue.id],
                description = description,
                tracker_id = new_tracker_id,
                status_id = new_status_id,
                priority_id = old_issue.priority.id,
                assigned_to_id = new_user_id,
                start_date = old_issue.start_date,
                done_ratio = old_issue.done_ratio,
                is_private = old_issue.is_private,
                parent_issue_id = new_parent_id,
                estimated_hours = old_issue.estimated_hours
            )

            issue = self.destination_server.redmine.issue.get(self.issue_ids_map[old_issue.id])
            for attachment in old_issue.attachments:
                is_set = False
                for attach in issue.attachments:
                    if attachment.filename == attach.filename and attachment.filesize == attach.filesize:
                        is_set = True
                if not is_set:
                    self.upload_attachment(issue, attachment)
                    issue.save()

    def update_journals(self, database, original_journals, users_ids):
        database = Database(database["host"], database["user"], database["password"], database["database"], database["port"])
        for new_issue in self.destination_project.issues:
            for new_journal in new_issue.journals:
                if new_journal["notes"] != "":
                    notes = new_journal["notes"].split(":")
                    o_issue_id = int(notes[0])
                    o_journal_id = int(notes[1])-1
                    try:
                        journal = original_journals[o_issue_id][o_journal_id]
                        database.update_journal(journal["created_on"], users_ids[str(journal["user"]["id"])], new_journal["id"])
                    except:
                        print("The journals does not found. Issue's id: " + str(o_issue_id) + ", journal's id: " + str(o_journal_id))