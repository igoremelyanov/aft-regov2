#load "Services/SiteManager.csx"
#load "Services/WebConfigUpdater.csx"
#load "Services/DeploymentConfig.csx"
#load "Services/BuildProcessor.csx"

/*

on REGOWEB01
> cd e:\sites\rego\tools\Scripts\Deployment
> e:\scriptcs\scriptcs.exe RunBuildCommand.csx -- --version v1.3.12 --action update+point_qa --dropDb false

*/

using CommandLine;
using CommandLine.Text;
using System.IO;

var options = new Options();
var parser = new Parser(s => { s.HelpWriter = Console.Error; });
if (!parser.ParseArguments(Env.ScriptArgs.ToArray(), options)){
  throw new Exception("Cannot parse arguments");
}
const string tempFolderPath = @"E:\sites\rego\tools\Scripts\Deployment\_temp";
var config = new DeploymentConfigParser().Parse("RegoProductionDeployment.json");
IEncryptionProvider ep = new EncryptionProvider(new DataEncoder());
var wcu = new WebConfigUpdater(ep);
var sm = new SiteManager();

var processor = new BuildProcessor(wcu, sm, config, tempFolderPath, options.Version, options.Action, options.CanDropDb);
try {
    // if Action starts with "update":
    if(processor.MustUpdateFromNuget){
        // if temp folder exists: Error another build seems to be running
        Console.WriteLine("Ensuring temp folder does not exist");
        processor.EnsureTempFolderDoesNotExist();
        // create temp folder and nuget.config
        Console.WriteLine("Creating temp folder and packages.config file");
        processor.CreateTempFolder();
        // install nuget packages in the temp folder
        Console.WriteLine("Installing NuGet");
        processor.InstallNuGet();
        // copy files to sites folders
        Console.WriteLine("Copying files to sites folders");
        processor.CopyToSitesFolders();
    }
    // if Action contains "point_" 
    if(processor.MustPointToSite){
        // stop winservices
        Console.WriteLine("Stop WinServices");
        processor.StopWinServices();
        // if folder does not exist throw exception
        Console.WriteLine("Ensuring site folders exist");
        processor.EnsureSiteFoldersExist();
        // change configs
        Console.WriteLine("Updating configs");
        processor.UpdateConfigs();
        // if drop db and environment is not production: drop db
        Console.WriteLine("Dropping database if needed");
        processor.DropDatabaseIfNeeded();
        // point sites
        Console.WriteLine("Pointing IIS to sites");
        processor.PointToSites();
        // start winservices
        Console.WriteLine("Start WinServices");
        processor.StartWinServices();
    }
} finally {
    processor.DeleteTempFolder();
}

// -------------------------------
public class Options {
  [Option('v', "version", HelpText = "Version", Required = true)]
  public string Version { get; set; }
  [Option('a', "action", HelpText = "Action", Required = true)]
  public string Action { get; set; }
  [Option('d', "dropDb", HelpText = "Drop Database", Required = true)]
  public string DropDb { get; set; }

  public bool CanDropDb { get { return (DropDb ?? "").ToLower() == "true"; } }
}