using CSharp_Interactive_Learning_App.ViewModels;

namespace CSharp_Interactive_Learning_App
{
    public partial class SplashScreen : ContentPage
    {
        UserViewModel ViewModel { get; set; }
        public SplashScreen(UserViewModel vm)
        {
            InitializeComponent();
            App.Current?.UserAppTheme = AppTheme.Dark;
            ViewModel = vm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await ViewModel.VerifyUser();
            RenderSplashScreenThenNavigate();
        }

        private void RenderSplashScreenThenNavigate()
        {
            var page = Shell.Current.CurrentPage;
            page.Opacity = 1;
            page.FadeToAsync(0);

            bool isLoggedIn = ViewModel.IsLoggedIn;
            if (isLoggedIn)
                Shell.Current.GoToAsync($"Home", true);
            else
                Shell.Current.GoToAsync($"Login", true);
        }
    }
}
