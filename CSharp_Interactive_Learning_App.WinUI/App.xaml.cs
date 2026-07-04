using CSharp_Interactive_Learning_App.Shared.Services;
using CSharp_Interactive_Learning_App.WinUI.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;

namespace CSharp_Interactive_Learning_App.WinUI
{
    public partial class App : Application
    {
        public Window? Window { get; set; }

        public static Frame RootFrame { get; set; }

        public static IServiceProvider Services { get; set; }

        public App()
        {
            InitializeComponent();

            var services = new ServiceCollection();

            ConfigureServices(services);

            Services = services.BuildServiceProvider();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            services.AddSingleton<NavigationService>();
            services.AddSingleton<IBattleService, BattleService>();
            services.AddSingleton<IAuthService, AuthService>();
            services.AddSingleton<ITokenService, TokenService>();
            services.AddSingleton<ISecureStorage, SecureStorage>();
            services.AddSingleton<ApiClient>();
            services.AddSingleton<HttpClient>();
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            Window = new MainWindow();
            Window.Activate();
        }
    }
}
