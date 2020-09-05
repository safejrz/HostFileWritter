using Microsoft.Extensions.Configuration;
using System;

namespace HostFileWritter
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFileUpdater hostFileUpdater = new HostFileUpdater(args);
            hostFileUpdater.Run();
        }
    }
}
