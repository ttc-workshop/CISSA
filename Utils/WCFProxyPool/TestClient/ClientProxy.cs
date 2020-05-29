using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.ChannelPool;

namespace TestServiceClient
{
    public class ClientProxy : ClientBase<TestServiceInterface.IService1>, TestServiceInterface.IService1
    {
        #region IService1 Members

        public string MyOperation1(string myValue)
        {
            return Channel.MyOperation1(myValue);
        }

        public string MyOperation2(TestServiceInterface.DataContract1 dataContractValue)
        {
            return Channel.MyOperation2(dataContractValue);
        }

        #endregion
    }

    public class ClientProxy2 : ClientBase<TestServiceInterface.IService2>, TestServiceInterface.IService2
    {
        #region IService2 Members

        public string Op1(string myValue)
        {
            return Channel.Op1(myValue);
        }

        public string Op2()
        {
            return Channel.Op2();
        }

        #endregion
    }

    // My sexy new proxy base class that uses a ChannelPool with pre-opened channels to speed things along
    public class ClientBasePoolProxy : ClientBasePool<TestServiceInterface.IService2>, TestServiceInterface.IService2
    {
        #region IService2 Members

        public string Op1(string myValue)
        {
            try
            {
                return Channel.Op1(myValue);
            } catch (Exception ex)
            {
                string msg = string.Format("Exception raised in proxy. Current time:{0}, Channel Opened Time: {1} :\n\t{2}\n]t{3}\n\n",DateTime.Now.ToShortTimeString(),
                    this.ChannelContext.DateTimeOpened.ToShortTimeString(), 
                    ex.Message,
                    ex.InnerException.Message ?? "");
                Console.WriteLine(msg);
                System.Diagnostics.Debug.WriteLine(msg);
                return null;
            }
        }

        public string Op2()
        {
            return Channel.Op2();
        }

        #endregion
    }


    // My sexy new proxy base class that uses a ChannelPool with pre-opened channels to speed things along
    public class ClientBasePoolProxyPart2 : ClientBasePool<TestServiceInterface.IService1>, TestServiceInterface.IService1
    {

        #region IService1 Members

        public string MyOperation1(string myValue)
        {
            return Channel.MyOperation1(myValue);
        }

        public string MyOperation2(TestServiceInterface.DataContract1 dataContractValue)
        {
            return Channel.MyOperation2(dataContractValue);
        }

        #endregion
    }
}
