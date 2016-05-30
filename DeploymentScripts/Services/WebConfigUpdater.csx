#load "DeploymentConfig.csx"

using System;
using System.IO;
using System.Xml.Linq;
using AFT.UGS.UgsToolsCommonCode.Extensions;
using AFT.UGS.UgsToolsCommonCode.Services;

public class WebConfigUpdater
{
    private readonly IEncryptionProvider _encryption;
    public WebConfigUpdater(IEncryptionProvider encryption)
    {
        _encryption = encryption;
    }

    public IEncryptionProvider Encryption { get { return _encryption; } }

    public void UpdateAll(DeploymentConfig config, string environment, string version)
    {
        config.ForEachSiteAndServer((site, server) =>
        {
            var path = @"\\" + server.Name + @"\" + server.Path.Replace(":", "$")+@"\"+version+@"\"+site.Path+@"\Web.config";

            if (site.Name.Contains("WinService"))
            {
                bool isBonus = site.Name.Equals("BonusWinService");

                path = @"\\" + server.Name + @"\" + server.Path.Replace(":", "$") + @"\" + version + @"\" + site.Path + @"\AFT.RegoV2." + (isBonus ? "Bonus." : "") + "WinService.exe.config";
            }

            UpdateSingleConfig(config, environment, path, server.Name, site.Name, version);

            if (site.Name.Equals("AdminWebsite"))
            {
                var jsPath = @"\\" + server.Name + @"\" + server.Path.Replace(":", "$") + @"\" + version + @"\" + site.Path + @"\App\config.js";
                UpdateSingleJsConfig(config, environment, jsPath);
            }

            Console.WriteLine("Updated:"+path);
        });
    }

    public void UpdateSingleConfig(DeploymentConfig config, string environment, string webConfigPath, string serverName, string siteName, string version)
    {
        var env = config.Environments.Where(e => e.Name == environment).First();

        var settings = env.AppSettings.ToDictionary(aps => aps.Key, aps => aps.Value, StringComparer.InvariantCultureIgnoreCase);

        var xDoc = XDocument.Parse(File.ReadAllText(webConfigPath));

        // change appSettings
        xDoc.ProcessElementsByXPath("/configuration/appSettings/add",
            add =>
            {
                string val;
                var key = add.AttrVal("key");
                if (settings.TryGetValue(key, out val))
                {
                    add.SetAttrVal("value", val);
                }
            });

        // update connection string
        xDoc.ProcessElementsByXPath("/configuration/connectionStrings/add",
            add =>
            {
                if (add.AttrVal("name") == env.ConnectionString.Name)
                {
                    //add.SetAttrVal("connectionString", env.ConnectionString.Value.Args(_encryption.Decrypt(config.GetPass(environment))));
                    add.SetAttrVal("connectionString", env.ConnectionString.Value.Args(config.GetPass(environment)));
                }
            });

        // update logs file path
        xDoc.ProcessElementsByXPath("/configuration/nlog/targets/target",
            add =>
            {
                if (add.AttrVal("name") == "file")
                {
                    add.SetAttrVal("fileName", @"\\regoweb01\RegoLogs" + @"\" + version + @"\" + serverName + @"\" + siteName + @"_log.txt");
                }
            });

        xDoc.Save(webConfigPath);
    }

    public void UpdateSingleJsConfig(DeploymentConfig config, string environment, string jsConfigPath)
    {
        var env = config.Environments.Where(e => e.Name == environment).First();

        var settings = env.AppSettings.ToDictionary(aps => aps.Key, aps => aps.Value, StringComparer.InvariantCultureIgnoreCase);

        string val;
        string text = File.ReadAllText(jsConfigPath);
        text = text.Replace("local", env.Name);
        File.WriteAllText(jsConfigPath, text);
    }
}
