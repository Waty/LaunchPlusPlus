using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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

        private LoginModel _newAccount = new LoginModel();
        public LoginModel NewAccount
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
                Cookie token = await WebApi.LoginAsync(item.Username, item.Password).ConfigureAwait(true);

                if (token == null)
                {
                    Messages.Add($"[Error] Failed to start grab login token for {item.Username}");
                    return;
                }

                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = Path.Combine(Directory.GetCurrentDirectory(), "MapleStory.exe"),
                        Arguments = "-nxl " + token.Value
                    });

                    item.IsQueued = false;

                    Messages.Add($"[{item.Username}] Successfully started MapleStory");
                }
                catch (FileNotFoundException)
                {
                    Messages.Add($"[{item.Username}] Failed to locate MapleStory.exe");
                }
                catch (InvalidOperationException)
                {
                    Messages.Add($"[{item.Username}] Failed to locate MapleStory.exe");
                }
                catch (Win32Exception)
                {
                    Messages.Add($"[{item.Username}] Failed to locate MapleStory.exe");
                }
                catch
                {
                    Messages.Add($"[{item.Username}] Failed to start MapleStory due to an unknown error");
                }
            }
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
            var filePath = Path.Combine(AccFolder, $"{NewAccount.Username}.json");
            using (var fs = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write))
            using (var sw = new StreamWriter(fs))
            {
                var json = JsonConvert.SerializeObject(NewAccount);
                await sw.WriteAsync(json).ConfigureAwait(true);
            }

            ClearAddAccountAction();
            ReloadAccountsListAction();
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
            NewAccount = new LoginModel();
        }
    }
}
