using EmptyBox.IO.Network;
using EmptyBox.IO.Network.Help;
using EmptyBox.ScriptRuntime.Results;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Iris.Core.Network
{
    public sealed class IrisSocket : APointedSocket<IrisAddress, IrisPort, IrisAccessPoint, IrisAccount>
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

        public async override Task<VoidResult<SocketOperationStatus>> Close()
        {
            IsActive = false;
            SocketProvider.SocketClosed(this);
            return new VoidResult<SocketOperationStatus>(SocketOperationStatus.Success, null);
        }

        public async override Task<VoidResult<SocketOperationStatus>> Open()
        {
            await Task.Yield();
            if (SocketProvider.SocketOpened(this))
            {
                IsActive = true;
                return new VoidResult<SocketOperationStatus>(SocketOperationStatus.Success, null);
            }
            else
            {
                return new VoidResult<SocketOperationStatus>(SocketOperationStatus.UnknownError, null);
            }
        }

        public async Task<VoidResult<SocketOperationStatus>> Send(IAccessPoint<IrisAddress, IrisPort> receiver, byte[] data)
        {
            await Task.Yield();
            MessageSended?.Invoke(this, receiver, data);
            return new VoidResult<SocketOperationStatus>(SocketOperationStatus.Success, null);
        }

        public async override Task<VoidResult<SocketOperationStatus>> Send(IAccessPoint<IAddress, IPort> receiver, byte[] data)
        {
            return await Send(receiver as IAccessPoint<IrisAddress, IrisPort>, data);
        }
    }
}
