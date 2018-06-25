using EmptyBox.IO.Network;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iris.Core.Network
{
    public struct IrisAccessPoint : IAccessPoint
    {
        public static implicit operator IrisAccessPoint((IrisAddress Address, IrisPort Port) pair)
        {
            return new IrisAccessPoint(pair.Address, pair.Port);
        }

        public static implicit operator IrisAccessPoint(Tuple<IrisAddress, IrisPort> pair)
        {
            return new IrisAccessPoint(pair.Item1, pair.Item2);
        }

        public IrisAddress Address { get; private set; }
        public IrisPort Port { get; private set; }

        IAddress IAccessPoint.Address => Address;
        IPort IAccessPoint.Port => Port;

        public IrisAccessPoint(IrisAddress address, IrisPort port)
        {
            Address = address;
            Port = port;
        }

        public override bool Equals(object obj)
        {
            if (obj is IrisAccessPoint point)
            {
                return point.Port == Port && point.Address == Address;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return Address.GetHashCode() ^ Port.GetHashCode();
        }

        public override string ToString()
        {
            return Address.ToString() ;
        }
    }
}
