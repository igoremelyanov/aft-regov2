# An utility to help drop/create, backup, restore REGO database

Currently supports 3 different commands:

## DropCreateAagDatabase - Drop then create Rego-Staging in an Availability Group environment

Parameters:

	-u, --username                 Database username.

	-p, --password                 Database user password.

	-g, --availabilityGroupName    Name of the AlwaysOn Availability Group.

	-d, --databaseName             Database name.

	-b, --backupPath               Path to store the full database backups.

	-P, --primaryReplica           Hostname of primary replica.

	-S, --secondaryReplicas        Comma separated list of secondary replica hostnames.

These parameters can be either provided as command line arguments or through application configuration file as appSettings. Command line arguments take precedence.

Example:

	>UgsDatabaseHelper.exe -c DropCreateAagDatabase -u sa -p saPassword -d Rego-Staging

## Backup - Create a full backup to disk

Run backup SQL statement with "with format" option.

Parameters:

	-u, --username                 Database username.

	-p, --password                 Database user password.

	-d, --databaseName             Database name.

	-f, --file                     Path to the backup file to be used in the Backup or Restore command.

	-h, --host                     Database host for Backup or Restore command.

These parameters can be either provided as command line arguments or through application configuration file as appSettings. Command line arguments take precedence.

Example:

	>UgsDatabaseHelper.exe -c Backup -u sa -p saPassword -d Rego -f D:\backups\Rego.bak -h localhost

## Restore - Restore a full backup from disk

Restore a database to the point in the specified backup file.

Parameters:

	-u, --username                 Database username.

	-p, --password                 Database user password.

	-d, --databaseName             Database name.

	-f, --file                     Path to the backup file to be used in the Backup or Restore command.

	-h, --host                     Database host for Backup or Restore command.

These parameters can be either provided as command line arguments or through application configuration file as appSettings. Command line arguments take precedence.

Example:

	>UgsDatabaseHelper.exe -c Restore -u sa -p saPassword -d Rego -f D:\backups\Rego.bak -h localhost