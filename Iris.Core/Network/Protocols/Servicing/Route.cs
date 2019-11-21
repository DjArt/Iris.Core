using System;
using System.Collections.Generic;
using System.Text;

namespace Iris.Core.Network.Protocols.Servicing
{
    public class Route
    {
        public static readonly IrisPort Port = new Guid("00000000-0000-0000-0000-000000000000");
        public static readonly TimeSpan AnswerDelay = new TimeSpan(150000000);

        public enum ServiceDetectionPurpose : byte
        {
            Expansion = 1,
            Connection = 0,
        }

        public enum СonnectionEstablishmentStatus : byte
        {
            Allowed = 1,
            Closed = 0
        }

        public struct ServiceDetectionPacket
        {
            public ServiceDetectionPurpose Purpose { get; set; }
        }

        public struct СonnectionEstablishmentPacket
        {
            public IrisPort ConnectionPort { get; set; }
            public IrisPort RequesterPort { get; set; }
        }

        public struct СonnectionEstablishmentStatusPacket
        {
            public IrisPort? Port { get; set; }
            public IrisPort? RequesterPort { get; set; }
            public СonnectionEstablishmentStatus Status { get; set; }
        }
    }
}
