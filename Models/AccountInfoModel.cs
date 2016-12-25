using Microsoft.Practices.Prism.Mvvm;

namespace Launch__.Models
{
    public class AccountInfoModel : BindableBase
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

        private string _pic;
        public string Pic
        {
            get { return _pic; }
            set { SetProperty(ref _pic, value); }
        }
    }
}