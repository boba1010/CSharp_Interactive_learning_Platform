using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CSharp_Interactive_Learning_App.WinUI.Services;
using CSharp_Interactive_Learning_App.WinUI.Views;
using System.Threading.Tasks;

namespace CSharp_Interactive_Learning_App.WinUI.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial bool IsBusy { get; set; }

        [ObservableProperty]
        public partial bool IsNotBusy { get; set; }

        partial void OnIsBusyChanged(bool value)
        {
            IsNotBusy = !value;
            WeakReferenceMessenger.Default.Send(new BusyStateChangedMessage(value));
        }
    }

    public record BusyStateChangedMessage(bool IsBusy);
}
