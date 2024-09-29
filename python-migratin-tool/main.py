from controller import Controller

print("Begin migration")
controller = Controller()

print("Load configuration files...")
redmine_projects = controller.read_redmine_projects()
redmines = controller.read_redmines_json()
database = controller.read_database_json()
users_ids = controller.read_users_json()
tracker_ids = controller.read_trackers_json()
status_ids = controller.read_statuses_json()

tracker_ids = controller.trackers_migration(database, tracker_ids, status_ids)
# tracker_ids = controller.trackers_migration(database, tracker_ids, status_ids)

for source, destination in redmine_projects.items():
    redmineSource = redmines["source"]
    redmineSource["identifier"] = source
    redmineDestination = redmines["destination"]
    if destination is not None:
        redmineDestination["identifier"] = destination
    else:
        redmineDestination["identifier"] = None

    print("Connect to source server...")
    controller.connect_to_source_server(redmineSource)

    print("Connect to destination server...")
    controller.connect_to_destination_server(redmineDestination, redmineSource)
    journals = controller.get_journals()

    print("Create issues...")
    controller.create_issues()

    print("Upload issues with historys...")
    controller.upload_historys(journals, tracker_ids, status_ids, users_ids)

    print("Set issue's attributes...")
    controller.update_issues(issue_ids, users_ids, tracker_ids, status_ids)
    controller.update_issues(users_ids, tracker_ids, status_ids)
    controller.update_journals(database, journals, users_ids)

print("Migration completed!")
