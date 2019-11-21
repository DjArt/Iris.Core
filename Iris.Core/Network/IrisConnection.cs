using EmptyBox.IO.Network;
using EmptyBox.IO.Network.Help;
using EmptyBox.ScriptRuntime.Results;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Iris.Core.Network
{
    public sealed class IrisConnection : APointedConnection<IrisAddress, IrisPort, IrisAccessPoint, IrisAccount>
    {
        internal event MessageReceiveHandler<IrisConnection> MessageSended;

        internal bool FromListener { get; private set; }
        internal bool Interrupted { get; set; }

        public new event MessageReceiveHandler<IrisConnection> MessageReceived;
        public new event ConnectionInterruptHandler<IrisConnection> ConnectionInterrupted;

        internal IrisConnection(IrisAccount provider, IrisAccessPoint remoteHost)
        {
            ConnectionProvider = provider;
            RemotePoint = remoteHost;
            IsActive = false;
            FromListener = false;
        }

        internal IrisConnection(IrisAccount provider, IrisPort port, IrisAccessPoint remoteHost) : this(provider, remoteHost)
        {
            LocalPoint = new IrisAccessPoint(provider.Address, port);
            FromListener = true;
        }

        protected override void OnConnectionInterrupt()
        {
            ConnectionInterrupted?.Invoke(this);
            base.OnConnectionInterrupt();
        }

        protected override void OnMessageReceive(byte[] message)
        {
            MessageReceived?.Invoke(this, message);
            base.OnMessageReceive(message);
        }

        internal async void Interrupt()
        {
            Interrupted = true;
            await Close();
        }

        internal void Receive(byte[] message)
        {
            OnMessageReceive(message);
        }

        public override async Task<VoidResult<SocketOperationStatus>> Close()
        {
            OnConnectionInterrupt();
            ConnectionProvider.ConnectionClosed(this);
            return new VoidResult<SocketOperationStatus>(SocketOperationStatus.Success, null);
        }

        public override async Task<VoidResult<SocketOperationStatus>> Open()
        {
            await Task.Yield();
            if (await ConnectionProvider.ConnectionOpened(this))
            {
                IsActive = true;
                return new VoidResult<SocketOperationStatus>(SocketOperationStatus.Success, null);
            }
            else
            {
                return new VoidResult<SocketOperationStatus>(SocketOperationStatus.UnknownError, null);
            }
        }

        public override async Task<VoidResult<SocketOperationStatus>> Send(byte[] data)
        {
            await Task.Yield();
            MessageSended?.Invoke(this, data);
            return new VoidResult<SocketOperationStatus>(SocketOperationStatus.Success, null);
        }
    }
}
