using CSharp_Interactive_Learning_App.WinUI.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace CSharp_Interactive_Learning_App.WinUI.Views
{
    public sealed partial class LoginPage : Page
    {
        public LoginViewModel ViewModel { get; set; }
        public LoginPage()
        {
            InitializeComponent();
            ViewModel = App.Services.GetRequiredService<LoginViewModel>();
        }

        private void OnCreateAccountButtonClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SignupPage));
        }

        private void OnEnterPlaygroundClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PlayGroundPage));
        }
    }
}
