using MailKit.Security;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace HelperLibrary.DAL
{
    public class ServerCredentials : IEquatable<ServerCredentials>
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)] public int ID { get; set; }
        public ServerData? SmtpServer { get; set; }
        public ServerData? ImapServer { get; set; }
        public string ImagePath { get; set; }
        public string FriendlyName { get; set; }

        public ServerCredentials(ServerData SmtpServer, ServerData ImapServer, string ImagePath = "", string FriendlyName = "")
        {
            this.SmtpServer = SmtpServer;
            this.ImapServer = ImapServer;
            this.ImagePath = ImagePath;
            this.FriendlyName = FriendlyName;
        }
        public ServerCredentials(
            string SmtpHost, int SmtpPort, SecureSocketOptions SmtpSecurityOptions,
            string ImapHost, int ImapPort, SecureSocketOptions ImapSecurityOptions, 
            string ImagePath = "", string FriendlyName = "")
        {
            this.SmtpServer = new ServerData(SmtpHost, SmtpPort, SmtpSecurityOptions);
            this.ImapServer = new ServerData(ImapHost, ImapPort, ImapSecurityOptions);
            this.ImagePath = ImagePath;
            this.FriendlyName = FriendlyName;
        }
        public ServerCredentials(
            string SmtpHost, int SmtpPort, string ImapHost, int ImapPort, 
            string ImagePath = "", string FriendlyName = "")
            : this (SmtpHost, SmtpPort, SecureSocketOptions.Auto, ImapHost, ImapPort, SecureSocketOptions.Auto, ImagePath, FriendlyName) { }
        public ServerCredentials(string SmtpHost, string ImapHost, string ImagePath = "", string FriendlyName = "")
            : this (SmtpHost, 0, ImapHost, 0, ImagePath, FriendlyName) { }
        public ServerCredentials()
            : this(string.Empty, string.Empty) { }
        static ServerCredentials()
            => None = new();

        public static ServerCredentials None { get; }

        public static bool operator==(ServerCredentials? a, ServerCredentials? b)
        {
            if (a is null) return b is null;
            else if (b is null) return false;
            else return a.SmtpServer == b.SmtpServer 
                     && a.ImapServer == b.ImapServer 
                     && a.FriendlyName == b.FriendlyName;
        }
        public static bool operator !=(ServerCredentials? a, ServerCredentials? b)
            => !(a == b);

        public bool Equals(ServerCredentials? other) => this == other;

        public override bool Equals(object? obj)
        {
            if (obj is ServerCredentials sc)
                return this == sc;
            else return false;
        }

        public override int GetHashCode() => base.GetHashCode();
    }
}