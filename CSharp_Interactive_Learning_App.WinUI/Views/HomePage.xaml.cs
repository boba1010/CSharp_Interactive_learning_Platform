using CSharp_Interactive_Learning_App.Shared.Models;
using CSharp_Interactive_Learning_App.WinUI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;

namespace CSharp_Interactive_Learning_App.WinUI.Views
{
    public sealed partial class HomePage : Page
    {
        public HomeViewModel? ViewModel { get; set; }
        public HomePage()
        {
            InitializeComponent();
            ViewModel = App.Services.GetService<HomeViewModel>();

            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null)
                return;

            await ViewModel.VerifyUser();
            if (!ViewModel.IsLoggedIn)
            {
                Frame.Navigate(typeof(LoginPage));
                return;
            }

            await ViewModel.LoadChapters();
        }

        private async void OnBattleButtonClicked(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null)
                return;

            Button button = (Button)sender;

            var param = button.CommandParameter;

            if (param is Battle battle)
                await ViewModel.LoadBattleAsync(battle);
        }
    }
}
