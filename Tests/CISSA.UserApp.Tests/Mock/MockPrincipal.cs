using System.Security.Principal;

namespace CISSA.UserApp.Tests.Mock
{
    public class MockPrincipal : IPrincipal
    {
        private IIdentity _identity;

        #region IPrincipal Members

        public IIdentity Identity
        {
            get
            {
                if (_identity == null)
                {
                    _identity = new MockIdentity();
                }
                return _identity;
            }
        }

        public bool IsInRole(string role)
        {
            return false;
        }

        #endregion
    }
}
