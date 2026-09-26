using CSharp_Interactive_Learning_App.WinUI.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace CSharp_Interactive_Learning_App.WinUI.Views;

public sealed partial class ProfilePage : Page
{
    public ProfileViewModel ViewModel { get; set; }
    public ProfilePage()
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<ProfileViewModel>();
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoadProfileAsync();
    }
}
