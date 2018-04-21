using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iris.Core.Network;
using Iris.Core;
using EmptyBox.IO.Network.IP;
using EmptyBox.Automation.Network;
using EmptyBox.Automation;
using EmptyBox.Automation.IO;

namespace Iris.DesktopTest
{
    class Program
    {
        static void Main(string[] args)
        {
        }

        static async void Tests()
        {
            Coordinator x = new Coordinator();
            x.Public.Address = new IrisAddress() { Address = "123" };
            Coordinator y = new Coordinator();
            y.Public.Address = new IrisAddress() { Address = "456" };
            TCPConnectionListener listener = new TCPConnectionListener(new IPAccessPoint(new IPAddress(0, 0, 0, 0), new IPPort(4444)));
            TCPConnection connection = new TCPConnection(new IPAccessPoint(new IPAddress(0, 0, 0, 0), new IPPort(4444)));
            ConnectionWorker cw0 = new ConnectionWorker();
            ConnectionWorker cw1 = new ConnectionWorker();
            ConnectionListenerWorker cwl = new ConnectionListenerWorker();
            
        }
    }
}
