using EmptyBox.IO.Network;
using EmptyBox.IO.Network.Help;
using EmptyBox.ScriptRuntime.Results;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Iris.Core.Network
{
    public sealed class IrisConnectionListener : APointedConnectionListener<IrisAddress, IrisPort, IrisAccount>
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

        public override async Task<bool> Start()
        {
            await Task.Yield();
            if (IsActive)
            {
                return false;
            }
            else if (ConnectionProvider.ListenerStarted(this))
            {
                IsActive = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        public override async Task<bool> Stop()
        {
            await Task.Yield();
            if (!IsActive)
            {
                ConnectionProvider.ListenerStopped(this);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
