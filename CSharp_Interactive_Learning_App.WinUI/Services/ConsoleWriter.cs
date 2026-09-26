using System.Text;

namespace CSharp_Interactive_Learning_App.WinUI.Services;

public sealed class ConsoleWriter(Action<string> write) : TextWriter
{
    private readonly Action<string> _write = write;

    public override Encoding Encoding => Encoding.UTF8;
    public override void Write(char value) => _write.Invoke(value.ToString());
    public override void Write(string? value) => _write(value ?? string.Empty);
    public override void WriteLine(string? value) => _write((value ?? string.Empty) + Environment.NewLine);
}
