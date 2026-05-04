using CSharp_Interactive_Learning_App.ViewModels;

namespace CSharp_Interactive_Learning_App.Views.UserViews;

public partial class LoginPage : ContentPage
{
	bool isPasswordVisible = false;
    UserViewModel ViewModel { get; set; }    

	public LoginPage(UserViewModel vm)
	{
		InitializeComponent();
        ViewModel = vm;
	}

    private async void OnLoginButtonClicked(object? sender, EventArgs e)
    {
        errorLabel.IsVisible = false;

        var email = emailEntry.Text;
        var password = passwordEntry.Text;

        if (email == null)
        {
            errorLabel.IsVisible = true;
            errorLabel.Text = "The email field is required";
            return;
        }
        if (password == null)
        {
            errorLabel.IsVisible = true;
            errorLabel.Text = "The password field is required";
            return;
        }

        await ViewModel.LoginAsync(email, password);

        string msg = Preferences.Get("ErrorMsg", null);
        if (!string.IsNullOrEmpty(msg))
        {
            errorLabel.IsVisible = true;
            errorLabel.Text = msg;
        }
    }

    private async void OnSignupButtonClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("Signup");
    }

    private async void OnPasswordShowButtonClicked(object? sender, EventArgs e)
    {
        isPasswordVisible = !isPasswordVisible;
        passwordEntry.IsPassword = isPasswordVisible;
        passwordShowButton.Text = isPasswordVisible ? "Show" : "Hide";

    }
}