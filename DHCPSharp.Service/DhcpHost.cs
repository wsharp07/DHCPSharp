using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace DHCPSharp
{
    public partial class DhcpHost : ServiceBase
    {
        DhcpServer _server;
        public DhcpHost(DhcpServer server)
        {
            InitializeComponent();
            ServiceName = "DHCPHost";
            _server = server;
        }

        public void ManualStart(string[] args)
        {
            OnStart(args);
        }
        public void ManualStop()
        {
            OnStop();
        }
        protected override void OnStart(string[] args)
        {
            _server.Start();
        }

        protected override void OnStop()
        {
            _server.Stop();
        }

        public void RunAsConsole(string[] args)
        {
            Console.WriteLine("DHCP server is starting up...");
            OnStart(args);
            Console.WriteLine($"{ServiceName} is running");
            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
            OnStop();
        }
    }
}
