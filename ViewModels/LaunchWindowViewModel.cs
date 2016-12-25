using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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

        private LoginModel _selectedAccount;
        public LoginModel SelectedAccount
        {
            get { return _selectedAccount; }
            set { SetProperty(ref _selectedAccount, value); }
        }

        private LoginModel _newAccount = new LoginModel();
        public LoginModel NewAccount
        {
            get { return _newAccount; }
            set { SetProperty(ref _newAccount, value); }
        }

        private bool _autoClosePlayScreen;
        public bool AutoClosePlayScreen
        {
            get { return _autoClosePlayScreen; }
            set { SetProperty(ref _autoClosePlayScreen, value); }
        }

        public ObservableCollection<LoginModel> Accounts { get; } = new ObservableCollection<LoginModel>();

        public ObservableCollection<string> Messages { get; } = new ObservableCollection<string>();

        public LaunchWindowViewModel()
        {
            StartQueuedLoginsCommand = new DelegateCommand(StartQueuedLoginsAction);
            ReloadAccountsListCommand = new DelegateCommand(ReloadAccountsListAction);
            AddAccountCommand = new DelegateCommand(AddAccountAction);
            ClearAccountCommand = new DelegateCommand(ClearAccountAction);
            DeleteAccountCommand = new DelegateCommand(DeleteAccountAction);

            ReloadAccountsListAction();
        }

        public ICommand StartQueuedLoginsCommand { get; }

        private async void StartQueuedLoginsAction()
        {
            foreach (var item in Accounts.Where(x => x.IsQueued))
            {
                Cookie token = await WebApi.LoginAsync(item?.Username, item?.Password);

                if (token == null)
                {
                    Messages.Add($"[Error] Failed to start grab login token for {item?.Username}");
                    return;
                }

                try
                {
                    Process pStarted = Process.Start(new ProcessStartInfo
                    {
                        FileName = Path.Combine(Directory.GetCurrentDirectory(), "MapleStory.exe"),
                        Arguments = "-nxl " + token.Value
                    });

                    if (AutoClosePlayScreen)
                    {
                        pStarted.WaitForInputIdle();
                        pStarted.CloseMainWindow();
                    }

                    item.IsQueued = false;
                }
                catch (FileNotFoundException)
                {
                    Messages.Add($"[{item.Username}] Failed to locate MapleStory.exe");
                    return;
                }
                catch (InvalidOperationException)
                {
                    Messages.Add($"[{item.Username}] Failed to locate MapleStory.exe");
                    return;
                }
                catch (Win32Exception)
                {
                    Messages.Add($"[{item.Username}] Failed to locate MapleStory.exe");
                    return;
                }
                catch
                {
                    Messages.Add($"[{item.Username}] Failed to start MapleStory due to an unknown error");
                    return;
                }

                Messages.Add($"[{item.Username}] Successfully started MapleStory");
            }
        }

        public ICommand ReloadAccountsListCommand { get; set; }

        private async void ReloadAccountsListAction()
        {
            Accounts.Clear();

            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Launch++");

            DirectoryInfo folder = new DirectoryInfo(path);

            foreach (var file in folder.EnumerateFiles("*.json"))
            {
                using (var stream = file.OpenText())
                {
                    string js = await stream.ReadToEndAsync();

                    LoginModel model = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<LoginModel>(js));

                    Accounts.Add(model);
                }
            }
        }

        public ICommand AddAccountCommand { get; set; }

        private async void AddAccountAction()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Launch++");

            string js = JsonConvert.SerializeObject(NewAccount);

            using (FileStream fs = new FileStream(Path.Combine(path, $"{NewAccount.Username}.json"), FileMode.CreateNew, FileAccess.Write))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                await sw.WriteAsync(js);
            }

            ClearAccountAction();

            ReloadAccountsListAction();
        }

        public ICommand DeleteAccountCommand { get; set; }

        private void DeleteAccountAction()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Launch++", SelectedAccount?.Username + ".json");

            if (File.Exists(path))
                File.Delete(path);
            else
                Messages.Add($"Failed to delete the account {SelectedAccount?.Username}");

            ReloadAccountsListAction();
        }

        public ICommand ClearAccountCommand { get; set; }

        private void ClearAccountAction()
        {
            NewAccount = new LoginModel();
        }
    }
}
