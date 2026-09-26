using CommunityToolkit.Mvvm.Messaging;
using CSharp_Interactive_Learning_App.Shared.Models;
using CSharp_Interactive_Learning_App.Shared.Services;
using CSharp_Interactive_Learning_App.WinUI.Messages;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace CSharp_Interactive_Learning_App.WinUI.ViewModels;

public partial class LeaderboardViewModel(ILeaderboardService leaderboard) : BaseViewModel
{
    public ObservableCollection<UserRank> Ranks { get; } = [];

    public async Task LoadLeadeboardAsync()
    {
        IsBusy = true;

        var result = await leaderboard.GetLeaderboardAsync();

        if (!result.IsSuccess)
        {
            IsBusy = false;
            WeakReferenceMessenger.Default.Send(new InfoBarMessage("Error", result.ErrorMessage!, IsError: true));
            return;
        }

        var ranks = result.Data;
        for (int i = 0; i < ranks?.Count; i++)
        {
            var rank = ranks[i];
            Ranks.Add(rank);
        }

        IsBusy = false;
    }
}
