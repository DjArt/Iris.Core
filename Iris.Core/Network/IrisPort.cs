using EmptyBox.IO.Network;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iris.Core.Network
{
    public struct IrisPort : IPort
    {
        public const uint PORT_LENGTH = 16;

        public static implicit operator IrisPort(Guid guid)
        {
            return new IrisPort(guid);
        }

        public static bool operator ==(IrisPort x, IrisPort y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(IrisPort x, IrisPort y)
        {
            return !x.Equals(y);
        }

        public Guid Value { get; private set; }

        public IrisPort(Guid value)
        {
            Value = value;
        }

        public override bool Equals(object obj)
        {
            if (obj is IrisPort port)
            {
                return Value.Equals(port.Value);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
