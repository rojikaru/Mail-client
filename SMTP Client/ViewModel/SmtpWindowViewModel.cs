using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HelperLibrary;
using HelperLibrary.DAL;
using MailKit.Net.Smtp;
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
        public ObservableCollection<MimePart> AttachmentsList { get; }

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
            RemoveCmd = new RelayCommand<MimePart>(ExecuteRemoveCommand);
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
                    AttachmentsList.Add(new MimePart() { FileName = path });
        }

        private void ExecuteRemoveCommand(MimePart? entity)
        {
            if (entity == null) return;
            AttachmentsList.Remove(entity);
        }

        public async Task Auth(ServerData SmtpData, string username)
        {
            await Client.ConnectAsync(SmtpData.Host, SmtpData.Port, SmtpData.SecurityOptions);
            From = username;
        }
        public async Task Auth(UserCredentials credentials) => await Auth(credentials.Servers.SmtpServer!, credentials.UserName);

        private static MimeMessage BuildMessage(string from, string to, string subject, string body, params MimePart[] attachments)
            => BuildMessage(from, to, subject, body, false, attachments);
        private static MimeMessage BuildMessage(string from, string to, string subject, string body, bool isHtml, params MimePart[] attachments)
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
            Client.Disconnect(true);
        }
    }
}
