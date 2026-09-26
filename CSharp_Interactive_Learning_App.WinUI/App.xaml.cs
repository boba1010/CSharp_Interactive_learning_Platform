using CSharp_Interactive_Learning_App.Shared.Services;
using CSharp_Interactive_Learning_App.WinUI.Helpers;
using CSharp_Interactive_Learning_App.WinUI.Services;
using CSharp_Interactive_Learning_App.WinUI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;

namespace CSharp_Interactive_Learning_App.WinUI
{
    public partial class App : Application
    {
        private Window? _window;

        public static IServiceProvider Services { get; set; } = null!;

        public App()
        {
            InitializeComponent();

            var services = new ServiceCollection();
            ConfigureServices(services);
            Services = services.BuildServiceProvider();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            services.AddSingleton<IBattleService, BattleService>();
            services.AddSingleton<IAuthService, AuthService>();
            services.AddSingleton<IUserService, UserService>();
            services.AddSingleton<ITokenService, TokenService>();
            services.AddSingleton<ISecureStorage, SecureStorage>();
            services.AddSingleton<ILeaderboardService, LeaderboardService>();
            services.AddSingleton<IAchivementsService, AchivementsService>();
            services.AddSingleton<IDispatcherQueue, DispatcherQueue>();
            services.AddSingleton(Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread());
            services.AddTransient<BaseViewModel>();
            services.AddTransient<HomeViewModel>();
            services.AddTransient<ProfileViewModel>();
            services.AddTransient<SignupViewModel>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<BattleViewModel>();
            services.AddTransient<LeaderboardViewModel>();
            services.AddTransient<PlaygroundViewModel>();
            services.AddTransient<AchievementsViewModel>();
            services.AddSingleton<HttpClient>();
            services.AddSingleton<ApiClient>();
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            _window = new MainWindow();
            _window.Activate();
        }
    }
}
