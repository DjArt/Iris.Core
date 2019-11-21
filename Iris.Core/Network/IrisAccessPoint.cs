using EmptyBox.IO.Network;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iris.Core.Network
{
    public sealed class IrisAccessPoint : IAccessPoint<IrisAddress, IrisPort>
    {
        public static implicit operator IrisAccessPoint((IrisAddress Address, IrisPort Port) pair)
        {
            return new IrisAccessPoint(pair.Address, pair.Port);
        }

        public static implicit operator IrisAccessPoint(Tuple<IrisAddress, IrisPort> pair)
        {
            return new IrisAccessPoint(pair.Item1, pair.Item2);
        }

        public static bool operator ==(IrisAccessPoint x, IrisAccessPoint y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(IrisAccessPoint x, IrisAccessPoint y)
        {
            return !x.Equals(y);
        }

        public IrisAddress Address { get; set; }
        public IrisPort Port { get; set; }

        public IrisAccessPoint()
        {

        }

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
            return Address.ToString() + ':' + Port.ToString();
        }
    }
}
