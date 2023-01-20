using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HelperLibrary;
using HelperLibrary.DAL;
using MailKit.Net.Smtp;
using Microsoft.Win32;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Security;
using System.Threading.Tasks;

namespace SMTP_Client.ViewModel
{
    public partial class SmtpWindowViewModel : ObservableObject
    {
        public static MessagePriority[] Priorities { get; }
        public ObservableCollection<string> AttachmentsList { get; }
        public ObservableCollection<string> ToList { get; }

        private ISmtpClient Client { get; }
        private OpenFileDialog Dialog { get; }

        public IRelayCommand AddAttachmentCmd { get; }
        public IRelayCommand AddToCmd { get; }
        public IRelayCommand RemoveAttachmentCmd { get; }
        public IRelayCommand RemoveToCmd { get; }
        public IRelayCommand SendCmd { get; }

        private string From;

        [ObservableProperty, NotifyCanExecuteChangedFor(nameof(AddToCmd))]
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
            AddAttachmentCmd = new RelayCommand(ExecuteAddAttachmentCommand);
            AddToCmd = new RelayCommand(ExecuteAddToCmd, CanExecuteAddToCmd);
            RemoveAttachmentCmd = new RelayCommand<string>(ExecuteRemoveAttachmentCommand);
            RemoveToCmd = new RelayCommand<string>(ExecuteRemoveToCommand);
            SendCmd = new AsyncRelayCommand(ExecuteSendCommand, CanExecuteSendCommand);

            to = subject = body = From = string.Empty;
            SelectedPriority = MessagePriority.Normal;

            AttachmentsList = new();
            ToList = new();
            Client = new SmtpClient();

            Dialog = new()
            {
                Filter = "All files (*.*)|*.*",
                FilterIndex = 0,
                Multiselect = true,
            };
        }

        private void ExecuteRemoveToCommand(string? obj)
        {
            if (obj != null)
                ToList.Remove(obj);
        }

        private void ExecuteAddToCmd()
        {
            var emails = To.Split(',', ' ', ';').Where(x => Helper.EmailPattern.IsMatch(x));
            foreach (string email in emails)
                ToList.Add(email);

            To = string.Empty;
            SendCmd.NotifyCanExecuteChanged();
        }
        private bool CanExecuteAddToCmd()
            => Helper.EmailPattern.IsMatch(To) && !ToList.Contains(To);

        private async Task ExecuteSendCommand()
            => await Task.Run(() =>
            {
                lock (Client.SyncRoot)
                {
                    MimeMessage msg = BuildMessage(From, ToList, Subject, Body, AttachmentsList.ToArray());
                    Client.Send(msg);
                }
            });
        private bool CanExecuteSendCommand()
            => ToList.Count > 0;

        private void ExecuteAddAttachmentCommand()
        {
            Dialog.FileName = string.Empty;

            if (Dialog.ShowDialog() == true)
                foreach (var path in Dialog.FileNames)
                    AttachmentsList.Add(path);
        }

        private void ExecuteRemoveAttachmentCommand(string? entity)
        {
            if (entity == null) return;
            AttachmentsList.Remove(entity);
        }

        public async Task Auth(ServerData SmtpData, string uname, SecureString pwd)
        {
            From = uname;

            await Client.ConnectAsync(SmtpData.Host, SmtpData.Port, SmtpData.SecurityOptions);
            await Client.AuthenticateAsync(new NetworkCredential(uname, pwd));
        }
        public async Task Auth(UserCredentials credentials, SecureString pwd, MimeMessage? msg = null)
        {
            await Auth(credentials.Servers.SmtpServer!, credentials.UserName, pwd);

            if (msg == null) return;
            // else
            ToList.Add(GetRecipient(msg, credentials.UserName));
            Subject = ReSubject(msg.Subject);

            static string GetRecipient(MimeMessage msg, string UName)
            {
                string rawfrom = msg.From.ToString(),
                       rawto = msg.To.ToString();

                string from = Helper.BracketsEmailPattern.Matches(rawfrom).FirstOrDefault()?.Value[1..^1]
                              ?? Helper.EmailPattern.Matches(rawfrom).First().Value,
                       to = Helper.BracketsEmailPattern.Matches(rawto).FirstOrDefault()?.Value[1..^1]
                              ?? Helper.EmailPattern.Matches(rawto).First().Value;

                return from == UName ? to : from;
            }
            static string ReSubject(string OriginalSubject)
            {
                string normalized = OriginalSubject.Trim().ToLower();
                return normalized.StartsWith("re:")
                             ? OriginalSubject
                             : "Re: " + OriginalSubject;
            }
        }

        private static MimeMessage BuildMessage(string from, IEnumerable<string> to, string subject, string body, params string[] attachments)
            => BuildMessage(from, to, subject, body, false, attachments);
        private static MimeMessage BuildMessage(string from, IEnumerable<string> to, string subject, string body, bool isBodyHtml, params string[] attachments)
        {
            BodyBuilder builder = new();

            if (isBodyHtml) builder.HtmlBody = body;
            else builder.TextBody = body;

            foreach (var attachment in attachments)
                builder.Attachments.Add(attachment);

            MimeMessage message = new();
            message.From.Add(InternetAddress.Parse(from));
            message.To.AddRange(to.Select(x => InternetAddress.Parse(x)));
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
