using MailKit.Security;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using MailKit.Net.Imap;

namespace HelperLibrary.DAL
{
    public class ServerData : IEquatable<ServerData>
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)] public int ID { get; set; }
        [Required] public string Host { get; set; }
        [Required] public int Port { get; set; }
        [Required] public SecureSocketOptions SecurityOptions { get; set; }

        public ServerData(string host, int port = 0, SecureSocketOptions securityOptions = SecureSocketOptions.Auto)
        {
            Host = host;
            Port = port;
            SecurityOptions = securityOptions;
        }
        public ServerData()
            : this(string.Empty, 0, SecureSocketOptions.Auto) { }
        static ServerData()
            => None = new();

        public static ServerData None { get; }

        public static bool operator==(ServerData? left, ServerData? right)
        {
            if (left is null) return right is null;
            else if (right is null) return false;
            else return left.Host == right.Host && left.Port == right.Port;
        }
        public static bool operator !=(ServerData? left, ServerData? right)
            => !(left == right);

        public bool Equals(ServerData? other) => this == other;
    }
}
