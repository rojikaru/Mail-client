using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HelperLibrary;
using IMAP_Client.Model;
using Microsoft.Web.WebView2.Wpf;
using MimeKit;
using MimeKit.Text;
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

        private static readonly string DownloadsFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads\";
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
                    source.CopyTo(GetAvailablePath(DownloadsFolder, ShortNameWithNoExtension(source), source.Extension));

                    string GetAvailablePath(string PromptedFolder, string filename, string Extension)
                    {
                        string freepath = PromptedFolder + filename + Extension;
                        for (int i = 0; ; i++)
                        {
                            if (!File.Exists(freepath)) break;
                            freepath = $"{PromptedFolder}{source.Name[..source.Name.LastIndexOf('.')]} - copy ({i}){source.Extension}";
                        }
                        return freepath;
                    }
                    string ShortNameWithNoExtension(FileInfo file)
                        => file.Name[..file.Name.LastIndexOf('.')];
                }
            }
        }

        private const string tempfolder = "Temp/";

        private static List<TaggedFile> GetAttachments(MimeMessage Message)
        {
            List<TaggedFile> att = new();

            if (Message.Attachments.Any())
            {
                foreach (var attachment in Message.Attachments)
                {
                    string fileName = new FileInfo(attachment.ContentType.Name).Name;
                    Directory.CreateDirectory(tempfolder);

                    using var stream = File.Create(tempfolder + fileName);
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

            string html = msg.HtmlBody ?? string.Empty;

            await Browser.EnsureCoreWebView2Async();
            if (msg.HtmlBody != null)
            await CurrentDispatcher.InvokeAsync(() => 
            {
                Browser.NavigateToString(html);
            });

            foreach(var attachment in GetAttachments(Message))
                Attachments.Add(attachment);
        }

        ~ViewEmailWindowViewModel()
        {
            Directory.Delete(tempfolder, true);
        }
    }
}
