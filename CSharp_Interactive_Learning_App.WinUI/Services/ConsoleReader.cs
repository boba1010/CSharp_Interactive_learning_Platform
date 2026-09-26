using System.Threading.Channels;

namespace CSharp_Interactive_Learning_App.WinUI.Services;

public sealed class ConsoleReader : TextReader
{
    private readonly Channel<string> _input = Channel.CreateUnbounded<string>();

    public event Action? InputRequested;

    public void Submit(string input)
    {
        _input.Writer.TryWrite(input);
    }

    public override string? ReadLine()
    {
        InputRequested?.Invoke();
        return _input.Reader.ReadAsync().AsTask().GetAwaiter().GetResult();
    }
}
