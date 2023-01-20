using Login.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace Login.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is not LoginWindowViewModel vm) return;
            vm.Pwd = ((PasswordBox)sender).SecurePassword;
        }
    }
}
