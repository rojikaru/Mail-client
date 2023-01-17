using Microsoft.Web.WebView2.WinForms;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMAP_Client.ViewModel
{
    public class ViewEmailWindowViewModel
    {
        public WebView2 Browser { get; }
        public MimeMessage? Message { get; private set; }

        public ViewEmailWindowViewModel()
        {
            Browser = new();
        }
        public void Load(MimeMessage msg)
        {
            Message = msg;
            Browser.NavigateToString(msg.HtmlBody);
        }
    }
}
