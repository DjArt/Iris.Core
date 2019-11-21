using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iris.Core.Network;
using Iris.Core;
using EmptyBox.IO.Network.IP;
using EmptyBox.IO.Network;
using EmptyBox.Serializer;
using Iris.Core.Network.Packets;
using System.Windows.Forms;

namespace Iris.DesktopTest
{
    class Program
    {
        static IrisConnection connection;
        static IrisConnectionListener listener;
        static List<IrisConnection> connections = new List<IrisConnection>();
        
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.Unicode;
            method().Wait();
            Console.ReadKey();
        }

        static async Task method()
        {
            IrisAccountProvider accountProvider = new IrisAccountProvider();
            IrisAccount account = accountProvider.CreateAccount();
            Console.WriteLine("Создан аккаунт: {0}", account.Address);
            Console.Write("На каком порту запустить прослушиватель?: ");
            string readed = Console.ReadLine();
            if (readed == "0")
            {
                Console.WriteLine("Прослушиватель не был запущен.");
            }
            else
            {
                TCPConnectionListener listener = new TCPConnectionListener(new IPAccessPoint(new IPAddress(0, 0, 0, 0), ushort.Parse(readed)));
                accountProvider.AddConnectionListener(listener);
            }
            await accountProvider.Start();
            Console.WriteLine("Провайдер аккаунтов запущен.");
            Console.Write("Подключится к адресу?: ");
            readed = Console.ReadLine();
            if (readed == "0")
            {
                Console.WriteLine("Прослушиватель не был запущен.");
            }
            else
            {
                TCPConnection connection = new TCPConnection(new IPAccessPoint(IPAddress.Parse(readed.Split(':')[0]), ushort.Parse(readed.Split(':')[1])));
                var status = await connection.Open();
                if (status.Status == SocketOperationStatus.Success)
                {
                    Console.WriteLine("Статус: {0}", await accountProvider.AddConnection(connection));
                }
                else
                {
                    Console.WriteLine("Не удалось подключиться :(");
                }
            }
            await Task.Delay(3000);
            ServerPart(account);
            ClientPart(account);
            while(true)
            {
                string text = Console.ReadLine();
                if (text.StartsWith("/a "))
                {
                    text = text.Substring(3);
                    connection?.Send(Encoding.UTF32.GetBytes(text));
                }
                else if (text.StartsWith("/b "))
                {
                    text = text.Substring(3);
                    foreach (IrisConnection con in connections)
                    {
                        await con.Send(Encoding.UTF32.GetBytes(text));
                    }
                }
                else if (text == "/p")
                {
                    foreach (var addr in account.RawCommunications)
                    {
                        Console.WriteLine($"{addr.Key}: {(await account.PingService.Ping(addr.Key))?.ToString("s\\,fff")} sec.");
                    }
                }
                else if (text.StartsWith("/c "))
                {
                    text = text.Substring(3);
                    if (IrisAddress.TryParse(text, out IrisAddress address))
                    {
                        await account.IOIService.TryConnectTo(address);
                        var connection = account.CreateConnection((address, new Guid("00000001-0000-0000-0000-000000000000")));
                        connections.Add(connection);
                        connection.ConnectionInterrupted += Connection_ConnectionInterrupt;
                        connection.MessageReceived += Connection_MessageReceived;
                        await connection.Open();
                    }
                }
            }
        }

        static async void ClientPart(IrisAccount account)
        {
            if (account.RawCommunications.Count > 0)
            {
                connection = account.CreateConnection((account.RawCommunications.Keys.First(), new Guid("00000001-0000-0000-0000-000000000000")));
                connection.MessageReceived += Connection_MessageReceived;
                await connection.Open();
            }
        }

        static async void ServerPart(IrisAccount account)
        {
            listener = account.CreateConnectionListener(new Guid("00000001-0000-0000-0000-000000000000"));
            await listener.Start();
            listener.ConnectionReceived += Listener_ConnectionSocketReceived;
        }

        private static void Listener_ConnectionSocketReceived(IConnectionListener handler, IPointedConnection<IrisAddress, IrisPort> socket)
        {
            Console.WriteLine("Прослушиватель принял новое соединение от: {0}", socket.RemotePoint);
            IrisConnection connection = socket as IrisConnection;
            connections.Add(connection);
            connection.ConnectionInterrupted += Connection_ConnectionInterrupt;
            connection.MessageReceived += Connection_MessageReceived;
        }

        private static void Connection_MessageReceived(IConnection connection, byte[] message)
        {
            Console.WriteLine(Encoding.UTF32.GetString(message));
        }

        private static void Connection_ConnectionInterrupt(IConnection connection)
        {
            connections.Remove(connection as IrisConnection);
        }
    }
}
