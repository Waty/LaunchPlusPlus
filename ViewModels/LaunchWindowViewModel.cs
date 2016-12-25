using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Launch__.Models;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Newtonsoft.Json;

namespace Launch__.ViewModels
{
    public class LaunchWindowViewModel : BindableBase
    {
        public string Version => "v1.2.3";

        private static readonly string AccFolder =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Launch++");

        private AccountInfoModel _newAccount = new AccountInfoModel();

        public AccountInfoModel NewAccount
        {
            get { return _newAccount; }
            set { SetProperty(ref _newAccount, value); }
        }

        public ObservableCollection<LoginModel> Accounts { get; } = new ObservableCollection<LoginModel>();

        public ObservableCollection<string> Messages { get; } = new ObservableCollection<string>();

        public LaunchWindowViewModel()
        {
            StartQueuedLoginsCommand = new DelegateCommand(StartQueuedLoginsAction);
            ReloadAccountsListCommand = new DelegateCommand(ReloadAccountsListAction);
            AddAccountCommand = new DelegateCommand(AddAccountAction);
            ClearAccountCommand = new DelegateCommand(ClearAddAccountAction);
            DeleteAccountCommand = new DelegateCommand(DeleteAccountAction);

            ReloadAccountsListAction();
        }

        public ICommand StartQueuedLoginsCommand { get; }

        private async void StartQueuedLoginsAction()
        {
            foreach (var item in Accounts.Where(x => x.IsQueued))
            {
                string passport;
                try
                {
                    passport = await GetPassportAsync(item).ConfigureAwait(true);
                }
                catch (Exception e)
                {
                    Messages.Add($"[{item.Username}] Failed to grab login token: {e.Message}");
                    return;
                }

                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = Path.Combine(Directory.GetCurrentDirectory(), "MapleStory.exe"),
                        Arguments = "-nxl " + passport
                    });

                    item.IsQueued = false;

                    Messages.Add($"[{item.Username}] Successfully started MapleStory");
                }
                catch (Exception e)
                {
                    Messages.Add($"[{item.Username}] Failed to start MapleStory.exe:");
                    Messages.Add(e.Message);
                }
            }
        }

        private async Task<string> GetPassportAsync(LoginModel item)
        {
            // if token is expired, refresh it
            if (item.NexonToken.ExpirationDate < DateTime.Now)
            {
                item.NexonToken = await WebApi.RefreshToken(item.NexonToken).ConfigureAwait(false);
                await SaveLoginModel(item).ConfigureAwait(false);
            }

            return await WebApi.GetPassport(item.NexonToken.Token).ConfigureAwait(false);
        }

        public ICommand ReloadAccountsListCommand { get; set; }

        private async void ReloadAccountsListAction()
        {
            Accounts.Clear();

            var folder = new DirectoryInfo(AccFolder);
            foreach (var file in folder.EnumerateFiles("*.json"))
            {
                using (var stream = file.OpenText())
                {
                    var json = await stream.ReadToEndAsync().ConfigureAwait(true);
                    var model = JsonConvert.DeserializeObject<LoginModel>(json);
                    Accounts.Add(model);
                }
            }
        }

        public ICommand AddAccountCommand { get; set; }

        private async void AddAccountAction()
        {
            var token = await WebApi.Login(NewAccount.Email, NewAccount.Password).ConfigureAwait(true);
            var loginModel = new LoginModel
            {
                Username = NewAccount.Email,
                NexonToken = token
            };

            await SaveLoginModel(loginModel).ConfigureAwait(true);
            ClearAddAccountAction();
            ReloadAccountsListAction();
        }

        private async Task SaveLoginModel(LoginModel model)
        {
            var filePath = Path.Combine(AccFolder, $"{model.Username}.json");
            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            using (var sw = new StreamWriter(fs))
            {
                var json = JsonConvert.SerializeObject(model);
                await sw.WriteAsync(json).ConfigureAwait(true);
            }
        }

        public ICommand DeleteAccountCommand { get; set; }

        private void DeleteAccountAction()
        {
            foreach (var acc in Accounts.Where(model => model.IsQueued))
            {
                var filePath = Path.Combine(AccFolder, $"{acc.Username}.json");
                if (File.Exists(filePath)) File.Delete(filePath);
                else Messages.Add($"Failed to delete the account '{acc.Username}' (unable to find file)");
            }

            ReloadAccountsListAction();
        }

        public ICommand ClearAccountCommand { get; set; }

        private void ClearAddAccountAction()
        {
            NewAccount = new AccountInfoModel();
        }
    }
}
