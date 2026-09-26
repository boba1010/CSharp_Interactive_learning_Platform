using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CSharp_Interactive_Learning_App.Shared.Contracts.Requests;
using CSharp_Interactive_Learning_App.Shared.Models;
using CSharp_Interactive_Learning_App.Shared.Services;
using CSharp_Interactive_Learning_App.WinUI.Messages;
using CSharp_Interactive_Learning_App.WinUI.Views;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace CSharp_Interactive_Learning_App.WinUI.ViewModels
{
    public partial class HomeViewModel(IBattleService battleService, IAuthService authService) : BaseViewModel
    {
        public ObservableCollection<Chapter> Chapters { get; set; } = [];
        public bool IsLoggedIn { get; set; }

        public async Task LoadChapters()
        {
            try
            {
                if (IsBusy)
                    return;

                IsBusy = true;

                var response = await battleService.GetAllChaptersAsync();
                if (!response.IsSuccess)
                {
                    IsBusy = false;
                    WeakReferenceMessenger.Default.Send(new ShowDialogMessage("Error", response?.ErrorMessage, "OK", null));
                    return;
                }

                var chapters = response.Data!;
                foreach (var chapter in chapters)
                    Chapters.Add(chapter);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task VerifyUser()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            var response = await authService.VerifyUserAsync();

            IsLoggedIn = response.Data;

            IsBusy = false;
        }

        [RelayCommand]
        public async Task LoadBattleAsync(Battle battle)
        {
            WeakReferenceMessenger.Default.Send(
                new NavigationMessage(false, typeof(BattlePage), new RequestBattleStart { BattleId = battle.Id, ChapterId = battle.ChapterId }));
        }
    }
}
