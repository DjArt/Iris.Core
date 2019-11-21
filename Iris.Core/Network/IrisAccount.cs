using EmptyBox.IO.Network;
using Iris.Core.Network.Packets;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Iris.Core.Network.Protocols.Servicing;
using Iris.Core.Network.Protocols.Informative;
using System.Threading.Tasks;

using static Iris.Core.Network.IrisAccountProvider;

namespace Iris.Core.Network
{
    public sealed class IrisAccount : IPointedConnectionProvider<IrisAddress, IrisPort>, IPointedSocketProvider<IrisAddress, IrisPort>
    {
        public readonly List<IrisConnection> Connections = new List<IrisConnection>();
        public readonly List<IrisSocket> Sockets = new List<IrisSocket>();
        public readonly Dictionary<IrisAddress, List<ICommunicationElement>> RawCommunications = new Dictionary<IrisAddress, List<ICommunicationElement>>();
        private readonly List<IrisConnectionListener> ConnectionListeners = new List<IrisConnectionListener>();
        internal readonly IrisAccountProvider Provider;
        private readonly Random Random = new Random();

        public IrisAddress Address { get; private set; }

        public IrisOverIris IOIService { get; }
        public PingService PingService { get; }

        internal IrisAccount(IrisAccountProvider provider, IrisAddress address)
        {
            Provider = provider;
            Address = address;
            IOIService = new IrisOverIris(this);
            PingService = new PingService(this);
            IOIService.Start();
            PingService.Start();
        }

        internal bool ListenerStarted(IrisConnectionListener listener)
        {
            if (!ConnectionListeners.Any(x => x.ListenerPoint == listener.ListenerPoint))
            {
                ConnectionListeners.Add(listener);
                return true;
            }
            else
            {
                return false;
            }
        }

        internal void ListenerStopped(IrisConnectionListener listener)
        {
            if (ConnectionListeners.Contains(listener))
            {
                ConnectionListeners.Remove(listener);
            }
        }

        internal void SocketClosed(IrisSocket socket)
        {
            if (Sockets.Contains(socket))
            {
                Sockets.Remove(socket);
            }
        }

        internal bool SocketOpened(IrisSocket socket)
        {
            if (!Sockets.Contains(socket))
            {
                if (!Sockets.Any(x => x.LocalPoint == socket.LocalPoint))
                {
                    Sockets.Add(socket);
                    socket.MessageSended += Socket_MessageSended;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        internal async Task<bool> ConnectionOpened(IrisConnection connection)
        {
            if (!Connections.Contains(connection))
            {
                if (connection.FromListener)
                {
                    Connections.Add(connection);
                    return true;
                }
                else
                {
                    if (RawCommunications.ContainsKey(connection.RemotePoint.Address) || await IOIService.TryConnectTo(connection.RemotePoint.Address) != null)
                    {
                        IrisPort requesterPort = SelectRandomPort();
                        StandardHeader question = new StandardHeader()
                        {
                            IsConnection = true,
                            Recipient = (connection.RemotePoint.Address, Route.Port),
                            Sender = (Address, Route.Port),
                            Message = Serializer.Serialize(new Route.СonnectionEstablishmentPacket() { ConnectionPort = connection.RemotePoint.Port, RequesterPort = requesterPort })
                        };
                        ICommunicationElement randomComm = RawCommunications[connection.RemotePoint.Address][Random.Next(RawCommunications[connection.RemotePoint.Address].Count)];
                        await randomComm.Send(Serializer.Serialize(question));

                        bool СonnectionEstablishmentAnswerCheck(byte[] message, out Route.СonnectionEstablishmentStatusPacket answer)
                        {
                            if (Serializer.TryDeserialize(message, out StandardHeader header))
                            {
                                if (header.IsConnection && header.Recipient.Port == Route.Port && header.Sender.Port == Route.Port)
                                {
                                    if (Serializer.TryDeserialize(header.Message, out answer))
                                    {
                                        return answer.RequesterPort == requesterPort && answer.Port == connection.RemotePoint.Port;
                                    }
                                }
                            }
                            answer = default;
                            return false;
                        }

                        var answer = await randomComm.WaitAnswer<Route.СonnectionEstablishmentStatusPacket>(СonnectionEstablishmentAnswerCheck, Route.AnswerDelay);
                        if (answer.Success && answer.Value.Status == Route.СonnectionEstablishmentStatus.Allowed)
                        {
                            connection.LocalPoint = new IrisAccessPoint(Address, requesterPort);
                            Connections.Add(connection);
                            Console.WriteLine("Создано соединение с {0}", connection.RemotePoint);
                            connection.MessageSended += Connection_MessageSended;
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
        }

        private void Connection_MessageSended(IrisConnection connection, byte[] message)
        {
            StandardHeader msg = new StandardHeader()
            {
                Message = message,
                Sender = connection.LocalPoint,
                Recipient = connection.RemotePoint,
                IsConnection = true
            };
            RawCommunications[msg.Recipient.Address][Random.Next(RawCommunications[msg.Recipient.Address].Count)].Send(Serializer.Serialize(msg));
        }

        private async void Socket_MessageSended(IPointedSocket<IrisAddress, IrisPort> socket, IAccessPoint<IrisAddress, IrisPort> receiver, byte[] message)
        {
            if (RawCommunications.ContainsKey(receiver.Address) || await IOIService.TryConnectTo(receiver.Address) != null)
            {
                StandardHeader msg = new StandardHeader()
                {
                    Message = message,
                    Sender = socket.LocalPoint as IrisAccessPoint,
                    Recipient = receiver as IrisAccessPoint,
                    IsConnection = false
                };
                await RawCommunications[msg.Recipient.Address][Random.Next(RawCommunications[msg.Recipient.Address].Count)].Send(Serializer.Serialize(msg));
            }
        }

        internal async void ConnectionClosed(IrisConnection connection)
        {
            Connections.Remove(connection);
            connection.MessageSended -= Connection_MessageSended;
            if (!connection.Interrupted)
            {
                StandardHeader answer = new StandardHeader()
                {
                    IsConnection = true,
                    Recipient = new IrisAccessPoint(connection.RemotePoint.Address, Route.Port),
                    Sender = new IrisAccessPoint(connection.LocalPoint.Address, Route.Port),
                    Message = Serializer.Serialize(new Route.СonnectionEstablishmentStatusPacket() { Port = connection.RemotePoint.Port, RequesterPort = connection.LocalPoint.Port, Status = Route.СonnectionEstablishmentStatus.Closed })
                };
                await connection.Send(Serializer.Serialize(answer));
            }
        }

        internal async void MessageReceived(ICommunicationElement connection, StandardHeader header)
        {
            if (header.IsConnection)
            {
                if (header.Recipient.Port == Route.Port && header.Sender.Port == Route.Port)
                {
                    if (header.Message == null)
                    {
                        if (!RawCommunications.ContainsKey(header.Sender.Address))
                        {
                            RawCommunications[header.Sender.Address] = new List<ICommunicationElement>();
                        }
                        if (!RawCommunications[header.Sender.Address].Contains(connection))
                        {
                            RawCommunications[header.Sender.Address].Add(connection);
                            if (connection is IConnection _connection)
                            {
                                _connection.ConnectionInterrupted += RawConnection_ConnectionInterrupt;
                            }
                            StandardHeader answer = new StandardHeader()
                            {
                                Sender = (Address, Route.Port),
                                Recipient = header.Sender,
                                IsConnection = true
                            };
                            await connection.Send(Serializer.Serialize(answer));
                        }
                    }
                    else if (Serializer.TryDeserialize(header.Message, out Route.СonnectionEstablishmentPacket packet0))
                    {
                        IrisConnectionListener listener = ConnectionListeners.Find(x => x.ListenerPoint.Port == packet0.ConnectionPort);
                        if (listener != null)
                        {
                            IrisConnection irisConnection = new IrisConnection(this, listener.ListenerPoint.Port, (header.Sender.Address, packet0.RequesterPort));
                            await irisConnection.Open();
                            StandardHeader answer = new StandardHeader()
                            {
                                IsConnection = true,
                                Recipient = header.Sender,
                                Sender = header.Recipient,
                                Message = Serializer.Serialize(new Route.СonnectionEstablishmentStatusPacket()
                                {
                                    Port = listener.ListenerPoint.Port,
                                    RequesterPort = packet0.RequesterPort,
                                    Status = Route.СonnectionEstablishmentStatus.Allowed
                                })
                            };
                            await connection.Send(Serializer.Serialize(answer));
                            irisConnection.MessageSended += Connection_MessageSended;
                            Connections.Add(irisConnection);
                            listener.TriggerConnectionReceived(irisConnection);
                        }
                        else
                        {
                            StandardHeader answer = new StandardHeader()
                            {
                                IsConnection = true,
                                Recipient = header.Sender,
                                Sender = header.Recipient,
                                Message = Serializer.Serialize(new Route.СonnectionEstablishmentStatusPacket()
                                {
                                    Port = packet0.ConnectionPort,
                                    RequesterPort = packet0.RequesterPort,
                                    Status = Route.СonnectionEstablishmentStatus.Closed
                                })
                            };
                            await connection.Send(Serializer.Serialize(answer));
                        }
                    }
                    else if (Serializer.TryDeserialize(header.Message, out Route.СonnectionEstablishmentStatusPacket packet1) && packet1.Status == Route.СonnectionEstablishmentStatus.Closed)
                    {
                        IrisConnection _connection = Connections.Find(x => x.LocalPoint.Port == packet1.Port && x.RemotePoint.Port == packet1.RequesterPort);
                        _connection?.Interrupt();
                    }
                }
                else
                {
                    IrisConnection irisConnection = Connections.Find(x => x.LocalPoint.Port == header.Recipient.Port && x.RemotePoint == header.Sender);
                    if (irisConnection != null)
                    {
                        irisConnection.Receive(header.Message);
                    }
                }
            }
            else
            {
                IrisSocket socket = Sockets.Find(x => x.LocalPoint == header.Recipient);
                if (socket != null)
                {
                    socket.Receive(header.Sender, header.Message);
                }
            }
        }

        private bool PortIsUsed(IrisPort port)
        {
            return false;
        }

        internal IrisPort SelectRandomPort()
        {
            byte[] randomPart = new byte[8];
            Random.NextBytes(randomPart);
            return new Guid(0, 0, -1, randomPart);
        }

        private void RawConnection_ConnectionInterrupt(IConnection connection)
        {
            connection.ConnectionInterrupted -= RawConnection_ConnectionInterrupt;
            List<KeyValuePair<IrisAddress, List<ICommunicationElement>>> lists = RawCommunications.Where(x => x.Value.Contains(connection)).ToList();
            foreach(KeyValuePair<IrisAddress, List<ICommunicationElement>> list in lists)
            {
                list.Value.Remove(connection);
                if (list.Value.Count == 0)
                {
                    IEnumerable<IrisConnection> interrupted = Connections.Where(x => x.RemotePoint.Address == list.Key).ToList();
                    foreach(IrisConnection irisConnection in interrupted)
                    {
                        irisConnection.Interrupt();
                    }
                    RawCommunications.Remove(list.Key);
                }
            }
        }

        public IEnumerable<IrisAddress> GetLocalArea()
        {
            return RawCommunications.Keys.ToList();
        }

        public IrisConnection CreateConnection(IrisAccessPoint accessPoint)
        {
            return new IrisConnection(this, accessPoint);
        }

        public IrisConnectionListener CreateConnectionListener(IrisPort port)
        {
            return new IrisConnectionListener(this, port);
        }

        public IrisSocket CreateSocket(IrisPort port)
        {
            return new IrisSocket(this, port);
        }

        IPointedConnection<IrisAddress, IrisPort> IPointedConnectionProvider<IrisAddress, IrisPort>.CreateConnection(IAccessPoint<IAddress, IPort> accessPoint)
        {
            if (accessPoint is IAccessPoint<IrisAddress, IrisPort> _accessPoint && _accessPoint is IrisAccessPoint __accessPoint)
            {
                return CreateConnection(__accessPoint);
            }
            else
            {
                throw new ArgumentException();
            }
        }

        IPointedConnectionListener<IrisAddress, IrisPort> IPointedConnectionProvider<IrisAddress, IrisPort>.CreateConnectionListener(IPort port)
        {
            if (port is IrisPort _port)
            {
                return CreateConnectionListener(_port);
            }
            else
            {
                throw new ArgumentException();
            }
        }

        IPointedConnection<IrisAddress> IPointedConnectionProvider<IrisAddress>.CreateConnection(IAddress address)
        {
            throw new NotSupportedException();
        }

        IPointedConnectionListener<IrisAddress> IPointedConnectionProvider<IrisAddress>.CreateConnectionListener()
        {
            throw new NotSupportedException();
        }

        IConnection<IrisPort> IConnectionProvider<IrisPort>.CreateConnection(IPort port)
        {
            throw new NotSupportedException();
        }

        IConnectionListener<IrisPort> IConnectionProvider<IrisPort>.CreateConnectionListener(IPort port)
        {
            if (port is IrisPort _port)
            {
                return CreateConnectionListener(_port);
            }
            else
            {
                throw new ArgumentException();
            }
        }

        IConnection IConnectionProvider.CreateConnection()
        {
            throw new NotSupportedException();
        }

        IConnectionListener IConnectionProvider.CreateConnectionListener()
        {
            throw new NotSupportedException();
        }

        IPointedSocket<IrisAddress, IrisPort> IPointedSocketProvider<IrisAddress, IrisPort>.CreateSocket(IPort port)
        {
            if (port is IrisPort _port)
            {
                return CreateSocket(_port);
            }
            else
            {
                throw new ArgumentException();
            }
        }

        IPointedSocket<IrisAddress> IPointedSocketProvider<IrisAddress>.CreateSocket()
        {
            throw new NotSupportedException();
        }

        ISocket<IrisPort> ISocketProvider<IrisPort>.CreateSocket(IPort port)
        {
            if (port is IrisPort _port)
            {
                return CreateSocket(_port);
            }
            else
            {
                throw new ArgumentException();
            }
        }

        ISocket ISocketProvider.CreateSocket()
        {
            throw new NotSupportedException();
        }
    }
}
