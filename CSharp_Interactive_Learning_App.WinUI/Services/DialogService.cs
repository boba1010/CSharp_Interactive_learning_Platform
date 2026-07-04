using System.Threading.Tasks;

namespace CSharp_Interactive_Learning_App.WinUI.Services
{
    public static class DialogService
    {
        public static async Task ShowAlertAsync(string title, string content, string closeButtonText = "OK")
        {
            XamlRoot xamlRoot = null!;

            if (xamlRoot == null && Application.Current is App currentApp && currentApp.Window != null)
            {
                xamlRoot = currentApp.Window.Content.XamlRoot;
            }
            if (xamlRoot == null)
            {
                throw new InvalidOperationException("No valid XamlRoot found to display the dialog.");
            }

            var dialog = new ContentDialog()
            {
                Title = title,
                Content = content,
                CloseButtonText = closeButtonText,
                XamlRoot = xamlRoot
            };

            await dialog.ShowAsync();
        }
    }
}
