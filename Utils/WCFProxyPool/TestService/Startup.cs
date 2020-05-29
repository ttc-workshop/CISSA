using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel;

namespace TestService
{
    public static class Startup
    {
        static void Main()
        {
            Console.WriteLine("Attempting to start service....\n\n");

            ServiceHost host = new ServiceHost(typeof(service1));
            ServiceHost host2 = new ServiceHost(typeof(Service2));
            host.Open();
            host2.Open();

            Console.WriteLine("Service started, hit <ENTER> to end.....");
            Console.ReadLine();
            Console.WriteLine("....closing down services...");

            host.Close();
            host2.Close();
        }
    }
}
