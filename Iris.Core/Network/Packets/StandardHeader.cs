using System;
using System.Collections.Generic;
using System.Text;

namespace Iris.Core.Network.Packets
{
    public struct StandardHeader
    {
        public bool IsConnection;
        public IrisAccessPoint Sender;
        public IrisAccessPoint Recipient;
        public byte[] Message;
    }
}
