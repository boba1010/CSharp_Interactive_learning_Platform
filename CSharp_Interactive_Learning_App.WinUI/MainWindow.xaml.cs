using CommunityToolkit.Mvvm.Messaging;
using CSharp_Interactive_Learning_App.WinUI.Messages;
using CSharp_Interactive_Learning_App.WinUI.ViewModels;
using CSharp_Interactive_Learning_App.WinUI.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;

namespace CSharp_Interactive_Learning_App.WinUI
{
    public sealed partial class MainWindow : Window
    {
        public BaseViewModel? ViewModel { get; set; }

        private readonly SemaphoreSlim _dialogSemaphore = new(1, 1);

        public MainWindow()
        {
            InitializeComponent();

            ViewModel = App.Services.GetService<BaseViewModel>();

            AppWindow.TitleBar.PreferredTheme = Microsoft.UI.Windowing.TitleBarTheme.Dark;

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(titleBar);

            var homeItem = (NavigationViewItem)navBar.MenuItems[0];

            homeItem.IsSelected = true;

            rootFrame.Navigate(typeof(HomePage));

            WeakReferenceMessenger.Default.Register<BusyStateChangedMessage>(this, (_, message) =>
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    if (message.IsBusy)
                    {
                        titleBar.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        Type currentPage = rootFrame.CurrentSourcePageType;
                        
                        titleBar.Visibility = Visibility.Visible;

                        if (currentPage == typeof(LoginPage) || currentPage == typeof(SignupPage))
                            navBar.Visibility = Visibility.Collapsed;
                    }
                });
            });

            WeakReferenceMessenger.Default.Register<NavigationMessage>(this, (_, message) =>
            {
                if (message.GoBack && rootFrame.CanGoBack)
                {
                    rootFrame.GoBack();
                    return;
                }

                rootFrame.Navigate(message.TargetPage, message.Parameter);
            });



            WeakReferenceMessenger.Default.Register<ShowDialogMessage>(this, async (_, message) =>
            {
                await _dialogSemaphore.WaitAsync();

                try
                {
                    var dialog = new ContentDialog
                    {
                        Title = message.Title,
                        Content = message.Content,
                        PrimaryButtonText = message.PrimaryText,
                        SecondaryButtonText = message.SecondaryText,
                        XamlRoot = root.XamlRoot
                    };

                    await dialog.ShowAsync();
                }
                finally
                {
                    _dialogSemaphore.Release();
                }
            });

            WeakReferenceMessenger.Default.Register<NavbarMessage>(this, async (_, message) =>
            {
                navBar.Visibility = message.Hide ? Visibility.Collapsed : Visibility.Visible;
            });

            WeakReferenceMessenger.Default.Register<InfoBarMessage>(this, async (_, message) =>
            {
                infoBar.Title = message.Title;
                infoBar.Message = message.Message;

                if (message.IsWarning)
                    infoBar.Severity = InfoBarSeverity.Warning;
                if (message.IsError)
                    infoBar.Severity = InfoBarSeverity.Error;

                infoBar.IsOpen = true;
            });

            WeakReferenceMessenger.Default.Register<TeachingTipMessage>(this, async (_, message) =>
            {
                teachingTip.Title = message.Title;
                teachingTip.Content = message.Message;
                teachingTip.IsOpen = true;
            });
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var item = (NavigationViewItem)args.SelectedItem;

            switch (item.Tag)
            {
                case "Home":
                    if (rootFrame.CurrentSourcePageType == typeof(HomePage))
                        return;
                    rootFrame.Navigate(typeof(HomePage));
                    break;
                case "Leaderboard":
                    if (rootFrame.CurrentSourcePageType == typeof(LeaderboardPage))
                        return;
                    rootFrame.Navigate(typeof(LeaderboardPage));
                    break;
                case "Profile":
                    if (rootFrame.CurrentSourcePageType == typeof(ProfilePage))
                        return;
                    rootFrame.Navigate(typeof(ProfilePage));
                    break;
                case "Achievements":
                    if (rootFrame.CurrentSourcePageType == typeof(AchievementsPage))
                        return;
                    rootFrame.Navigate(typeof(AchievementsPage));
                    break;
                case "PlayGround":
                    if (rootFrame.CurrentSourcePageType == typeof(PlayGroundPage))
                        return;
                    rootFrame.Navigate(typeof(PlayGroundPage));
                    break;
            }
        }
    }
}
