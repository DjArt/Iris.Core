using EmptyBox.IO.Access;
using EmptyBox.IO.Devices;
using EmptyBox.IO.Devices.Bluetooth;
using EmptyBox.IO.Network;
using EmptyBox.ScriptRuntime;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Iris.Core.Network
{
    public sealed class IrisAccountProvider
    {
        private readonly List<IConnection> RawConnections = new List<IConnection>();
        private readonly List<IConnectionProvider> RawConnectionProviders = new List<IConnectionProvider>();
        private readonly List<ISocket> RawSockets = new List<ISocket>();
        private readonly List<ISocketProvider> RawSocketProviders = new List<ISocketProvider>();
        private readonly List<IDeviceProvider<IDevice>> DeviceProviders = new List<IDeviceProvider<IDevice>>();

        private IrisAccountProvider()
        {
            async Task Initializate()
            {
                RefResult<IBluetoothAdapter, AccessStatus> default_bluetooth = await BluetoothAdapterProvider.GetDefault();
                if (default_bluetooth.Status == AccessStatus.Success)
                {
                    IBluetoothAdapter adapter = default_bluetooth.Result;
                    if (adapter.LEDeviceProvider != null)
                    {
                        DeviceProviders.Add(adapter.LEDeviceProvider);
                    }
                }
            }

            Initializate().Wait();
        }

        private void RawConnection_MessageReceived(IConnection connection, byte[] message)
        {
            throw new NotImplementedException();
        }

        private void RawConnection_ConnectionInterrupt(IConnection connection)
        {
            throw new NotImplementedException();
        }

        public async Task<VoidResult<bool>> Start()
        {

        }
    }
}
