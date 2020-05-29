using System.Security.Principal;
using System.Web;
using Rhino.Mocks;

namespace CISSA.UserApp.Tests.Mock
{
    public class MockHttpContext : HttpContextBase
    {
        private readonly HttpSessionStateBase _sessionStateBase;

        private IPrincipal _user;

        public MockHttpContext()
        {
            _sessionStateBase = MockRepository.GenerateStub<HttpSessionStateBase>(null);
        }

        public override HttpSessionStateBase Session
        {
            get { return _sessionStateBase; }
        }

        public override IPrincipal User
        {
            get
            {
                if (_user == null)
                {
                    _user = new MockPrincipal();
                }
                return _user;
            }
            set { _user = value; }
        }
    }
}