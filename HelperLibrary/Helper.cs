using HelperLibrary.DAL;
using MailKit.Net.Imap;
using System.Text.RegularExpressions;

namespace HelperLibrary
{
    public static class Helper
    {
        public static Regex EmailPattern { get; }
        public static Regex BracketsEmailPattern { get; }

        static Helper()
        {
            EmailPattern = new(@".*@.*\..*");
            BracketsEmailPattern = new(@"<.*@.*\..*>");
        }
    }
}
