#load "DeploymentConfig.csx"
#load "WebConfigUpdater.csx"
#load "SiteManager.csx"

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Policy;
using System.Xml.Linq;
using AFT.UGS.UgsToolsCommonCode.Extensions;
using AFT.UGS.UgsToolsCommonCode.Services;
using System.Net;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;

public class BuildProcessor
{
    private readonly WebConfigUpdater _wcu;
    private readonly SiteManager _sm;
    private readonly DeploymentConfig _config;
    private readonly string _tempFolder;
    private readonly string _version;
    private readonly string _action;
    private readonly string _environment;
    private readonly bool _dropDb;
    private readonly bool _mustUpdate;
    private readonly bool _mustPoint;

    private readonly IDictionary<string, string> _envByAction = 
        new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
        {
            {"update", "" },
            {"update+point_staging", "staging" },
            {"update+point_production", "production" },
            {"point_staging", "staging" },
            {"point_production", "production" }
        };

    public BuildProcessor(
        WebConfigUpdater ucu, 
        SiteManager sm, 
        DeploymentConfig config,
        string tempFolder,
        string version,
        string action,
        bool dropDb)
    {
        _wcu = ucu;
        _sm = sm;
        _config = config;
        _version = (version??"").Trim().StartsWith("v") ? version.Substring(1) : version;
        _tempFolder = tempFolder;
        _action = action;
        if (!_envByAction.ContainsKey(_action))
        {
            throw new Exception("Unknown action '"+_action+"'");
        }
        _environment = _envByAction[_action];
        if (_environment == "production" && dropDb)
        {
            throw new Exception("It is not allowed to drop production database");
        }
        _dropDb = dropDb;
        _mustUpdate = _action.StartsWith("update");
        _mustPoint = _action.IndexOf("point_") >= 0;

        Console.WriteLine(
            " version:"+_version+
            " action:"+_action+
            " environment:"+_environment+
            " drop db:"+_dropDb+
            " must update:"+_mustUpdate+
            " must point:"+_mustPoint);

    }

    public bool MustUpdateFromNuget { get { return _mustUpdate; } }
    public bool MustPointToSite { get { return _mustPoint; } }

    private string TempFolderPath { get { return _tempFolder; } }

    public void EnsureTempFolderDoesNotExist(){
        if (Directory.Exists(TempFolderPath))
        {
            throw new Exception("There seems to be another build running, temp folder exists, if not delete temp folder "+TempFolderPath);
        }
        Console.WriteLine("verified that temp folder does not exist at "+TempFolderPath);
    }
    public void CreateTempFolder(){
        Directory.CreateDirectory(TempFolderPath);
        File.WriteAllText(TempFolderPath+@"\packages.config", @"<?xml version=""1.0"" encoding=""utf-8""?>
<packages>
  <package id=""AFT.RegoV2.AdminApi"" version=""{0}"" targetFramework=""net45"" />
  <package id=""AFT.RegoV2.AdminWebsite"" version=""{0}"" targetFramework=""net45"" />
  <package id=""AFT.RegoV2.FakeUGS"" version=""{0}"" targetFramework=""net45"" />
  <package id=""AFT.RegoV2.GameWebsite"" version=""{0}"" targetFramework=""net45"" />
  <package id=""AFT.RegoV2.MemberApi"" version=""{0}"" targetFramework=""net45"" />
  <package id=""AFT.RegoV2.MemberWebsite"" version=""{0}"" targetFramework=""net45"" />
  <package id=""AFT.RegoV2.Bonus.Api"" version=""{0}"" targetFramework=""net45"" />
  <package id=""AFT.RegoV2.FakePaymentServer"" version=""{0}"" targetFramework=""net45"" />
  <package id=""AFT.RegoV2.WinService"" version=""{0}"" targetFramework=""net45"" />
  <package id=""AFT.RegoV2.Bonus.WinService"" version=""{0}"" targetFramework=""net45"" />
</packages>".Args(_version));
        Console.WriteLine("created temp folder "+TempFolderPath);
    }
    public void InstallNuGet(){
        var res = ExecuteScript(@"cd """+TempFolderPath+@""";c:\NuGet\NuGet.exe install packages.config -ConfigFile c:\NuGet\NuGet.config -OutputDirectory """+TempFolderPath+@""" -NoCache");
        foreach (PSObject e in res)
        {
            Console.WriteLine(e.BaseObject.ToString());
        }
        foreach (var site in _config.Sites)
        {
            var folder = Path.Combine(TempFolderPath, site.Package + "." + _version);
            if (!Directory.Exists(folder))
            {
                throw new Exception(
                    "Cannot find package:"+site.Package + "." + _version+" "+
                    "Do you have it on the server? "+
                    "Missing folder:"+folder);
            }
        }
        Console.WriteLine("installed NuGet version "+_version);
    }
    public void CopyToSitesFolders(){
        // verify that live versions ('staging','production' are not same as current)
        var currVerName = "v" + _version;
        foreach (var e in _config.Environments)
        {
            var env = e.Name;
            var ver = _sm.GetVersionByEnvironment(env);
            if (ver == currVerName || ver == _version)
            {
                throw new Exception("Version "+_version+" is already being used in environment:"+env);
            }
        }

        // delete web configs transforms
        // rename temp web configs
        foreach (var site in _config.Sites)
        {
            var packFolder = Path.Combine(TempFolderPath, site.Package + "." + _version);
            var contentFolder = Path.Combine(packFolder, "Content");
            if(File.Exists(Path.Combine(contentFolder,"Web.config.transform")))
                File.Delete(Path.Combine(contentFolder,"Web.config.transform"));
            if(File.Exists(Path.Combine(contentFolder,"_temp.Web.config")))
                File.Move(Path.Combine(contentFolder,"_temp.Web.config"), Path.Combine(contentFolder,"Web.config"));
        }

        // ensure site folders
        _config.ForEachSiteAndServer((site, server) =>
        {
            var packFolder = Path.Combine(TempFolderPath, site.Package + "." + _version);
            var contentFolder = Path.Combine(packFolder, "Content");

            var versionPath = @"\\" + server.Name + @"\" + server.Path.Replace(":", "$") + "v" + _version;

            // if version folder does not exist: create it
            if (!Directory.Exists(versionPath))
            {
                Directory.CreateDirectory(versionPath);
            }

            var sitePath = Path.Combine(versionPath, site.Path);
            // if site folder exists
            if (Directory.Exists(sitePath))
            {
                // delete it recursively
                ExecuteScript("rmdir -r \""+sitePath+"\"");
            }
            // creeate site folder
            Directory.CreateDirectory(sitePath);

            // xcopy site
            ExecuteScript("xcopy \""+contentFolder+"\" \""+sitePath+"\" /s /e");

            Console.WriteLine("Copied from \""+contentFolder+"\" to \""+sitePath+"\"");
        });

        Console.WriteLine("sites copied to folders");
    }
    public void DeleteTempFolder(){
        if (Directory.Exists(TempFolderPath))
        {
            ExecuteScript("rmdir -r \"" + TempFolderPath + "\"");
            Console.WriteLine("deleted temp folder " + TempFolderPath);
        }
    }
    public void EnsureSiteFoldersExist(){
        _config.ForEachSiteAndServer((site, server) =>
        {
            var packFolder = Path.Combine(TempFolderPath, site.Package + "." + _version);
            var contentFolder = Path.Combine(packFolder, "Content");

            var versionPath = @"\\" + server.Name + @"\" + server.Path.Replace(":", "$") + "v" + _version;

            // if version folder does not exist: create it
            if (!Directory.Exists(versionPath))
            {
                Directory.CreateDirectory(versionPath);
            }

            var sitePath = Path.Combine(versionPath, site.Path);
            if(Directory.Exists(sitePath))
                Console.WriteLine("Confirm folder exists:"+sitePath);
            else
                throw new Exception("Expected folder cannot be found:"+sitePath);
        });
    }
    public void UpdateConfigs(){
        _wcu.UpdateAll(_config, _environment, "v"+_version);
        Console.WriteLine("Updated all configs of version "+_version+" to environment "+_environment);
    }
    public void DropDatabaseIfNeeded(){
        if (_dropDb && _environment != "production")
        {
            var dbName = "Rego-Staging";
            //TODO: REMOVE SA LOGIN AND USE REGOSTAGING INSTEAD WHEN PERMISSIONS ISSUES ARE RESOLVED
            var dbUser = "sa";
            var dbPass = "R3g0p@ss123!!";
            //var dbUser = "regostaging";
            //var dbPass = _config.DbPasswordStaging;

            Console.WriteLine("Code that will execute:" + @"E:\sites\rego\tools\DatabaseHelper\UgsDatabaseHelper.exe -c DropCreateAagDatabase -u " + dbUser + " -p ****** -d " + dbName);

            var res = ExecuteScript(@"E:\sites\rego\tools\DatabaseHelper\UgsDatabaseHelper.exe -c DropCreateAagDatabase -u " + dbUser + " -p " + dbPass + " -d " + dbName);
            foreach (PSObject e in res)
            {
                Console.WriteLine(e.BaseObject.ToString());
            }
        }
    }
    public void PointToSites(){

        var ver = _sm.GetVersionByEnvironment(_environment);
        if (ver == "v"+_version || ver == _version)
        {
            throw new Exception("Version "+_version+" is already being used in environment:"+_environment);
        }

        _sm.PointEnvironmentToVersion(_config, _environment, "v"+_version);

//        var url = 
//            "http://{0}tgpaccess.com/api/ugs/server-status/4e5461aa-5e68-411c-8395-fecb65460825"
//            .Args(_environment == "production" ? "" : _environment+".");
//        // check the status page
//        using(var wc = new WebClient()){
//            var s = wc.DownloadString(url);
//            var ss = JsonConvert.DeserializeObject<ServerStatus>(s);
//
//            if (string.IsNullOrEmpty(ss.PhysicalPath))
//            {
//                throw new Exception("Unexpected status response: "+s);
//            }
//            if (!ss.CanConnectToDb)
//            {
//                throw new Exception("Site is live but cannot connect to database. Status:"+s);
//            }
//        }

        Console.WriteLine("Pointed environment "+_environment+" to version "+_version);
    }
    public void StopWinServices()
    {
        _sm.SetupWinService(_config, _environment, "v" + _version, "stop");

        Console.WriteLine("Stopped " + _environment + " WinService");
    }

    public void StartWinServices()
    {
        _sm.SetupWinService(_config, _environment, "v" + _version, "start");

        Console.WriteLine("Started " + _environment + " WinService of version " + _version);
    }

    private static Collection<PSObject> ExecuteScript(string script){
        return ExecuteScript(null, script);
    }
    private static Collection<PSObject> ExecuteScript(string server, string script){
        PowerShell ps = PowerShell.Create();
        ps.AddCommand("Invoke-Command");
        if(!string.IsNullOrEmpty(server)) ps.AddParameter("-ComputerName",server);
        ps.AddParameter("-ScriptBlock",ScriptBlock.Create(script));

        var r = ps.Invoke();

        if(ps.Streams != null && ps.Streams.Error != null && ps.Streams.Error.Count > 0)
            throw new Exception("POWERSHELL ERROR:"+ps.Streams.Error.JoinAsString("; "));

        return r;
    }
}
public class ServerStatus{
    public string PhysicalPath { get; set; }
    public string MachineName { get; set; }
    public string Version { get; set; }
    public string DbVersion { get; set; }
    public bool CanConnectToDb { get; set; }
}