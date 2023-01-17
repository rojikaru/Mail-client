using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HelperLibrary;
using IMAP_Client.Model;
using Microsoft.Web.WebView2.Wpf;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Windows;
using System.Windows.Threading;

namespace IMAP_Client.ViewModel
{
    public partial class ViewEmailWindowViewModel : ObservableObject
    {
        public ObservableCollection<TaggedFile> Attachments { get; }
        public WebView2 Browser { get; }
        private Dispatcher CurrentDispatcher { get; }

        [ObservableProperty] private MimeMessage? m_message;
        [ObservableProperty] private string title;

        public IRelayCommand OpenFileCmd { get; }

        public ViewEmailWindowViewModel()
        {
            OpenFileCmd = new RelayCommand<TaggedFile>(ExecuteOpenFileCmd);

            Browser = new();
            Attachments = new();
            CurrentDispatcher = Dispatcher.CurrentDispatcher;

            title = string.Empty;
        }

        private void ExecuteOpenFileCmd(TaggedFile? obj)
        {
            if (obj == null ||
                obj == TaggedFile.Empty) return;

            FileInfo source = obj.Info;
            try
            {
                Process.Start(new ProcessStartInfo(source.FullName) { UseShellExecute = true });
            }
            catch
            {
                if (MessageBox.Show("Could not open selected file.\nShould it be copied to the download folder?",
                    "Error!", MessageBoxButton.YesNo, MessageBoxImage.Warning)
                    == MessageBoxResult.Yes)
                {
                    string dlfolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads\",
                           dest = dlfolder + source.Name;

                    if (!File.Exists(dest))
                        File.Copy(source.FullName, dest);
                    else
                    {
                        string dest1;
                        for (int i = 0; ; i++)
                        {
                            dest1 = $"{dlfolder}{source.Name[..source.Name.LastIndexOf('.')]} - copy ({i}){source.Extension}";
                            if (!File.Exists(dest1)) break;
                        }
                        File.Copy(source.FullName, dest1);
                        MessageBox.Show("Copied!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }

        private static List<TaggedFile> GetAttachments(MimeMessage Message)
        {
            List<TaggedFile> att = new();

            if (Message.Attachments.Any())
            {
                foreach (var attachment in Message.Attachments)
                {
                    string fileName = attachment.ContentType.Name;
                    Directory.CreateDirectory("Temp");

                    using var stream = File.Create("Temp/" + fileName);
                    if (attachment is MessagePart rfc822)
                        rfc822.Message.WriteTo(stream);
                    else
                    {
                        MimePart part = (MimePart)attachment;
                        part.Content.DecodeTo(stream);
                    }

                    att.Add(new TaggedFile(stream.Name));
                }
            }
            else
            {
                att.Add(TaggedFile.Empty);
            }

            return att;
        }

        public async void Load(MimeMessage msg)
        {
            Message = msg;
            Title = $"{msg.Subject} - Email viewer";

            await Browser.EnsureCoreWebView2Async();
            await CurrentDispatcher.InvokeAsync(() => 
            {
                Browser.NavigateToString(msg.HtmlBody);
            });

            foreach(var attachment in GetAttachments(Message))
                Attachments.Add(attachment);
        }
    }
}
