using HelperLibrary.DAL;
using MailKit.Net.Imap;
using System.Text.RegularExpressions;

namespace HelperLibrary
{
    public static class Helper
    {
        public static Regex EmailPattern { get; }

        static Helper()
        {
            EmailPattern = new(@".*@.*\..*");

        }
    }
}
