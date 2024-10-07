import sys

from controller import Controller

def redmine_migration(validation):
    print("Begin migration")
    controller = Controller()

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
            redmineDestination["identifier"] = None

        print("Connect to source server...")
        controller.connect_to_source_server(redmineSource)

        print("Connect to destination server...")
        controller.connect_to_destination_server(redmineDestination, redmineSource)

        if validation:
            print("Validation...")
            return controller.validate_migration(database, users_ids)

        # Jornals = hozzászólások + státusz változások
        journals = controller.get_journals()

        print("Create issues...")
        controller.create_issues()

        print("Upload issues with historys...")
        controller.upload_historys(journals, tracker_ids, status_ids, users_ids)

        print("Set issue's attributes...")
        controller.update_issues(users_ids, tracker_ids, status_ids)
        controller.update_journals(database, journals, users_ids)

    print("Migration completed!")

if __name__ == "__main__":
    need_validate = False
    if len(sys.argv) > 1:
        if sys.argv[1] == "validate":
            need_validate = True
            print("Validation mode:")

    redmine_migration(need_validate)