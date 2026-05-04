using CSharp_Interactive_Learning_App.ViewModels;

namespace CSharp_Interactive_Learning_App.Views.UserViews;

public partial class SignupPage : ContentPage
{
	bool isPasswordVisible = false;

    UserViewModel ViewModel { get; set; }
	public SignupPage(UserViewModel vm)
	{
		InitializeComponent();
		ViewModel = vm;
	}

	private async void OnSignupButtonClicked(object? sender, EventArgs e)
	{
        errorLabel.IsVisible = false;
        var username = usernameEntry.Text;
		var fullName = fullNameEntry.Text;
        var email = emailEntry.Text;
		var password = passwordEntry.Text;

		if (fullName == null)
		{
            errorLabel.IsVisible = true;
            errorLabel.Text = "The fullName field is required";
			return;
        }
		if (username == null)
		{
            errorLabel.IsVisible = true;
            errorLabel.Text = "The username field is required";
			return;
        }
		if (username.Length < 8)
		{
            errorLabel.IsVisible = true;
            errorLabel.Text = "The username should be eight characters or more.";
            return;
        }
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

		await ViewModel.SignupAsync(fullName, username, email, password);
		string msg = Preferences.Get("ErrorMsg", null);
		if (!string.IsNullOrEmpty(msg))
		{
			errorLabel.IsVisible = true;
			errorLabel.Text = msg;
		}
	}

	private async void OnLoginButtonClicked(object? sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("Login");
	}

    private async void OnPasswordShowButtonClicked(object? sender, EventArgs e)
    {
        isPasswordVisible = !isPasswordVisible;
        passwordEntry.IsPassword = isPasswordVisible;
        passwordShowButton.Text = isPasswordVisible ? "Show" : "Hide";

    }
}