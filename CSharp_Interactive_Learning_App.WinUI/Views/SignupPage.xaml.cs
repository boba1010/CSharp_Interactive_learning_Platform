using CSharp_Interactive_Learning_App.WinUI.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace CSharp_Interactive_Learning_App.WinUI.Views
{
    public sealed partial class SignupPage : Page
    {
        public SignupViewModel ViewModel { get; set; }
        public SignupPage()
        {
            InitializeComponent();
            ViewModel = App.Services.GetRequiredService<SignupViewModel>();
        }

        private async void OnLoginButtonClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(LoginPage));
        }

        private void OnEnterPlaygroundClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PlayGroundPage));
        }
    }
}
