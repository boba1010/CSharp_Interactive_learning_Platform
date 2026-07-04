using CSharp_Interactive_Learning_App.WinUI.Views;

namespace CSharp_Interactive_Learning_App.WinUI
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(titleBar);

            App.RootFrame = rootFrame;

            rootFrame.Navigated += OnNavigated;

            rootFrame.Navigate(typeof(HomePage));
        }

        private async void OnNavigated(object? sender, NavigationEventArgs e)
        {
            titleBar.IsBackButtonVisible = rootFrame.CanGoBack;
            titleBar.IsBackButtonEnabled = rootFrame.CanGoBack;
        }

        private async void OnBackRequested(TitleBar sender, object args)
        {
            if (rootFrame.CanGoBack)
                rootFrame.GoBack();
        }
    }
}
