using EmptyBox.IO.Network;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Iris.Core.Network
{
    public sealed class IrisConnection : IConnection
    {
        internal event ConnectionMessageReceiveHandler MessageSended;

        public IConnectionProvider ConnectionProvider => throw new NotImplementedException();

        public IPort Port => throw new NotImplementedException();

        public IAccessPoint RemoteHost => throw new NotImplementedException();

        public bool IsActive => throw new NotImplementedException();

        public event ConnectionMessageReceiveHandler MessageReceived;
        public event ConnectionInterruptHandler ConnectionInterrupt;

        internal void Interrupt()
        {
            ConnectionInterrupt?.Invoke(this);
        }

        internal void Receive(byte[] message)
        {
            MessageReceived?.Invoke(this, message);
        }

        public Task<SocketOperationStatus> Close()
        {
            throw new NotImplementedException();
        }

        public Task<SocketOperationStatus> Open()
        {
            throw new NotImplementedException();
        }

        public async Task<SocketOperationStatus> Send(byte[] data)
        {
            await Task.Yield();
            MessageSended?.Invoke(this, data);
            return SocketOperationStatus.Success;
        }
    }
}
