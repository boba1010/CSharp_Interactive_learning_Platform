using CSharp_Interactive_Learning_App.WinUI.Views;

namespace CSharp_Interactive_Learning_App.WinUI.Services
{
    public class NavigationService
    {
        public void NavigateTo<TPage>(object? parameter = null) where TPage : Page
        {
            App.RootFrame.Navigate(typeof(TPage), parameter);

            if (typeof(TPage) != typeof(HomePage))
            {
                App.RootFrame.BackStack.Clear();
                App.RootFrame.BackStack.Add(new PageStackEntry(typeof(HomePage), null, null));
            }
        }

        public void GoBack()
        {
            if (App.RootFrame.CanGoBack)
                App.RootFrame.GoBack();
        }
    }
}
