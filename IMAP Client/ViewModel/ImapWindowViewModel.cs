using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HelperLibrary.DAL;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MimeKit;
using SMTP_Client.View;
using SMTP_Client.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace IMAP_Client.ViewModel
{
    public partial class ImapWindowViewModel : ObservableObject
    {
        private static IDictionary<string, SearchQuery> m_Queries { get; }
        public static IEnumerable<string> StrQueries => m_Queries.Keys;
        public ObservableCollection<MimeMessage> Messages { get; }
        private IDictionary<string, IMailFolder> m_Folders { get; }
        public IEnumerable<string> Folders => m_Folders.Keys;

        public IRelayCommand SearchCmd { get; }
        public IRelayCommand WriteCmd { get; }

        private IImapClient Client { get; }

        [ObservableProperty, NotifyCanExecuteChangedFor(nameof(SearchCmd))]
        private string m_SearchKey;
        private UserCredentials? creds;

        private string? m_SelectedQuery;
        public string SelectedQuery
        {
            get => m_SelectedQuery ?? string.Empty;
            set
            {
                CancelLoadingSource?.Cancel();
                SetProperty(ref m_SelectedQuery, value, nameof(SelectedQuery));
                SearchCmd.NotifyCanExecuteChanged();
                Messages.Clear();

                Task.Run(async () =>
                {
                    if (value != null)
                    {
                        await Task.Delay(1000);
                        ExecuteSearch(m_Queries[value]);
                    }
                });
            }
        }

        private string? m_SelectedFolder;
        public string SelectedStrFolder
        {
            get => m_SelectedFolder!;
            set
            {
                CancelLoadingSource?.Cancel();
                Messages.Clear();

                Task.Run(async () =>
                {
                    await Task.Delay(1000);
                    lock (Client.SyncRoot)
                    {
                        SelectedFolder?.Close();
                        m_SelectedFolder = value;
                        SelectedFolder = m_Folders[value];
                        SelectedFolder.Open(FolderAccess.ReadWrite);

                        ExecuteSearch(SearchQuery.All);
                    }
                });
            }
        }

        private IMailFolder? SelectedFolder;

        private CancellationTokenSource? _cancelloadingsource;
        private CancellationTokenSource? CancelLoadingSource
        {
            get => _cancelloadingsource;
            set
            {
                lock (this)
                {
                    _cancelloadingsource?.Dispose();
                    _cancelloadingsource = value;
                }
            }
        }
        private CancellationToken CancelLoadingToken => CancelLoadingSource?.Token ?? default;

        private Dispatcher CurrentDispatcher { get; }

        static ImapWindowViewModel()
        {
            m_Queries = new Dictionary<string, SearchQuery>()
            {
                { "All", SearchQuery.All },
                { "Flagged", SearchQuery.Flagged}, 
                { "Not flagged", SearchQuery.NotFlagged },
                { "Deleted", SearchQuery.Deleted }, 
                { "Answered", SearchQuery.Answered },
                { "Seen", SearchQuery.Seen },
                { "Not seen", SearchQuery.NotSeen },
                { "Old", SearchQuery.Old },
                { "Recent", SearchQuery.Recent }
            };
        }
        public ImapWindowViewModel()
        {
            Messages = new();
            m_Folders = new Dictionary<string, IMailFolder>();
            Client = new ImapClient();

            SearchCmd = new RelayCommand(ExecuteSearchCmd, CanExecuteSearchCmd);
            WriteCmd = new AsyncRelayCommand(ExecuteWriteCmd);

            m_SearchKey = string.Empty;
            SelectedQuery = "All";

            CurrentDispatcher = Dispatcher.CurrentDispatcher;

            GC.Collect(GC.MaxGeneration);
        }
        
        private async Task ExecuteWriteCmd()
        {
            SmtpWindow wnd = new();
            var VM = (SmtpWindowViewModel)wnd.DataContext;
            await VM.Auth(creds!);
            wnd.ShowDialog();
        }

        private void ExecuteSearchCmd()
            => ExecuteSearch(SearchKey.Trim());
        private bool CanExecuteSearchCmd()
            => !string.IsNullOrWhiteSpace(SearchKey);

        public async Task Auth(UserCredentials credentials, string pwd)
        {
            creds = credentials;
            ServerData? ImapData = credentials.Servers.ImapServer;

            if (ImapData is null) throw new ArgumentException("Servers.ImapServer property may not be null to auth", nameof(credentials));

            await Client.ConnectAsync(ImapData.Host, ImapData.Port, ImapData.SecurityOptions);
            await Client.AuthenticateAsync(new NetworkCredential(credentials.UserName, pwd));

            foreach (var ns in Client.PersonalNamespaces)
            {
                var folders = Client.GetFolders(ns)
                                    .Where(x => (x.Attributes & FolderAttributes.NonExistent) != FolderAttributes.NonExistent);
                foreach (var folder in folders)
                    m_Folders.Add(folder.Name, folder);
            }

            SelectedStrFolder = Folders.First();
        }

        private static SearchQuery AggregateQueriesByTerm(string term)
        {
            SearchQuery[] queries = new[]
            {
                SearchQuery.BccContains(term),
                SearchQuery.CcContains(term),
                SearchQuery.ToContains(term),
                SearchQuery.FromContains(term),
                SearchQuery.BodyContains(term),
                SearchQuery.SubjectContains(term),
                SearchQuery.MessageContains(term),
            };

            SearchQuery result = queries[0];
            foreach (SearchQuery query in queries)
                result = result.Or(query);

            return result;
        }

        private void ExecuteSearch(string query) => ExecuteSearch(AggregateQueriesByTerm(query));
        private void ExecuteSearch(SearchQuery query)
        {
            if (SelectedFolder == null) return;

            lock (Client.SyncRoot)
            {
                CancelLoadingSource = new();

                var idlist = SelectedFolder.Search(query).Reverse().ToArray();

                if (idlist.Length == 0)
                    MessageBox.Show("No items found >.<", "Information", MessageBoxButton.OK, MessageBoxImage.Information);

                MimeMessage msg;
                foreach (var id in idlist)
                {
                    if (CancelLoadingToken.IsCancellationRequested)
                        return;

                    msg = SelectedFolder.GetMessage(id);
                    CurrentDispatcher.InvokeAsync(() => Messages.Add(msg), DispatcherPriority.DataBind, CancelLoadingToken);
                }
            }

            //if (CancelLoadingToken.IsCancellationRequested)
            //    CancelLoadingSource = new();
        }

        ~ImapWindowViewModel()
        {
            Client.Disconnect(true);
        }
    }
}
