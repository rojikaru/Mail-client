using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HelperLibrary;
using HelperLibrary.DAL;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Win32;
using MimeKit;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SMTP_Client.ViewModel
{
    public partial class SmtpWindowViewModel : ObservableObject
    {
        public static MessagePriority[] Priorities { get; }
        public ObservableCollection<string> AttachmentsList { get; }

        private ISmtpClient Client { get; }
        private OpenFileDialog Dialog { get; }

        public IRelayCommand AddCmd { get; }
        public IRelayCommand RemoveCmd { get; }
        public IRelayCommand SendCmd { get; }

        private string From;

        [ObservableProperty, NotifyCanExecuteChangedFor(nameof(SendCmd))]
        private string to;

        [ObservableProperty, NotifyCanExecuteChangedFor(nameof(SendCmd))]
        private string subject;

        [ObservableProperty, NotifyCanExecuteChangedFor(nameof(SendCmd))]
        private string body;

        [ObservableProperty, NotifyCanExecuteChangedFor(nameof(SendCmd))]
        private MessagePriority selectedPriority;

        static SmtpWindowViewModel()
        {
            Priorities = Enum.GetValues<MessagePriority>();
        }
        public SmtpWindowViewModel()
        {
            AddCmd = new RelayCommand(ExecuteAddCommand);
            RemoveCmd = new RelayCommand<string>(ExecuteRemoveCommand);
            SendCmd = new AsyncRelayCommand(ExecuteSendCommand, CanExecuteSendCommand);

            to = subject = body = From = string.Empty;
            SelectedPriority = MessagePriority.Normal;

            AttachmentsList = new();
            Client = new SmtpClient();

            Dialog = new()
            {
                Filter = "All files (*.*)|*.*",
                FilterIndex = 0,
                Multiselect = true,
            };
        }

        private async Task ExecuteSendCommand()
            => await Task.Run(() =>
            {
                lock (Client.SyncRoot)
                {
                    MimeMessage msg = BuildMessage(From, To, Subject, Body, AttachmentsList.ToArray());
                    Client.Send(msg);
                }
            });
        private bool CanExecuteSendCommand()
            => Helper.EmailPattern.IsMatch(To);

        private void ExecuteAddCommand()
        {
            Dialog.FileName = string.Empty;

            if (Dialog.ShowDialog() == true)
                foreach (var path in Dialog.FileNames)
                    AttachmentsList.Add(path);
        }

        private void ExecuteRemoveCommand(string? entity)
        {
            if (entity == null) return;
            AttachmentsList.Remove(entity);
        }

        public async Task Auth(ServerData SmtpData, string uname, string pwd)
        {
            From = uname;

            await Client.ConnectAsync(SmtpData.Host, SmtpData.Port, SmtpData.SecurityOptions);
            await Client.AuthenticateAsync(uname, pwd);
        }
        public async Task Auth(UserCredentials credentials, string pwd) 
            => await Auth(credentials.Servers.SmtpServer!, credentials.UserName, pwd);

        private static MimeMessage BuildMessage(string from, string to, string subject, string body, params string[] attachments)
            => BuildMessage(from, to, subject, body, false, attachments);
        private static MimeMessage BuildMessage(string from, string to, string subject, string body, bool isHtml, params string[] attachments)
        {
            BodyBuilder builder = new();

            if (isHtml) builder.HtmlBody = body;
            else builder.TextBody = body;

            foreach (var attachment in attachments)
                builder.Attachments.Add(attachment);

            MimeMessage message = new();
            message.From.Add(InternetAddress.Parse(from));
            message.To.Add(InternetAddress.Parse(to));
            message.Subject = subject;
            message.Body = builder.ToMessageBody();

            return message;
        }

        ~SmtpWindowViewModel()
        {
            try
            {
                Client.Disconnect(true);
            }
            catch { /* ignore */ }
        }
    }
}
