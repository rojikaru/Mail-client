using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IMAP_Client.Model;
using Microsoft.Web.WebView2.Wpf;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace IMAP_Client.ViewModel
{
    public partial class ViewEmailWindowViewModel : ObservableObject
    {
        public enum ViewEmailResult
        {
            None, Delete, Respond
        }

        public ViewEmailResult Result { get; private set; }

        public ObservableCollection<TaggedFile> Attachments { get; }
        public WebView2 Browser { get; }

        private Dispatcher CurrentDispatcher { get; }

        [ObservableProperty] private MimeMessage? m_message;
        [ObservableProperty] private string title;

        public IRelayCommand OpenFileCmd { get; }
        public IRelayCommand DeleteCmd { get; }
        public IRelayCommand RespondCmd { get; }

        public ViewEmailWindowViewModel()
        {
            DeleteCmd = new RelayCommand(ExecuteDeleteCmd);
            RespondCmd = new RelayCommand(ExecuteRespondCmd);

            Result = ViewEmailResult.None;

            OpenFileCmd = new RelayCommand<TaggedFile>(ExecuteOpenFileCmd);

            Browser = new();
            Attachments = new();
            CurrentDispatcher = Dispatcher.CurrentDispatcher;

            title = string.Empty;
        }

        private void ExecuteDeleteCmd()
            => Result = ViewEmailResult.Delete;
        private void ExecuteRespondCmd()
            => Result = ViewEmailResult.Respond;

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

                    static string GetAvailablePath(string PromptedFolder, string filename, string Extension)
                    {
                        string freepath = PromptedFolder + filename + Extension;
                        for (int i = 0; ; i++)
                        {
                            if (!File.Exists(freepath)) break;
                            freepath = $"{PromptedFolder}{filename} - copy ({i}){Extension}";
                        }
                        return freepath;
                    }
                    static string ShortNameWithNoExtension(FileInfo file)
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
                    MimeEntityToStream(stream, attachment);
                    att.Add(new TaggedFile(stream.Name));

                    static void MimeEntityToStream(FileStream fs, MimeEntity? entity)
                    {
                        if (entity == null)
                            throw new ArgumentNullException(nameof(entity));
                        else if (entity is MessagePart rfc822)
                            rfc822.Message.WriteTo(fs);
                        else
                        {
                            MimePart part = (MimePart)entity;
                            part.Content.DecodeTo(fs);
                        }
                    }
                }
            }
            else
            {
                att.Add(TaggedFile.Empty);
            }

            return att;
        }

        public async void Load(MimeMessage Message)
        {
            this.Message = Message;
            Title = $"{Message.Subject} - Email viewer";

            string html = Message.HtmlBody ?? string.Empty;

            await Browser.EnsureCoreWebView2Async();
            if (Message.HtmlBody != null)
            await CurrentDispatcher.InvokeAsync(() => 
            {
                Browser.NavigateToString(html);
            });

            foreach(var attachment in GetAttachments(this.Message))
                Attachments.Add(attachment);
        }
    }
}
