using EmptyBox.IO.Network;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iris.Core.Network
{
    public sealed class IrisAccount : IConnectionProvider, ISocketProvider
    {
        private readonly List<IrisConnection> Connections = new List<IrisConnection>();
        private readonly IrisAccountProvider Provider;
        private readonly Random Random = new Random();

        IAddress IConnectionProvider.Address => Address;
        IAddress ISocketProvider.Address => Address;

        public IrisAddress Address { get; private set; }

        internal IrisAccount(IrisAccountProvider provider, IrisAddress address)
        {
            Provider = provider;
            Address = address;
        }

        private bool PortIsUsed(IrisPort port)
        {
            return false;
        }

        private IrisPort SelectRandomPort()
        {
            byte[] randomPart = new byte[8];
            Random.NextBytes(randomPart);
            return new Guid(0, 0, -1, randomPart);
        }

        IConnection IConnectionProvider.CreateConnection(IAccessPoint accessPoint)
        {
            if (accessPoint is IrisAccessPoint _accessPoint)
            {
                return CreateConnection(_accessPoint);
            }
            else
            {
                throw new ArgumentException();
            }
        }

        IConnectionListener IConnectionProvider.CreateConnectionListener(IPort port)
        {
            if (port is IrisPort _port)
            {
                throw new NotImplementedException();
            }
            else
            {
                throw new ArgumentException();
            }
        }

        ISocket ISocketProvider.CreateSocket(IPort port)
        {
            if (port is IrisPort _port)
            {
                return CreateSocket(_port);
            }
            else
            {
                throw new ArgumentException();
            }
        }

        public IrisConnection CreateConnection(IrisAccessPoint accessPoint)
        {
            throw new NotImplementedException();
        }

        public IrisSocket CreateSocket(IrisPort port)
        {
            throw new NotImplementedException();
        }
    }
}
