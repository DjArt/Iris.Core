using EmptyBox.IO.Network;
using EmptyBox.ScriptRuntime.Results;
using EmptyBox.Serializer;
using Iris.Core.Network.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Iris.Core.Network.IrisAccountProvider;

namespace Iris.Core.Network.Protocols.Servicing
{
    public class IrisOverIris
    {
        private struct RelayEndpointRequestPacket
        {
            public IrisAddress Requester;
        }

        private struct RelayRequestPacket
        {
            public IrisAddress RemotePoint;
            public IEnumerable<IrisAddress> IgnoreList;
            public byte TTL;
        }

        private struct RelayRequestAnswerPacket
        {
            public IrisAddress RemotePoint;
            public byte? TTL;
        }

        private const byte MAX_TTL = 3;

        public static IrisPort Port => new Guid("00000000-0000-0000-0000-000000000001");

        private readonly IrisConnectionListener Listener;
        private readonly Dictionary<IrisConnection, IrisConnection> Connections = new Dictionary<IrisConnection, IrisConnection>();

        public IrisAccount Account { get; }

        public IrisOverIris(IrisAccount account)
        {
            Account = account;
            Listener = new IrisConnectionListener(Account, Port);
            Listener.ConnectionReceived += Listener_ConnectionReceived;
        }

        private void Listener_ConnectionReceived(IPointedConnectionListener<IrisAddress, IrisPort> handler, IPointedConnection<IrisAddress, IrisPort> connection)
        {
            IrisConnection con = connection as IrisConnection;
            Connections.Add(con, null);
            con.ConnectionInterrupted += Con_ConnectionInterrupted;
            con.MessageReceived += Con_MessageReceived;
        }

        private async void Con_MessageReceived(IrisConnection socket, byte[] message)
        {
            if (Serializer.TryDeserialize(message, out RelayRequestPacket requestPacket))
            {
                if (Connections[socket] == null)
                {
                    byte? metric = 0;
                    if (!Account.GetLocalArea().Contains(requestPacket.RemotePoint))
                    {
                        requestPacket.IgnoreList = requestPacket.IgnoreList == null ? new[] { socket.RemotePoint.Address } : requestPacket.IgnoreList.Append(socket.RemotePoint.Address);
                        metric = requestPacket.TTL > 0 ? await TryConnectTo(requestPacket.RemotePoint, (byte)(requestPacket.TTL - 1), requestPacket.IgnoreList) : null;
                    }
                    if (metric != null)
                    {
                        Connections[socket] = Account.CreateConnection(new IrisAccessPoint(requestPacket.RemotePoint, Port));
                        Connections[socket].ConnectionInterrupted += Con_ConnectionInterrupted1;
                        Connections[socket].MessageReceived += Con_MessageReceived1;
                        await Connections[socket].Open();
                        RelayEndpointRequestPacket packet = new RelayEndpointRequestPacket() { Requester = socket.RemotePoint.Address };
                        await Connections[socket].Send(Serializer.Serialize(packet));
                    }
                    RelayRequestAnswerPacket answer = new RelayRequestAnswerPacket()
                    {
                        RemotePoint = requestPacket.RemotePoint,
                        TTL = metric
                    };
                    await socket.Send(Serializer.Serialize(answer));
                }
            }
            else if (Serializer.TryDeserialize(message, out RelayEndpointRequestPacket endpointRequestPacket))
            {
                Account.Provider.AddConnection2(socket);
            }
            else if (Serializer.TryDeserialize(message, out RelayRequestAnswerPacket requestAnswerPacket))
            {

            }
            else if (Serializer.TryDeserialize(message, out StandardHeader header))
            {
                if (header.Sender.Address == socket.RemotePoint.Address)
                {
                    if (Connections[socket] != null)
                    {
                        await Connections[socket].Send(message);
                    }
                }
            }
        }

        private async void Con_MessageReceived1(IrisConnection socket, byte[] message)
        {
            IrisConnection con = Connections.FirstOrDefault(x => x.Value == socket).Key;
            if (con != null)
            {
                await con.Send(message);
            }
        }

        private void Con_ConnectionInterrupted1(IrisConnection connection)
        {
            connection.ConnectionInterrupted -= Con_ConnectionInterrupted1;
            connection.MessageReceived -= Con_MessageReceived1;
            IrisConnection con = Connections.FirstOrDefault(x => x.Value == connection).Key;
            if (con != null)
            {
                Connections[con] = null;
            }
        }

        private void Con_ConnectionInterrupted(IrisConnection connection)
        {
            Connections.Remove(connection);
        }

        public async void Start()
        {
            await Listener.Start();
        }

        public async Task<byte?> TryConnectTo(IrisAddress address, byte ttl = MAX_TTL, IEnumerable<IrisAddress> ignoreList = null)
        {
            IEnumerable<IrisAddress> localArea = Account.GetLocalArea();
            if (ignoreList != null)
            {
                localArea = localArea.Except(ignoreList);
            }
            Dictionary<IrisConnection, byte> paths = new Dictionary<IrisConnection, byte>();
            foreach (IrisAddress local in localArea)
            {
                bool CheckAnswer(byte[] @in, out RelayRequestAnswerPacket @out)
                {
                    return Serializer.TryDeserialize(@in, out @out) && @out.RemotePoint == address;
                }

                IrisConnection conn = Account.CreateConnection(new IrisAccessPoint(local, Port));
                if (await conn.Open())
                {
                    RelayRequestPacket requset = new RelayRequestPacket()
                    {
                        RemotePoint = address,
                        IgnoreList = localArea.Where(x => x != local),
                        TTL = ttl
                    };
                    Task<(RelayRequestAnswerPacket Value, bool Success)> answerTask = Task.Run(() => conn.WaitAnswer<RelayRequestAnswerPacket>(CheckAnswer, Route.AnswerDelay + Route.AnswerDelay));
                    if (await conn.Send(Serializer.Serialize(requset)))
                    {
                        var answer = await answerTask;
                        if (answer.Success && answer.Value.TTL != null)
                        {
                            paths.Add(conn, answer.Value.TTL.Value);
                        }
                        else
                        {
                            await conn.Close();
                        }
                    }
                }
            }
            if (paths.Count > 0)
            {
                byte min = paths.Min(x => x.Value);
                foreach (IrisConnection con in paths.Where(x => x.Value > min).Select(x => x.Key))
                {
                    await con.Close();
                }
                foreach (IrisConnection con in paths.Where(x => x.Value == min).Select(x => x.Key))
                {
                    await Account.Provider.AddConnection(con);
                }
                return min;
            }
            else
            {
                return null;
            }
        }
    }
}
