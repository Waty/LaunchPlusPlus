using Microsoft.Practices.Prism.Mvvm;

namespace Launch__.Models
{
    public class LoginModel : BindableBase
    {
        private string _username;
        public string Username
        {
            get { return _username; }
            set { SetProperty(ref _username, value); }
        }

        private string _password;
        public string Password
        {
            get { return _password; }
            set { SetProperty(ref _password, value); }
        }

        private bool _isQueued;
        public bool IsQueued
        {
            get { return _isQueued; }
            set { SetProperty(ref _isQueued, value); }
        }
    }
}