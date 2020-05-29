using System;
using System.ServiceModel;

namespace Intersoft.CISSA.UserApp.Utils
{
    public class ConnectionClient<TProxy, TChannel>: IDisposable 
        where TProxy: ClientBase<TChannel>, new()
        where TChannel : class

    {
        private TProxy _proxy;
        public TProxy Proxy
        {
            get
            {
                if (_proxy != null) return _proxy;
                
                throw new ObjectDisposedException("ConnectionClient");
            }
        }

        public ConnectionClient(TProxy proxy)
        {
            _proxy = proxy;
        }

        /// <summary>
        /// Disposes of this instance.
        /// </summary>
        public void Dispose()
        {
            DisposeProxy();
        }

        protected void DisposeProxy()
        {
            if (_proxy != null)
                try
                {
                    if (_proxy.State != CommunicationState.Faulted)
                    {
                        _proxy.Close();
                    }
                    else
                    {
                        _proxy.Abort();
                    }
                }
                catch (CommunicationException)
                {
                    _proxy.Abort();
                }
                catch (TimeoutException)
                {
                    _proxy.Abort();
                }
                catch (Exception)
                {
                    _proxy.Abort();
                    throw;
                }
                finally
                {
                    _proxy = null;
                }
        }

        ~ConnectionClient()
        {
            DisposeProxy();
        }
    }
}