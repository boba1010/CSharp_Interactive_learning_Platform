using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CSharp_Interactive_Learning_App.Shared.Models;
using CSharp_Interactive_Learning_App.Shared.Services;
using CSharp_Interactive_Learning_App.WinUI.Messages;
using System.Threading.Tasks;

namespace CSharp_Interactive_Learning_App.WinUI.ViewModels;

public partial class ProfileViewModel(IUserService userService) : BaseViewModel
{
    [ObservableProperty]
    public partial User User { get; set; }

    [ObservableProperty]
    public partial string UserInfo { get; set; } = "";

    public async Task LoadProfileAsync()
    {
        if (IsBusy)
            return;

        IsBusy = true;

        var result = await userService.GetUserDataAsync();
        if (!result.IsSuccess)
        {
            IsBusy = false;
            WeakReferenceMessenger.Default.Send(new InfoBarMessage("Error", result.ErrorMessage!, IsError: true));
            return;
        }

        User = result.Data!;

        UserInfo = $"@{User.Username} • {User.Email}";

        IsBusy = false;
    }
}
