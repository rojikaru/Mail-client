using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Data;

namespace SMTP_Client.Model
{
    class FileNameValueConverter : IValueConverter
    {
        private const string Ellipsis = "...";
        private const int MaxAcceptibleLength = 20;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string _val)
                return Shorten(new FileInfo(_val));
            else if (value is FileInfo info)
                return Shorten(info);
            else throw new NotImplementedException();

            string Shorten(FileInfo fi)
            {
                if (fi.Name.Length > MaxAcceptibleLength)
                {
                    int l = MaxAcceptibleLength - fi.Extension.Length - Ellipsis.Length;
                    return $"{fi.Name[..l]}{Ellipsis}{fi.Extension}";
                }
                else return fi.Name;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
