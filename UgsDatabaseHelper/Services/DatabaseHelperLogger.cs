using System;
using System.IO;
using System.Reflection;

namespace AFT.UGS.Builds.UgsDatabaseHelper.Services
{
    public interface IDatabaseHelperLogger
    {
        void CreateLog();
        void LogInfo(string info, params object[] args);
        void LogError(string info, params object[] args);
        Exception NewException(string message, params object[] args);
    }

    public sealed class DatabaseHelperLogger : IDatabaseHelperLogger
    {
        const string LogsFolder = "rego-database-helper-logs";
        const string LogName = "rego-log-{0}.log";
        private readonly IDatabaseHelperLogger _interface;

        private string _path;

        public DatabaseHelperLogger()
        {
            _interface = this;
        }

        void IDatabaseHelperLogger.CreateLog()
        {
            var assemblyDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var dirName = Path.Combine(assemblyDirectory, LogsFolder);
            if (!Directory.Exists(dirName))
                Directory.CreateDirectory(dirName);

            _path =
                Path.Combine(
                    dirName,
                    string.Format(LogName, DateTime.Now.ToString("yyyyMMddHHmmssfff")));

            _interface.LogInfo("Log created "+_path);
        }
        void IDatabaseHelperLogger.LogInfo(string info, params object[] args)
        {
            var s = DateTimeOffset.Now + " : " + String.Format(info, args) + "\n";
            Console.Write(s);
            File.AppendAllText(_path, s);
        }
        void IDatabaseHelperLogger.LogError(string info, params object[] args)
        {
            var s = "ERROR "+DateTimeOffset.Now+" !!!!!\n"+String.Format(info, args)+"\n-----------------\n";
            Console.Write(s);
            File.AppendAllText(_path, s);
        }
        Exception IDatabaseHelperLogger.NewException(string message, params object[] args)
        {
            _interface.LogError(message, args);

            return new Exception(String.Format(message, args));
        }
    }
}