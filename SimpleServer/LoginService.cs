using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleServer
{
    public class LoginService
    {
        private List<string> _logins;

        public LoginService(List<string> logins) =>
            _logins = logins;

        public bool IsLogged(string login) =>
            _logins.Contains(login);
    }
}
