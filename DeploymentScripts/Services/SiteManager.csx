#load "DeploymentConfig.csx"

using System;
using System.Linq;
using System.Management.Automation;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
using AFT.UGS.UgsToolsCommonCode.Extensions;

public class SiteManager
{
    public void StopSite(string server, string site)
    {
        ExecuteScript(server, @"Stop-WebSite """+site+@"""");
    }
    public void StartSite(string server, string site)
    {
        ExecuteScript(server, @"Start-WebSite """ + site + @"""");
    }
    public string GetSitePhysicalPath(string server, string site)
    {
        var r = ExecuteScript(server, @"Import-Module ""WebAdministration""; dir ""IIS:\""; Get-WebFilePath ""IIS:\Sites\"+site+@"""");
        return r.First(e =>
                    e.Properties["Attributes"] != null &&
                    e.Properties["Attributes"].Value.ToString().IndexOf("Directory") >= 0
                    ).Properties["FullName"].Value.ToString();
    }
    public void SetSitePhysicalPath(string server, string site, string physicalPath)
    {
        ExecuteScript(server, @"Import-Module ""WebAdministration""; dir ""IIS:\""; Set-ItemProperty ""IIS:\Sites\"+site+@""" -Name physicalPath -Value """+physicalPath+@"""");

        Console.WriteLine("Set Server: "+server+" Site: "+site+" Physical Path:"+physicalPath);
    }
    public string GetEnvironmentsReport(DeploymentConfig config, string environment)
    {
        var prefix = GetEnvironmentPrefix(environment);
        var sb = new StringBuilder();
        sb.AppendLine("Environment: "+environment);
        sb.AppendLine("Version: "+GetVersionByEnvironment(environment));
        config.ForEachSiteAndServer((site, server) =>
        {
            sb.Append(prefix+site.Path)
              .Append(" on ")
                .Append(server.Name)
              .Append(": ")
              .AppendLine(GetSitePhysicalPath(server.Name, prefix+site.Path));
        });
        return sb.ToString();
    }
    public string GetVersionByEnvironment(string environment)
    {
        var s = GetSitePhysicalPath("REGOWEB01", GetEnvironmentPrefix(environment) + "regov2-admin-api");
        var m = Regex.Match(s, @"(?:[vV]{1,1}[0-9]{1,}\.[0-9]{1,}\.[0-9]{1,})");
        if (m.Success)
        {
            return m.Groups[0].Value;
        }
        throw new Exception("Cannot find version by path:" + s);
    }
    public string GetEnvironmentByVersion(string version)
    {
        var staging = GetVersionByEnvironment("staging");
        if (version == staging) return "staging";
        var production = GetVersionByEnvironment("production");
        if (version == production) return "production";
        return null;
    }
    public string GetEnvironmentPrefix(string environment)
    {
        environment = (environment ?? "production").ToLower();
        return (environment == "staging") ? "staging-" : "";
    }
    public void StopEnvironment(DeploymentConfig config, string environment)
    {
        var prefix = GetEnvironmentPrefix(environment);
        config.ForEachSiteAndServer((site, server) =>
                StopSite(server.Name, prefix + site.Path));
    }
    public void StartEnvironment(DeploymentConfig config, string environment)
    {
        var prefix = GetEnvironmentPrefix(environment);
        config.ForEachSiteAndServer((site, server) =>
                StartSite(server.Name, prefix + site.Path));
    }
    public void PointEnvironmentToVersion(DeploymentConfig config, string environment, string version)
    {
        if (!version.ToLower().StartsWith("v")) version = "v" + version;
        var prefix = GetEnvironmentPrefix(environment);
        config.ForEachSiteAndServer((site, server) => 
        {
            if (!site.Name.Contains("WinService"))
            {
                if (site.Environments.Contains(environment))
                {
                    SetSitePhysicalPath(
                        server.Name,
                        prefix + site.Path,
                        server.Path.TrimEnd('\\') + @"\" + version + @"\" + site.Path);

                    StartSite(server.Name, prefix + site.Path);
                }
                else
                {
                    StopSite(server.Name, prefix + site.Path);
                }
            }
        });
    }
    public void SetupWinService(DeploymentConfig config, string environment, string version, string action)
    {
        if (!version.ToLower().StartsWith("v")) version = "v" + version;
        var prefix = GetEnvironmentPrefix(environment);
        var env = config.Environments.Where(e => e.Name == environment).First();
        var settings = env.AppSettings.ToDictionary(aps => aps.Key, aps => aps.Value, StringComparer.InvariantCultureIgnoreCase);

        config.ForEachSiteAndServer((site, server) =>
        {
            if (site.Name.Contains("WinService"))
            {
                bool isBonus = site.Name.Equals("BonusWinService");

                if (action.Equals("stop"))
                {
                    string winServiceName;
                    if (settings.TryGetValue((isBonus ? "Bonus" : "") + "WinServiceName", out winServiceName))
                    {
                        Console.WriteLine("Attempting to stop " + winServiceName + " on server " + server.Name);
                        ExecuteScript(server.Name, @"sc.exe stop " + winServiceName);
                        Thread.Sleep(10000);
                        Console.WriteLine("Attempting to delete " + winServiceName + " on server " + server.Name);
                        ExecuteScript(server.Name, @"sc.exe delete " + winServiceName);
                        Thread.Sleep(10000);
                    }
                }
                else
                {
                    string winServicePhysicalPath = server.Path.TrimEnd('\\') + @"\" + version + @"\" + site.Path + @"\AFT.RegoV2." + (isBonus ? "Bonus." : "") + "WinService.exe";

                    Console.WriteLine("Attempting to install WinService of version " + version + " on server " + server.Name);
                    ExecuteScript(server.Name, winServicePhysicalPath + @" install");
                    Console.WriteLine("Attempting to start WinService of version " + version + " on server " + server.Name);
                    ExecuteScript(server.Name, winServicePhysicalPath + @" start");
                }
            }
        });
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
