from controller import Controller
print("Begin migration")
controller = Controller()


print("Load configuration files...")
redmines = controller.read_redmines_json()
source = redmines["source"]
destination = redmines["destination"]
database = controller.read_database_json()
users_ids = controller.read_users_json()
tracker_ids = controller.read_trackers_json()
status_ids = controller.read_statuses_json()

print("Connect to source server...")
controller.connect_to_source_server(source)

print("Connect to destination server...")
controller.connect_to_destination_server(destination, source)
journals = controller.get_journals()
controller.initialize_issue()
issue_ids = controller.get_new_issue_ids()

print("Create issues...")
controller.create_issues()

print("Upload issues with historys...")
controller.upload_historys(journals, tracker_ids, status_ids, issue_ids, users_ids)

print("Set issue's attributes...")
controller.update_issues(issue_ids, users_ids, tracker_ids, status_ids)
controller.update_journals(database, journals, users_ids)
print("Migration completed!")