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

        private NexonToken _nexonToken;
        public NexonToken NexonToken
        {
            get { return _nexonToken; }
            set { SetProperty(ref _nexonToken, value); }
        }

        private bool _isQueued;
        public bool IsQueued
        {
            get { return _isQueued; }
            set { SetProperty(ref _isQueued, value); }
        }
    }
}