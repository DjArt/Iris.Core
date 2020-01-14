using EmptyBox.IO.Network;
using EmptyBox.IO.Network.Help;
using EmptyBox.ScriptRuntime.Results;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Iris.Core.Network
{
    public sealed class IrisSocket : APointedSocket<IrisAddress, IrisPort, IrisAccount>
    {
        internal event PointedSocketMessageReceiveHandler<IrisAddress, IrisPort> MessageSended;

        internal IrisSocket(IrisAccount provider, IrisPort port)
        {
            SocketProvider = provider;
            LocalPoint = new IrisAccessPoint(provider.Address, port);
        }

        internal void Receive(IrisAccessPoint sender, byte[] message)
        {
            OnMessageReceive(sender, message);
        }

        public async override Task<bool> Close()
        {
            IsActive = false;
            SocketProvider.SocketClosed(this);
            return true;
        }

        public async override Task<bool> Open()
        {
            await Task.Yield();
            if (SocketProvider.SocketOpened(this))
            {
                IsActive = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> Send(IAccessPoint<IrisAddress, IrisPort> receiver, byte[] data)
        {
            await Task.Yield();
            MessageSended?.Invoke(this, receiver, data);
            return true;
        }

        public async override Task<bool> Send(IAccessPoint<IAddress, IPort> receiver, byte[] data)
        {
            return await Send(receiver as IAccessPoint<IrisAddress, IrisPort>, data);
        }
    }
}
