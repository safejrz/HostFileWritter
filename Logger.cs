using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace HostFileWritter
{
    public enum Severity
    {
        Error,
        Warning,
        Info,
        Debug
    }

    public class Logger
    {
        Settings Config { get;  }
        private string FilePath { get; }
        private Severity Sev { get; }

        public Logger()
        {
            var builder = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json");

            IConfigurationRoot root = builder.Build();
            Config = root.GetSection("Settings").Get<Settings>();

            Sev = (Severity)Enum.Parse(typeof(Severity), Config.LogSeverity);
            var now = DateTime.Now;
            var year = now.Year.ToString().Substring(2, 2);
            var month = now.Month >= 10 ? now.Month.ToString() : "0" + now.Month;
            var day = now.Day >= 10 ? now.Day.ToString() : "0" + now.Day;
            FilePath = Config.LogsPath + year + month + day + ".log";
        }   
        
        public void Write(string message, Severity severity = Severity.Info)
        {
            if (severity >= Sev) return;

            //Create directory if it doesn't exist yet
            Directory.CreateDirectory(Config.LogsPath);
            var fs = new FileStream(FilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);

            // Set stream position to end-of-file
            fs.Seek(0, SeekOrigin.End);
            fs.Close();

            using (StreamWriter file = new StreamWriter(FilePath, true))
            {                
                file.WriteLine(DateTime.Now + " " + message);
            }
        }
    }
}
