using EmptyBox.IO.Network;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Iris.Core.Network
{
    public sealed class IrisSocket : ISocket
    {
        public ISocketProvider SocketProvider => throw new NotImplementedException();

        public IPort Port => throw new NotImplementedException();

        public event SocketMessageReceiveHandler MessageReceived;

        public Task<SocketOperationStatus> Close()
        {
            throw new NotImplementedException();
        }

        public Task<SocketOperationStatus> Open()
        {
            throw new NotImplementedException();
        }

        public Task<SocketOperationStatus> Send(IAccessPoint host, byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}
