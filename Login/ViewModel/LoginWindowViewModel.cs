using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HelperLibrary;
using HelperLibrary.DAL;
using HelperLibrary.Repository;
using IMAP_Client.View;
using IMAP_Client.ViewModel;
using Login.Model;
using MailKit.Net.Imap;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Windows;
using static System.Windows.Visibility;

namespace Login.ViewModel
{
    public partial class LoginWindowViewModel : ObservableObject
    {
        public IRelayCommand CreationCommand { get; }
        public IRelayCommand SelectExistingCmd { get; }
        public IRelayCommand ServerSelectCmd { get; }
        public IRelayCommand CheckEmailCmd { get; }
        public IRelayCommand CheckPwdCmd { get; }
        public IRelayCommand GoToStartCmd { get; }
        public IRelayCommand GoToSelectionCmd { get; }
        public IRelayCommand SelectEmailCmd { get; }
        public IRelayCommand AddServerCmd { get; }
        public IRelayCommand GoBackToCreationCmd { get; }
        public IRelayCommand GoBackFromPwdCmd { get; }

        [ObservableProperty] private Visibility m_startGridVisibility;
        [ObservableProperty] private Visibility m_serverSelectionGridVisibility;
        [ObservableProperty] private Visibility m_customServerGridVisibility;
        [ObservableProperty] private Visibility m_usernameInputGridVisibility;
        [ObservableProperty] private Visibility m_existingGridVisibility;
        [ObservableProperty] private Visibility m_passwordGridVisibility;
        [ObservableProperty] private Visibility m_error2Visibility;
        [ObservableProperty] private Visibility m_error1Visibility;

        [ObservableProperty, NotifyCanExecuteChangedFor(nameof(CheckEmailCmd))] private string? m_uname;
        [ObservableProperty, NotifyCanExecuteChangedFor(nameof(SelectEmailCmd))] private UserCredentials? m_user;
        [ObservableProperty, NotifyCanExecuteChangedFor(nameof(AddServerCmd))] private string? smtpserv;
        [ObservableProperty, NotifyCanExecuteChangedFor(nameof(AddServerCmd))] private string? imapserv;
        [ObservableProperty, NotifyCanExecuteChangedFor(nameof(AddServerCmd))] private string? smtpport;
        [ObservableProperty, NotifyCanExecuteChangedFor(nameof(AddServerCmd))] private string? imapport;
        [ObservableProperty, NotifyCanExecuteChangedFor(nameof(AddServerCmd))] private SecureSocketOptions smtpsec;
        [ObservableProperty, NotifyCanExecuteChangedFor(nameof(AddServerCmd))] private SecureSocketOptions imapsec;

        private SecureString? m_pwd;
        public SecureString Pwd
        {
            get => m_pwd!;
            set
            {
                m_pwd?.Dispose();

                if (SetProperty(ref m_pwd, value, nameof(Pwd)))
                    CheckPwdCmd.NotifyCanExecuteChanged();
            }
        }

        private IUnitOfWork UnitOfWork { get; }
        private IGenericRepository<ServerData> Serverdatas => UnitOfWork.Repository<ServerData>();
        private IGenericRepository<ServerCredentials> Servercreds => UnitOfWork.Repository<ServerCredentials>();
        public IGenericRepository<UserCredentials> Users => UnitOfWork.Repository<UserCredentials>();

        public IEnumerable<ServerCredentials> Servers { get; }
        public static SecureSocketOptions[] Securities { get; }

        static LoginWindowViewModel()
        {
            Securities = Enum.GetValues<SecureSocketOptions>();
        }
        public LoginWindowViewModel()
        {
            CreationCommand = new RelayCommand(ExecuteNewCommand);
            SelectExistingCmd = new RelayCommand(ExecuteSelectExistingCmd);
            ServerSelectCmd = new RelayCommand<ServerCredentials>(ExecuteServerSelectCmd);
            CheckEmailCmd = new RelayCommand(ExecuteCheckEmailCmd, CanExecuteCheckEmailCmd);
            CheckPwdCmd = new RelayCommand<Window>(ExecuteCheckPwdCmd, CanExecuteCheckPwdCmd);
            GoToStartCmd = new RelayCommand(ExecuteGoToStartCmd);
            SelectEmailCmd = new RelayCommand(ExecuteSelectEmailCmd, CanExecuteSelectEmailCmd);
            GoToSelectionCmd = new RelayCommand(ExecuteGoToSelectionCmd, CanExecuteGoToSelectionCmd);
            AddServerCmd = new RelayCommand(ExecuteAddServerCmd, CanExecuteAddServerCmd);
            GoBackToCreationCmd = new RelayCommand(ExecuteGoBackToCreationCmd);
            GoBackFromPwdCmd = new RelayCommand(ExecuteGoBackFromPwdCmd, CanExecuteGoBackFromPwdCmd);

            AllInvisible();
            StartGridVisibility = Visible;

            CredentialsContext cnt = new(false);
            UnitOfWork = new GenericUnitOfWork(cnt);

            Pwd = new();

            if (Servercreds.Count == 0)
            {
                Serverdatas.AddRange(Datarepo.ServerDatas);
                Servercreds.AddRange(Datarepo.ServerCreds);
                Servers = Servercreds;
            }
            else
            {
                _ = Serverdatas.GetAll().ToList(); // load servers (lazy loading)
                Servers = Servercreds.IntersectBy(Datarepo.ServerCreds.Select(x => x.FriendlyName), x => x.FriendlyName);
            }

            InitCredentialsContext();
        }

        private bool? ToUnameInput;
        private void ExecuteGoBackFromPwdCmd()
        {
            if (ToUnameInput == null)
                return;

            AllInvisible();
            Pwd = new();

            if (ToUnameInput == true) UsernameInputGridVisibility = Visible;
            else if (ToUnameInput == false) ExistingGridVisibility = Visible;
        }
        private bool CanExecuteGoBackFromPwdCmd() => PasswordChecking != true;

        private bool? ToSelection;
        private void ExecuteGoBackToCreationCmd()
        {
            if (ToSelection == true)
                GoToSelectionCmd.Execute(null);
            else if (ToSelection == false)
            {
                AllInvisible();
                CustomServerGridVisibility = Visible;
                Uname = string.Empty;
            }
        }

        private bool? AddingServer;
        private async void ExecuteAddServerCmd()
        {
            AddingServer = true;
            AddServerCmd.NotifyCanExecuteChanged();
            GoToSelectionCmd.NotifyCanExecuteChanged();
            Error1Visibility = Collapsed;
            try
            {
                User!.Servers = new(
                    Smtpserv!, ushort.Parse(Smtpport!), Smtpsec,
                    Imapserv!, ushort.Parse(Imapport!), Imapsec
                    );

                await UserCredentials.TryAuth(User!, new(), false);

                AddingServer = false;
                AddServerCmd.NotifyCanExecuteChanged();
                GoToSelectionCmd.NotifyCanExecuteChanged();

                AllInvisible();
                UsernameInputGridVisibility = Visible;
                ToSelection = false;

                if (!Servercreds.Any(x => x == User!.Servers))
                    await Servercreds.AddAsync(User!.Servers);
            }
            catch
            {
                Error1Visibility = Visible;
                AddingServer = false;
                AddServerCmd.NotifyCanExecuteChanged();
                GoToSelectionCmd.NotifyCanExecuteChanged();
            }
        }
        private bool CanExecuteAddServerCmd()
            => !string.IsNullOrWhiteSpace(Smtpserv)
            && !string.IsNullOrWhiteSpace(Imapserv)
            && ushort.TryParse(Smtpport, out _)
            && ushort.TryParse(Imapport, out _)
            && AddingServer != true;

        private void ExecuteGoToSelectionCmd()
        {
            AllInvisible();
            ServerSelectionGridVisibility = Visible;

            Smtpserv = Imapserv = 
            Smtpport = Imapport = string.Empty;
            Smtpsec = Imapsec = SecureSocketOptions.None;

            User = new();
        }
        private bool CanExecuteGoToSelectionCmd() => AddingServer != true;

        private void ExecuteSelectEmailCmd()
        {
            AllInvisible();
            PasswordGridVisibility = Visible;
            ToUnameInput = false;
        }
        private bool CanExecuteSelectEmailCmd() => User != null;

        private void ExecuteGoToStartCmd()
        {
            AllInvisible();
            StartGridVisibility = Visible;

            User = null;
        }

        private void ExecuteNewCommand()
        {
            AllInvisible();
            ServerSelectionGridVisibility = Visible;

            User = new();
        }

        private void ExecuteSelectExistingCmd()
        {
            if (Users.Count == 0)
            {
                MessageBox.Show("No users found. Try creating new account",
                    "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                AllInvisible();
                User = Users.FirstOrDefault();
                ExistingGridVisibility = Visible;
            }
        }

        private void ExecuteCheckEmailCmd()
        {
            if (Users.Any(x => x.UserName == Uname)) 
                Error1Visibility = Visible;
            else
            {
                AllInvisible();
                PasswordGridVisibility = Visible;
                User!.UserName = Uname!;
                ToUnameInput = true;
            }
        }
        private bool CanExecuteCheckEmailCmd() => Helper.EmailPattern.IsMatch(Uname ?? string.Empty);

        private bool? PasswordChecking;
        private async void ExecuteCheckPwdCmd(Window? thiswnd)
        {
            Error1Visibility = Error2Visibility = Collapsed;
            PasswordChecking = true;
            CheckPwdCmd.NotifyCanExecuteChanged();
            GoBackFromPwdCmd.NotifyCanExecuteChanged();

            try
            {
                await UserCredentials.TryAuth(User!, Pwd);

                ImapWindow imapwnd = new();
                var vm = (ImapWindowViewModel)imapwnd.DataContext;
                await vm.Auth(User!, Pwd);

                if (!Users.Contains(User!))
                await Users.AddAsync(User!);

                imapwnd.Show();
                thiswnd?.Close();
            }
            catch (MailServerException)
            {
                Error1Visibility = Visible;
            }
            catch (UserCredentialsException)
            {
                Error2Visibility = Visible;
            }

            PasswordChecking = false;
            CheckPwdCmd.NotifyCanExecuteChanged();
            GoBackFromPwdCmd.NotifyCanExecuteChanged();
        }
        private bool CanExecuteCheckPwdCmd(Window? wnd) 
            => Pwd.Length > 0 && PasswordChecking != true;

        private void ExecuteServerSelectCmd(ServerCredentials? obj)
        {
            if (obj is null) return;

            AllInvisible();
            if (obj.FriendlyName == "Custom server")
            {
                Smtpport = Imapport = "0";
                Smtpsec = Imapsec = SecureSocketOptions.Auto;
                CustomServerGridVisibility = Visible;
            }
            else
            {
                UsernameInputGridVisibility = Visible;
                ToSelection = true;
            }

            User!.Servers = obj;
        }

        private void AllInvisible()
        {
            StartGridVisibility =
                ServerSelectionGridVisibility =
                CustomServerGridVisibility =
                Error1Visibility =
                Error2Visibility =
                UsernameInputGridVisibility =
                ExistingGridVisibility =
                PasswordGridVisibility =
                Collapsed;
        }

        private async void InitCredentialsContext()
        {
            if (Serverdatas.Count != 0 || Servercreds.Count != 0) return;

            await Serverdatas.AddRangeAsync(Datarepo.ServerDatas);
            await Servercreds.AddRangeAsync(Datarepo.ServerCreds);
        }

        ~LoginWindowViewModel()
        {
            UnitOfWork.Dispose();
        }
    }
}
