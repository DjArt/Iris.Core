using EmptyBox.IO.Network;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Iris.Core.Network
{
    public struct IrisAddress : IAddress, IEquatable<IrisAddress>
    {
        public const ushort ADDRESS_LENGTH = 66;
        public static IrisAddress MulticastAddress { get; } = new IrisAddress(new byte[ADDRESS_LENGTH]);

        public static bool operator ==(IrisAddress x, IrisAddress y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(IrisAddress x, IrisAddress y)
        {
            return !x.Equals(y);
        }

        public static bool TryParse(string str, out IrisAddress address)
        {
            byte[] key = Convert.FromBase64String(str);
            if (key.Length == ADDRESS_LENGTH)
            {
                address = new IrisAddress(key);
                return true;
            }
            else
            {
                address = default;
                return false;
            }
        }

        public byte[] Key { get; private set; }

        public IrisAddress(byte[] key)
        {
            if (key.Length != ADDRESS_LENGTH)
            {
                throw new ArgumentOutOfRangeException();
            }
            else
            {
                Key = key;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is IrisAddress address)
            {
                return Equals(address);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return Key.Aggregate(0, (x, y) => x ^ y);
        }

        public override string ToString()
        {
            return Convert.ToBase64String(Key);
        }

        public bool Equals(IrisAddress other)
        {
            for (ushort i0 = 0; i0 < Key.Length; i0++)
            {
                if (Key[i0] != other.Key[i0])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
