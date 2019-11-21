using EmptyBox.IO.Network;
using Iris.Core.Network.Protocols.Servicing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

using static Iris.Core.Network.IrisAccountProvider;

namespace Iris.Core.Network.Protocols.Informative
{
    public class PingService
    {
        private struct PingPacket
        {
            public bool Way;
            public byte Check;
        }

        public static readonly IrisPort Port = new Guid("00000000-0000-0000-0001-000000000000");

        private IrisSocket Socket;
        private Random Random = new Random();

        public IrisAccount Account { get; }

        public PingService(IrisAccount account)
        {
            Account = account;
            Socket = Account.CreateSocket(Port);
            Socket.MessageReceived += Socket_MessageReceived;
        }

        private async void Socket_MessageReceived(IPointedSocket<IrisAddress, IrisPort> socket, IAccessPoint<IrisAddress, IrisPort> sender, byte[] message)
        {
            if (Serializer.TryDeserialize(message, out PingPacket packet) && !packet.Way)
            {
                packet.Way = true;
                await Socket.Send(sender, Serializer.Serialize(packet));
            }
        }

        public async void Start()
        {
            await Socket.Open();
        }

        public async void Stop()
        {
            await Socket.Close();
        }

        public async Task<TimeSpan?> Ping(IrisAddress address)
        {
            PingPacket packet = new PingPacket();
            packet.Check = (byte)Random.Next(255);
            IrisAccessPoint receiver = new IrisAccessPoint(address, Port);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            await Socket.Send(receiver, Serializer.Serialize(packet));
            byte[] answer = await Socket.WaitAnswer(x => x as IrisAccessPoint == receiver, x => Serializer.TryDeserialize(x, out PingPacket y) && y.Way && y.Check == packet.Check, Route.AnswerDelay);
            stopwatch.Stop();
            return answer == null ? null : new TimeSpan?(stopwatch.Elapsed);
        }
    }
}
