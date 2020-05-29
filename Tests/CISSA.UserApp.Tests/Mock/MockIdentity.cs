using System.Security.Principal;

namespace CISSA.UserApp.Tests.Mock
{
    public class MockIdentity : IIdentity
    {
        #region IIdentity Members

        public string AuthenticationType
        {
            get { return "MockAuthentication"; }
        }

        public bool IsAuthenticated
        {
            get { return true; }
        }

        public string Name
        {
            get { return "someUser"; }
        }

        #endregion
    }
}
