using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Text.RegularExpressions;

public class DeploymentSite
{
    public string Name { get; set; }
    public string[] Servers { get; set; }
    public string[] Environments { get; set; }
    public string Path { get; set; }
    public string Package { get; set; }
}
public class DeploymentServer
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Path { get; set; }
}
public class DeploymentAppSetting
{
    public string Key { get; set; }
    public string Value { get; set; }
}
public class DeploymentConnectionString
{
    public string Name { get; set; }
    public string Value { get; set; }
}
public class DeploymentEnvironment
{
    public string Name { get; set; }
    public DeploymentAppSetting[] AppSettings { get; set; }
    public DeploymentConnectionString ConnectionString { get; set; }
}
public class DeploymentConfig
{
    public string DbPasswordStaging { get; set; }
    public DeploymentEnvironment[] Environments { get; set; }
    public DeploymentServer[] Servers { get; set; }
    public DeploymentSite[] Sites { get; set; }

    public string GetPass(string environment)
    {
        environment = (environment ?? "").ToLower();
        switch (environment)
        {
            case "staging":
                return DbPasswordStaging;
            default:
                throw new Exception("Unknown environment '"+environment+"'");
        }
        throw new Exception("Unknown environment '"+environment+"'");
    }

    public void ForEachSiteAndServer(Action<DeploymentSite, DeploymentServer> action)
    {
        foreach (var site in Sites)
        {
            foreach (var serverId in site.Servers)
            {
                var server = 
                    Servers
                        .SingleOrDefault(s => s.Id == serverId);

                action(site, server);
            }
        }
    }
}
public class JsonLowerCaseUnderscoreContractResolver : DefaultContractResolver
{
    private readonly Regex _regex = new Regex("(?!(^[A-Z]))([A-Z])");

    protected override string ResolvePropertyName(string propertyName)
    {
        return _regex.Replace(propertyName, "_$2").ToLower();
    }
}
/*
// use as:

var config = new DeploymentConfigParser().Parse("RegoProductionDeployment.json");

*/
public class DeploymentConfigParser
{
    public DeploymentConfig Parse(string path)
    {
        if (!File.Exists(path))
        {
            path = @"E:\sites\rego\tools\Scripts\Deployment\" + path;
            if (!File.Exists(path))
            {
                throw new Exception("Missing file "+path);
            }
        }

        var txt = File.ReadAllText(path);

        var settings = new JsonSerializerSettings()
        {
            ContractResolver = new JsonLowerCaseUnderscoreContractResolver(),
           
        };
        return JsonConvert.DeserializeObject<DeploymentConfig>(txt, settings);
    }
}
