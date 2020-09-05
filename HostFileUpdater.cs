using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;

namespace HostFileWritter
{
    class HostFileUpdater
    {
        Settings Config { get; set; }
        DateTime Now { get; set; }
        DateTime Start { get; set; }
        DateTime Stop { get; set; }
        string HostsFilePath { get; set; }
        Logger Log { get; set; }
        List<string> Urls { get; set; } = new List<string>();

        public HostFileUpdater(string[] args)
        {
            Now = DateTime.Now;
            Log = new Logger();

            try
            {                
                var builder = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json");

                IConfigurationRoot root = builder.Build();
                Config = root.GetSection("Settings").Get<Settings>();
                HostsFilePath = Config.HostsPath;
                Start = new DateTime(Now.Year, Now.Month, Now.Day,
                    Config.StartHour, Config.StartMinute, 0);
                Stop = new DateTime(Now.Year, Now.Month, Now.Day,
                    Config.StopHour, Config.StopMinute, 0);
                Urls.AddRange(Config.Urls);                
                Log.Write("Initialization succeeded.", Severity.Debug);

            }
            catch (Exception ex)
            {
                Log.Write("ERROR: Initialization Failed. Verify the config file is in this location: "
                    + Directory.GetCurrentDirectory() + " and that the values in it are valid. "
                    + ex.Message, Severity.Error);                
            }
        }

        [PrincipalPermission(SecurityAction.Demand, Role = @"BUILTIN\Administrators")]
        public void Run()
        {
            try
            {

                if (Now < Start || !Config.DaysOfWeek[(int)DateTime.Now.DayOfWeek])
                {
                    Log.Write("Running before start hour or at non-blokcing day. Ending run.", Severity.Debug);
                    return;
                }
                
                if (Now < Stop)
                {
                    Log.Write("Proceeding to block blacklisted sites.", Severity.Debug);
                    //Read Original hosts file Content
                    List<string> lines = File.ReadAllLines(HostsFilePath).ToList();
                    var originalLines = lines.Count;
                    Log.Write("Original hosts file memory load complete.", Severity.Debug);

                    //Check if hosts changes are required
                    foreach (var line in lines)
                    {
                        if (!line.StartsWith("127.0.0.1 ")) continue;
                        var url = line.Replace("127.0.0.1 ", "");
                        Urls = Urls.FindAll(u => !u.Contains(url));
                    }

                    if (Urls == null || Urls.Count == 0)
                    {
                        Log.Write("No new urls to block. Ending run.", Severity.Info);
                        return;
                    }

                    //Block Websurfing
                    var i = 0;
                    using (StreamWriter file = new StreamWriter(HostsFilePath, true))
                    {
                        Log.Write("About to rewrite host file, " + Urls.Count + " new urls to be blocked.", Severity.Debug);
                        file.WriteLine("");
                        foreach (string url in Urls)
                        {
                            file.WriteLine("127.0.0.1 " + url);
                            i++;
                            Log.Write("" + url + " blocked.", Severity.Debug);
                        }
                    }

                    Log.Write("Host file updated." + i + " sites blacklisted. Filestream Flushing done.", Severity.Info);
                }
                else
                {
                    Log.Write("Blocktime cleared, proceeding to allow back navigation to blocked sites.", Severity.Debug);

                    //Allow Websurfing                
                    List<string> lines = File.ReadAllLines(HostsFilePath).ToList();
                    var originalLines = lines.Count;
                    Log.Write("Host file memory load complete.", Severity.Debug);

                    List<string> nonBlockedURLLines = lines.FindAll(l => !l.Equals(string.Empty) && !(l.StartsWith("127.0.0.1 ") && Urls.Contains(l.Replace("127.0.0.1 ", ""))));
                    if (originalLines - nonBlockedURLLines.Count == 0)
                    {
                        Log.Write("Nothing to update. Ending run.", Severity.Info);
                        return;
                    }

                    Log.Write("About to rewrite host file, " + (originalLines - nonBlockedURLLines.Count) + " lines to be deleted.", Severity.Debug);

                    File.WriteAllLines(HostsFilePath, nonBlockedURLLines.ToArray());
                    Log.Write("Host file updated. Blacklisted sites cleared. Filestream Flushing done.", Severity.Debug);
                }

            }
            catch (UnauthorizedAccessException uAEx)
            {
                Log.Write(uAEx.Message, Severity.Error);
            }
            catch (PathTooLongException pathEx)
            {
                Log.Write(pathEx.Message, Severity.Error);
            }
            catch (Exception ex)
            {
                Log.Write("ERROR:" + ex.Message, Severity.Error);                
            }
        }
    }
}
