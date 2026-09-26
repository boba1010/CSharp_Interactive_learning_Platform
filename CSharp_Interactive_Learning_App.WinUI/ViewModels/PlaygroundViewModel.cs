using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CSharp_Interactive_Learning_App.WinUI.Helpers;
using CSharp_Interactive_Learning_App.WinUI.Services;
using System.Threading.Tasks;

namespace CSharp_Interactive_Learning_App.WinUI.ViewModels;

public partial class PlaygroundViewModel : BaseViewModel
{
    private readonly ConsoleReader reader;

    [ObservableProperty]
    public partial string Output { get; set; } = null!;
    [ObservableProperty]
    public partial bool IsWaitingForInput { get; set; }
    public string Code { get; set; } = null!;

    private readonly IDispatcherQueue dispatcherQueue;

    public PlaygroundViewModel(IDispatcherQueue dispatcherQueue)
    {
        this.dispatcherQueue = dispatcherQueue;

        reader = new ConsoleReader();
        reader.InputRequested += Reader_InputRequested;
        Console.SetIn(reader);

        Console.SetOut(new ConsoleWriter(text =>
        {
            dispatcherQueue.TryEnqueue(() =>
            {
                Output += text;
            });
        }));
    }

    private void Reader_InputRequested()
    {
        dispatcherQueue.TryEnqueue(() => IsWaitingForInput = true);
    }

    public async Task Submit(string input)
    {
        reader.Submit(input);
        Output += $">> {input}\n";
        IsWaitingForInput = false;
    }

    [RelayCommand]
    public async Task RunAsync()
    {
        await Task.Run(() => CSharp_Friendly_Compiler.CSharp_Friendly_Compiler.CompileAndRun(Code));
    }
}
