using CommandLine;
using CommandLine.Text;

namespace AFT.UGS.Builds.UgsDatabaseHelper
{
    public class Options
    {
        [Option('u', "username", HelpText = "Database username.")]
        public string Username { get; set; }

        [Option('p', "password", HelpText = "Database user password.")]
        public string Password { get; set; }

        [Option('g', "availabilityGroupName", HelpText = "Name of the AlwaysOn Availability Group.")]
        public string AvailabilityGroupName { get; set; }

        [Option('d', "databaseName", HelpText = "Database name.")]
        public string DatabaseName { get; set; }

        [Option('b', "backupPath", HelpText = "Path to store the full database backups.")]
        public string BackupPath { get; set; }

        [Option('P', "primaryReplica", HelpText = "Hostname of primary replica.")]
        public string PrimaryReplica { get; set; }

        [Option('S', "secondaryReplicas", HelpText = "Comma separated list of secondary replica hostnames.")]
        public string SecondaryReplicas { get; set; }

        [Option('c', "command", HelpText = "Command. Valid commands are DropCreateAagDatabase, Backup and Restore", Required = true)]
        public string Command { get; set; }

        [Option('h', "host", HelpText = "Database host for Backup or Restore command.")]
        public string Host { get; set; }

        [Option('f', "file", HelpText = "Path to the backup file to be used in the Backup or Restore command.")]
        public string FilePath { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}
