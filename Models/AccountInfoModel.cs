using Microsoft.Practices.Prism.Mvvm;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launch__.Models
{
    public class AccountInfoModel : BindableBase
    {
        private string _Username;
        public string Username
        {
            get { return _Username; }
            set { _Username = value; OnPropertyChanged(nameof(Username)); }
        }

        private string _Password;
        public string Password
        {
            get { return _Password; }
            set { _Password = value; OnPropertyChanged(nameof(Password)); }
        }

        private string _PIC;
        public string PIC
        {
            get { return _PIC; }
            set { _PIC = value; OnPropertyChanged(nameof(PIC)); }
        }
    }
}
