using EmptyBox.IO.Access;
using EmptyBox.IO.Devices;
using EmptyBox.IO.Devices.Bluetooth;
using EmptyBox.IO.Devices.Enumeration;
using EmptyBox.IO.Network;
using EmptyBox.IO.Network.Bluetooth;
using EmptyBox.IO.Network.IP;
using EmptyBox.Serializer;
using EmptyBox.ScriptRuntime;
using EmptyBox.ScriptRuntime.Results;
using Iris.Core.Network.Packets;
using Iris.Core.Network.Protocols.Servicing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmptyBox.IO.Network.Bluetooth.Classic;

namespace Iris.Core.Network
{
    public sealed class IrisAccountProvider
    {
        private static readonly Guid BLUETOOTH_SERVICE_ID = new Guid("44444444-0000-0000-0000-000000000000");

        public static readonly BinarySerializer Serializer = new BinarySerializer(Encoding.UTF32);

        private readonly List<IConnection> RawConnections = new List<IConnection>();
        private readonly List<IConnectionListener> RawConnectionListeners = new List<IConnectionListener>();
        private readonly List<IConnectionProvider> RawConnectionProviders = new List<IConnectionProvider>();
        private readonly List<ISocket> RawSockets = new List<ISocket>();
        private readonly List<ISocketProvider> RawSocketProviders = new List<ISocketProvider>();
        private readonly List<IDeviceSearcher<IDevice>> DeviceProviders = new List<IDeviceSearcher<IDevice>>();
        private readonly List<IrisAccount> Accounts = new List<IrisAccount>();
        private readonly uint STANDARD_HEADER_SIZE;

        public IrisAccountProvider()
        {
            STANDARD_HEADER_SIZE = Serializer.GetLength(new StandardHeader());
        }

        private async Task Initializate()
        {
            IDeviceEnumerator enumerator = DeviceEnumeratorProvider.Get();
            try
            {
                IBluetoothAdapter default_bluetooth = await enumerator.GetDefault<IBluetoothAdapter>();
                if (default_bluetooth != null)
                {
                    DeviceProviders.Add(default_bluetooth);
                    RawConnectionProviders.Add(default_bluetooth);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            #region Listeners init
            foreach (IConnectionProvider provider in RawConnectionProviders)
            {
                IConnectionListener listener = null;
                switch (provider)
                {
                    case IBluetoothAdapter btProvider:
                        listener = btProvider.CreateConnectionListener(new BluetoothPort(BLUETOOTH_SERVICE_ID));
                        break;
                    case ITCPConnectionProvider tcpProvider:
                        listener = tcpProvider.CreateConnectionListener(4444);
                        break;
                }
                RawConnectionListeners.Add(listener);
            }
            #endregion
            #region Searchers init
            foreach(IDeviceSearcher<IDevice> provider in DeviceProviders)
            {
                provider.DeviceFound += Provider_DeviceFound;
                provider.DeviceLost += Provider_DeviceLost;
                await provider.ActivateSearcher();
            }
            #endregion
        }

        private void Provider_DeviceLost(IDeviceSearcher<IDevice> provider, IDevice device)
        {
            
        }

        private async void Provider_DeviceFound(IDeviceSearcher<IDevice> provider, IDevice device)
        {
            switch (device)
            {
                case IBluetoothDevice btDevice:
                    IEnumerable<BluetoothClassicAccessPoint> services = await btDevice.GetClassicServices(BluetoothSDPCacheMode.Uncached);
                    BluetoothClassicAccessPoint irisService = services.FirstOrDefault(x => x.Port == BLUETOOTH_SERVICE_ID);
                    if (irisService.Address != null)
                    {
                        IConnection connection = (provider as IBluetoothAdapter).CreateConnection(irisService);
                        if (await connection.Open())
                        {
                            await AddConnection(connection);
                        }
                    }
                    break;
            }
        }

        public async Task<VoidResult<bool>> Start()
        {
            await Initializate();
            #region Listeners startup
            foreach (IConnectionListener listener in RawConnectionListeners)
            {
                listener.ConnectionReceived += Listener_ConnectionSocketReceived;
                await listener.Start();
            }
            foreach (ISocket socket in RawSockets)
            {
                socket.MessageReceived += Socket_MessageReceived; ;
            }
            #endregion
            return new VoidResult<bool>(true, null);
        }

        private void Socket_MessageReceived(ICommunicationElement socket, byte[] message)
        {
            throw new NotImplementedException();
        }

        private void Listener_ConnectionSocketReceived(IConnectionListener handler, IConnection socket)
        {
            RawConnections.Add(socket);
            socket.ConnectionInterrupted += Connection_ConnectionInterrupt;
            socket.MessageReceived += Connection_MessageReceived;
        }

        private void Connection_MessageReceived(ICommunicationElement connection, byte[] message)
        {
            if (message.Length > STANDARD_HEADER_SIZE && Serializer.TryDeserialize(message, out StandardHeader header))
            {
                if (header.Sender.Address != IrisAddress.MulticastAddress)
                {
                    if (header.Recipient.Address == IrisAddress.MulticastAddress)
                    {
                        if (header.Sender.Port == Route.Port && header.Recipient.Port == Route.Port && Serializer.TryDeserialize(header.Message, out Route.ServiceDetectionPacket packet))
                        {
                            StandardHeader answer = new StandardHeader()
                            {
                                IsConnection = true,
                                Sender = (Accounts.First().Address, Route.Port),
                                Recipient = header.Sender
                            };
                            connection.Send(Serializer.Serialize(answer));
                        }
                    }
                    else
                    {
                        IrisAccount account = Accounts.Find(x => x.Address == header.Recipient.Address);
                        if (account == null)
                        {
                            StandardHeader answer = new StandardHeader()
                            {
                                IsConnection = true,
                                Sender = new IrisAccessPoint(IrisAddress.MulticastAddress, Route.Port),
                                Recipient = header.Sender
                            };
                        }
                        else
                        {
                            account.MessageReceived(connection, header);
                        }
                    }
                }
            }
        }

        private void Connection_ConnectionInterrupt(IConnection connection)
        {
            RawConnections.Remove(connection);
        }

        public async Task<bool> AddConnection(IConnection connection)
        {
            connection.ConnectionInterrupted += Connection_ConnectionInterrupt;
            connection.MessageReceived += Connection_MessageReceived;
            StandardHeader question = new StandardHeader()
            {
                IsConnection = true,
                Sender = (Accounts.First().Address, Route.Port),
                Recipient = (IrisAddress.MulticastAddress, Route.Port),
                Message = Serializer.Serialize(new Route.ServiceDetectionPacket() { Purpose = Route.ServiceDetectionPurpose.Expansion })
            };
            if (await connection.Send(Serializer.Serialize(question)))
            {
                RawConnections.Add(connection);
                return true;
            }
            else
            {
                connection.ConnectionInterrupted -= Connection_ConnectionInterrupt;
                connection.MessageReceived -= Connection_MessageReceived;
                return false;
            }
        }

        internal void AddConnection2(IConnection socket)
        {
            RawConnections.Add(socket);
            socket.ConnectionInterrupted += Connection_ConnectionInterrupt;
            socket.MessageReceived += Connection_MessageReceived;
        }

        public void AddConnectionListener(IConnectionListener listener)
        {
            RawConnectionListeners.Add(listener);
        }

        public IrisAccount CreateAccount()
        {
            Random r = new Random();
            IrisAddress newAddress = new IrisAddress(new byte[IrisAddress.ADDRESS_LENGTH]);
            r.NextBytes(newAddress.Key);
            IrisAccount account = new IrisAccount(this, newAddress);
            Accounts.Add(account);
            return account;
        }

        public IrisAccount CreateAccount(IrisAddress address)
        {
            IrisAccount account = new IrisAccount(this, address);
            Accounts.Add(account);
            return account;
        }
    }
}
