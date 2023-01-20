using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace HelperLibrary.DAL
{
    public class UserCredentials
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)] public int ID { get; set; }
        [Required] public string UserName { get; set; }
        [Required] public ServerCredentials Servers { get; set; }

        public UserCredentials()
        {
            UserName = string.Empty;
            Servers = ServerCredentials.None;
        }

        public override string ToString() => UserName;

        private const string smtpfaulted = "Provided SMTP server parameters are not valid";
        private const string imapfaulted = "Provided IMAP server parameters are not valid";
        private const string authfaulted = "Username or password is not correct";

        public static async Task TryAuth(UserCredentials credentials, SecureString pwd, bool Login = true, bool Smtp = true, bool Imap = true)
        {
            ServerData data;
            if (Smtp)
            {
                SmtpClient client = new();
                data = credentials.Servers.SmtpServer!;
                try
                {
                    await client.ConnectAsync(data.Host, data.Port, data.SecurityOptions);
                }
                catch
                {
                    throw new MailServerException(smtpfaulted);
                }
                finally
                {
                    client.Disconnect(true);
                }
            }
            if (Imap)
            {
                ImapClient client = new();
                data = credentials.Servers.ImapServer!;

                try
                {
                    await client.ConnectAsync(data.Host, data.Port, data.SecurityOptions);
                }
                catch
                {
                    client.Disconnect(true);
                    throw new MailServerException(imapfaulted);
                }

                try
                {
                    if (Login) 
                        await client.AuthenticateAsync(new NetworkCredential(credentials.UserName, pwd));
                }
                catch
                {
                    throw new UserCredentialsException(authfaulted);
                }
                finally
                {
                    client.Disconnect(true);
                }
            }
        }
    }
}
