using HelperLibrary.DAL;
using MailKit.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Login.Model
{
    internal static class Datarepo
    {
        public static ServerData[] ServerDatas { get; }
        public static ServerCredentials[] ServerCreds { get;}

        static Datarepo()
        {
            ServerDatas = new ServerData[]
            {
                ServerData.None,
                new ServerData("smtp.gmail.com", 587, SecureSocketOptions.StartTls),
                new ServerData("imap.gmail.com", 993, SecureSocketOptions.SslOnConnect),
                new ServerData("smtp.mail.yahoo.com", 465, SecureSocketOptions.SslOnConnect),
                new ServerData("imap.mail.yahoo.com", 993, SecureSocketOptions.SslOnConnect),
                new ServerData("smtp-mail.outlook.com", 587, SecureSocketOptions.StartTls),
                new ServerData("outlook.office365.com", 993, SecureSocketOptions.StartTls),
                new ServerData("smtp.mail.me.com", 587, SecureSocketOptions.StartTls),
                new ServerData("imap.mail.me.com", 993, SecureSocketOptions.StartTls),
                new ServerData("smtp.ukr.net", 465, SecureSocketOptions.SslOnConnect),
                new ServerData("imap.ukr.net", 993, SecureSocketOptions.SslOnConnect),

            };
            ServerCreds = new ServerCredentials[]
            {
                new ServerCredentials(ServerDatas[1], ServerDatas[2], "https://www.shareicon.net/data/20x20/2016/07/10/119930_google_512x512.png", "Google"),
                new ServerCredentials(ServerDatas[3], ServerDatas[4], "https://cdn-icons-png.flaticon.com/32/3670/3670263.png", "Yahoo"),
                new ServerCredentials(ServerDatas[5], ServerDatas[6], "https://cdn-icons-png.flaticon.com/32/732/732223.png", "Outlook"),
                new ServerCredentials(ServerDatas[7], ServerDatas[8], "https://cdn-icons-png.flaticon.com/32/888/888860.png", "iCloud"),
                new ServerCredentials(ServerDatas[9], ServerDatas[10], "https://upst.fwdcdn.com/ukrnet-icon-57x57.png", "Ukr.net"),
                new ServerCredentials(ServerDatas[0], ServerDatas[0], "https://cdn-icons-png.flaticon.com/32/3178/3178158.png", "Custom server"),
            };
        }
    }
}
