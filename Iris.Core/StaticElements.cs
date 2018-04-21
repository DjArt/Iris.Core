using EmptyBox.IO.Serializator;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iris.Core
{
    internal static class StaticElements
    {
        internal static BinarySerializer Serializer { get; private set; }

        static StaticElements()
        {
            Serializer = new BinarySerializer(Encoding.UTF32);
        }
    }
}
