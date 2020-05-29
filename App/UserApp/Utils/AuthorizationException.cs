using System;

namespace Intersoft.CISSA.UserApp.Utils
{
    public class AuthorizationException : Exception
    {
        public AuthorizationException() : base() {}
        public AuthorizationException(string message) : base(message) {}

        public static void Throw()
        {
            throw new AuthorizationException(Resources.Base.UserNotAuthorized /*"Пользователь не авторизован!"*/);
        }
    }
}