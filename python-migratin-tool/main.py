from controller import Controller

print("Begin migration")
controller = Controller()

# TODO le kell kérni a státuszokat, majd a workflow-ot hozzá és át kell rakni a másik redmine-ba.
# TODO Issue helyes hivatkozás

print("Load configuration files...")
redmine_projects = controller.read_redmine_projects()
redmines = controller.read_redmines_json()
database = controller.read_database_json()
users_ids = controller.read_users_json()
tracker_ids = controller.read_trackers_json()
status_ids = controller.read_statuses_json()

for source, destination in redmine_projects.items():
    redmineSource = redmines["source"]
    redmineSource["identifier"] = source
    redmineDestination = redmines["destination"]
    if destination is not None:
        redmineDestination["identifier"] = destination
    else:
        redmineDestination["identifier"] = source

    print("Connect to source server...")
    controller.connect_to_source_server(redmineSource)

    print("Connect to destination server...")
    controller.connect_to_destination_server(redmineDestination, redmineSource)
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
