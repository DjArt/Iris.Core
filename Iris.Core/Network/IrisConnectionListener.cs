using EmptyBox.IO.Network;
using EmptyBox.IO.Network.Help;
using EmptyBox.ScriptRuntime.Results;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Iris.Core.Network
{
    public sealed class IrisConnectionListener : APointedConnectionListener<IrisAddress, IrisPort, IrisAccessPoint, IrisAccount>
    {
        internal IrisConnectionListener(IrisAccount provider, IrisPort port)
        {
            ConnectionProvider = provider;
            ListenerPoint = new IrisAccessPoint(provider.Address, port);
            IsActive = false;
        }

        internal void TriggerConnectionReceived(IrisConnection connection)
        {
            OnConnectionReceive(connection);
        }

        public override async Task<VoidResult<SocketOperationStatus>> Start()
        {
            await Task.Yield();
            if (IsActive)
            {
                return new VoidResult<SocketOperationStatus>(SocketOperationStatus.ListenerIsAlreadyStarted, null);
            }
            else if (ConnectionProvider.ListenerStarted(this))
            {
                IsActive = true;
                return new VoidResult<SocketOperationStatus>(SocketOperationStatus.Success, null);
            }
            else
            {
                return new VoidResult<SocketOperationStatus>(SocketOperationStatus.ConnectionIsAlreadyOpen, null);
            }
        }

        public override async Task<VoidResult<SocketOperationStatus>> Stop()
        {
            await Task.Yield();
            if (!IsActive)
            {
                ConnectionProvider.ListenerStopped(this);
                return new VoidResult<SocketOperationStatus>(SocketOperationStatus.Success, null);
            }
            else
            {
                return new VoidResult<SocketOperationStatus>(SocketOperationStatus.ListenerIsAlreadyClosed, null);
            }
        }
    }
}
