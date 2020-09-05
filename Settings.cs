using System;
using System.Collections.Generic;
using System.Text;

namespace HostFileWritter
{
    internal class Settings
    {
        public string LogsPath { get; set; }
        public string HostsPath { get; set; }
        public string LogSeverity { get; set; }
        public bool[] DaysOfWeek { get; set; }
        public int StartHour { get; set; }
        public int StartMinute { get; set; }
        public int StopHour { get; set; }
        public int StopMinute { get; set; }
        public string[] Urls { get; set; }        
    }
}
