using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using EmptyBox.IO.Network;

namespace Iris.Core.Network
{
    public class IrisProvider : IConnectionProvider
    {
        public IrisProvider()
        {
            IrisAddress a = new IrisAddress();
        }

        IAddress IConnectionProvider.Address => throw new NotImplementedException();

        IConnection IConnectionProvider.CreateConnection(IAccessPoint accessPoint)
        {
            throw new NotImplementedException();
        }

        IConnectionListener IConnectionProvider.CreateConnectionListener(IPort port)
        {
            throw new NotImplementedException();
        }
    }
}
