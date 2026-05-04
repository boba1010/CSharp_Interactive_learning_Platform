using CommunityToolkit.Maui;
using CSharp_Interactive_Learning_App.Services;
using CSharp_Interactive_Learning_App.ViewModels;
using Microsoft.Extensions.Logging;

namespace CSharp_Interactive_Learning_App
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddSingleton<UserViewModel>();
            builder.Services.AddSingleton<BattleViewModel>();
            builder.Services.AddSingleton<UserService>();
            builder.Services.AddSingleton<BattleService>();
            builder.Services.AddSingleton<HttpClient>();
#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
