using System;
using System.Collections.Generic;
using System.Text;
using EmptyBox.IO.Network;
using System.Security.Cryptography;

namespace Iris.Core.Network
{
    public struct IrisAddress : IAddress
    {
        public ECDiffieHellmanPublicKey Address;
    }
}
