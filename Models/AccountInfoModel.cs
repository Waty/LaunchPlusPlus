using Microsoft.Practices.Prism.Mvvm;

namespace Launch__.Models
{
    public class AccountInfoModel : BindableBase
    {
        private string _email;
        public string Email
        {
            get { return _email; }
            set { SetProperty(ref _email, value); }
        }

        private string _password;
        public string Password
        {
            get { return _password; }
            set { SetProperty(ref _password, value); }
        }
    }
}