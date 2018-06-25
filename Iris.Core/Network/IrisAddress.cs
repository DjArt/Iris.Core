using EmptyBox.IO.Network;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iris.Core.Network
{
    public struct IrisAddress : IAddress
    {
        public static bool operator ==(IrisAddress x, IrisAddress y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(IrisAddress x, IrisAddress y)
        {
            return !x.Equals(y);
        }

        public byte[] Key { get; private set; }

        public IrisAddress(byte[] key)
        {
            if (key.Length != 521)
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
                for (ushort i0 = 0; i0 < Key.Length; i0++)
                {
                    if (Key[i0] != address.Key[i0])
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }

        public override string ToString()
        {
            return Convert.ToBase64String(Key);
        }
    }
}
