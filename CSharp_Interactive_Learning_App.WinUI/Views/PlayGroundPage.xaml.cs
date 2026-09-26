using CSharp_Interactive_Learning_App.WinUI.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace CSharp_Interactive_Learning_App.WinUI.Views;

public sealed partial class PlayGroundPage : Page
{
    public PlaygroundViewModel ViewModel { get; }
    
    public PlayGroundPage()
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<PlaygroundViewModel>();
    }

    private async void Console_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            await ViewModel.Submit(consoleInput.Text);
            consoleInput.Text = "";
        }
    }

    private void CodeEditorControl_Loaded(object sender, RoutedEventArgs e)
    {
        codeEditor.Editor.Modified += Editor_Modified;

        codeEditor.Editor.SetText(@"using System;

public class Program
{
    public static void Main()
    {
        Console.WriteLine(""Hello, world!"");
    }
}");
    }

    private void Editor_Modified(WinUIEditor.Editor sender, WinUIEditor.ModifiedEventArgs args)
    {
        var length = codeEditor.Editor.TextLength;
        var text = codeEditor.Editor.GetText(length);
        ViewModel.Code = text;
    }
}
